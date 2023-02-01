using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class Manipulator4D : MonoBehaviour
{
    // Controller rotation & position are get from here
    // (button actions are obtained by UnityEngine.XR.InputDevice)
    [SerializeField] GameObject leftController;
    [SerializeField] GameObject rightController;

    Matrix4x4 _rotation = Matrix4x4.identity;

    public bool initialized = false;
    Matrix4x4 initRot = Matrix4x4.identity;
    Vector3 initPos = Vector3.zero;

    bool oldTriggerValue = false;
    bool oldSecondaryButtonValue = false;
    bool oldPrimary2DAxisClickValue = false;
    bool oldPrimaryButtonValue = false;

    public bool isManipulatable = true;

    public enum ProjectionType
    {
        One = 1,
        Three = 3,
        Four = 4
    }
    public ProjectionType projectionType = ProjectionType.Four;
    // public bool showOnlyOneAxisType = false; // obsolete
    public bool allowChangeManipulationTypeByController = false;

    [SerializeField] GameObject XYZ;
    [SerializeField] GameObject YZW;
    [SerializeField] GameObject ZWX;
    [SerializeField] GameObject WXY;

    GameObject[] projections = new GameObject[4];

    [SerializeField] Text text_XYZ;
    [SerializeField] Text text_YZW;
    [SerializeField] Text text_ZWX;
    [SerializeField] Text text_WXY;

    Text[] texts = new Text[4];

    [SerializeField] Text text_manipulationType;

    [SerializeField] Color textColor = Color.white;
    [SerializeField] Color highlightTextColor = Color.red;

    [SerializeField] bool assignRandomMatrix = false;
    public bool chiral = false;
    public bool answer = false;
    [SerializeField] bool randomChirality = false;
    [SerializeField] bool changeChiralityByController = false;
    [SerializeField] bool enable3DTranslationFor3DRotation = false;
    [SerializeField] List<GameObject> translationObjects = new List<GameObject>();
    [SerializeField] bool showAnswerByController = false;
    Dictionary<GameObject, Vector3> initialObjectPosition = new Dictionary<GameObject, Vector3>();

    public Matrix4x4 rotation
    {
        get { return _rotation; }
        set { _rotation = value; }
    }

    public enum ManipulationType
    {
        ThreeD = 0,
        ThreeD_plus_Translation = 1,
        TwoHands_by_Quaternion = 2,
        SL2 = 3
    }

    public ManipulationType manipulationType = ManipulationType.ThreeD;

    public enum AxisType
    {
        XYZ = 0,
        YZW = 1,
        ZWX = 2,
        WXY = 3
    }

    public bool changeAxisTypeByController = true;
    public AxisType axisType = AxisType.XYZ;

    AxisType initAxisType = AxisType.XYZ;

    public static readonly Matrix4x4[] permutationMatrices =
    {
        Matrix4x4.identity,
        new Matrix4x4(new Vector4(0, 0, 0, -1),
                      new Vector4(1, 0, 0, 0),
                      new Vector4(0, 1, 0, 0),
                      new Vector4(0, 0, 1, 0)),
        new Matrix4x4(new Vector4(0, 0, 1, 0),
                      new Vector4(0, 0, 0, 1),
                      new Vector4(1, 0, 0, 0),
                      new Vector4(0, 1, 0, 0)),
        new Matrix4x4(new Vector4(0, 1, 0, 0),
                      new Vector4(0, 0, 1, 0),
                      new Vector4(0, 0, 0, -1),
                      new Vector4(1, 0, 0, 0))
    };

    static readonly string[] manipulationTypeText =
        { "3D rotation", "3D rotation + 4D rotation by translation", "quaternion action by two hands", "SL2 action" };

    // 3D rotation
    Matrix4x4 CalcRotation3D(Quaternion q)
    {
        return Matrix4x4.TRS(Vector3.zero, q, Vector3.one);
    }

    Matrix4x4 CalcRotation4DByTranslation(Vector3 v)
    {
        float t = Vector3.Magnitude(v);

        if (t == 0)
        {
            return Matrix4x4.identity;
        }

        t *= 2 * Mathf.PI;
        float s = Mathf.Sin(t), c = Mathf.Cos(t);

        Vector3 n = Vector3.Normalize(v);

        float d = 1 - c;
        // Note that:
        // - the inverse of a permutation matrix is equal to the transpose
        // - matrices are column major (consists of 4 columns, so looks transposed)
        // - coordinate: (x,y,z,w)
        return new Matrix4x4(new Vector4(1 - n.x * n.x * d, -n.x * n.y * d, -n.x * n.z * d, n.x * s),
                                    new Vector4(-n.y * n.x * d, 1 - n.y * n.y * d, -n.y * n.z * d, n.y * s),
                                    new Vector4(-n.z * n.x * d, -n.z * n.y * d, 1 - n.z * n.z * d, n.z * s),
                                    new Vector4(-n.x * s, -n.y * s, -n.z * s, c));
    }

    // Returns the 4x4 matrix representing the left multiplication of the given quaternion x -> q*x
    Matrix4x4 LQuat2Mat4(Quaternion q)
    {
        return new Matrix4x4(new Vector4(q.w, q.z, -q.y, -q.x),
                             new Vector4(-q.z, q.w, q.x, -q.y),
                             new Vector4(q.y, -q.x, q.w, -q.z),
                             new Vector4(q.x, q.y, q.z, q.w));
    }

    // Returns the 4x4 matrix representing the right multiplication of the given quaternion x -> x*q
    Matrix4x4 RQuat2Mat4(Quaternion q)
    {
        return new Matrix4x4(new Vector4(q.w, -q.z, q.y, -q.x),
                             new Vector4(q.z, q.w, -q.x, -q.y),
                             new Vector4(-q.y, q.x, q.w, -q.z),
                             new Vector4(q.x, q.y, q.z, q.w));
    }

    // Returns the 4x4 matrix representing x -> ql * x * conj(qr)
    Matrix4x4 LRQuat2Mat4(Quaternion ql, Quaternion qr)
    {
        return LQuat2Mat4(ql) * RQuat2Mat4(Quaternion.Inverse(qr));
    }

    Matrix4x4 Quat2SL2(Quaternion q)
    {
        return LQuat2Mat4(q * q);
    }

    Matrix4x4 CalcRotationWithoutPermutation(Vector3 v)
    {
        switch (manipulationType)
        {
            case ManipulationType.ThreeD:
                return CalcRotation3D(leftController.transform.rotation);
            case ManipulationType.ThreeD_plus_Translation:
                return CalcRotation3D(leftController.transform.rotation) * CalcRotation4DByTranslation(v);
            case ManipulationType.TwoHands_by_Quaternion:
                return LRQuat2Mat4(leftController.transform.rotation, rightController.transform.rotation);
            case ManipulationType.SL2:
                return Quat2SL2(leftController.transform.rotation);
            default:
                return Matrix4x4.identity;
        }

    }

    Matrix4x4 CalcRotation(AxisType at, Vector3 v)
    {
        Matrix4x4 P = permutationMatrices[(int)at];
        return P.transpose * CalcRotationWithoutPermutation(v) * P;
    }

    void SetManipulationTypeText()
    {
        if (text_manipulationType)
        {
            text_manipulationType.text = manipulationTypeText[(int)manipulationType];

            if ((manipulationType == ManipulationType.ThreeD) && enable3DTranslationFor3DRotation)
            {
                text_manipulationType.text += " + 3D translation";
            }
        }
    }

    public void AssignRandomRotation()
    {
        _rotation = Pcx4D.RandomRotation.randomDistributionOnSO4();
    }

    void Awake()
    {
        projections[0] = XYZ;
        projections[1] = YZW;
        projections[2] = ZWX;
        projections[3] = WXY;

        texts[0] = text_XYZ;
        texts[1] = text_YZW;
        texts[2] = text_ZWX;
        texts[3] = text_WXY;

        for (int i = 0; i < 3; i++)
        {
            texts[i].color = textColor;
        }

        if (leftController)
        {
            texts[(int)axisType].color = highlightTextColor;
        }

        if (assignRandomMatrix)
        {
            AssignRandomRotation();
        }

        if (randomChirality)
        {
            chiral = (Random.value > 0.5f) ? true : false;
        }

        SetManipulationTypeText();
    }

    public void SetAxisType(AxisType a)
    {
        axisType = a;
        for (int i=0; i<4; i++)
        {
            texts[i].color = textColor;
        }
        texts[(int)axisType].color = highlightTextColor;
    }

    // Update is called once per frame
    void Update()
    {
        for (int i=0; i<4; i++)
        {
            if (projections[i] != null)
            {
                int pT = (int)projectionType;
                bool b = (pT == 4) || ((pT == 3) ^ ((int)axisType == i));
                projections[i].SetActive(b);
            }
        }

        if (!isManipulatable)
        {
            return;
        }

        // check existence of left handed controller(s)
        var leftHandedControllers = new List<InputDevice>();
        var desiredCharacteristics = InputDeviceCharacteristics.HeldInHand
                                    | InputDeviceCharacteristics.Left
                                    | InputDeviceCharacteristics.Controller;
        InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, leftHandedControllers);

        if (leftHandedControllers.Count > 0)
        {
            // only use the first controller
            var device = leftHandedControllers[0];

            if (leftController)
            {
                // Grip Button: Rotation
                bool gripValue = false;
                if (device.TryGetFeatureValue(CommonUsages.gripButton, out gripValue) && gripValue)
                {
                    // Rotation by (mainly) left Controller
                    if (!initialized)
                    {
                        // Get initial state
                        initialized = true;
                        initAxisType = axisType;
                        initRot = CalcRotation(initAxisType, Vector3.zero).inverse * _rotation;
                        initPos = leftController.transform.position;
                        
                        if ((manipulationType == ManipulationType.ThreeD) && enable3DTranslationFor3DRotation)
                        {
                            initialObjectPosition.Clear();
                            foreach (var obj in translationObjects)
                            {
                                initialObjectPosition.Add(obj, obj.transform.position);
                            }
                        }
                    }
                    else
                    {
                        // Update rotation matrix
                        _rotation = CalcRotation(initAxisType, leftController.transform.position - initPos) * initRot;

                        if ((manipulationType == ManipulationType.ThreeD) && enable3DTranslationFor3DRotation)
                        {
                            // translate 3D position
                            foreach (var obj in translationObjects)
                            {
                                Vector3 value;
                                if (initialObjectPosition.TryGetValue(obj, out value))
                                {
                                    obj.transform.position = value + leftController.transform.position - initPos;
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Grip button is not pressed
                    initialized = false;
                }
            }

            // Trigger button: Change axis type to rotate
            if (changeAxisTypeByController)
            {
                bool triggerValue = false;
                if (device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue) && triggerValue)
                {
                    if (!oldTriggerValue)
                    {
                        // Trigger Button is pressed

                        // Reset text color
                        texts[(int)axisType].color = textColor;

                        axisType++;
                        if ((int)axisType > 3) axisType = 0;

                        if (leftController)
                        {
                            // highlight text color for chosen axis type
                            // (do not highlight if not controlled by controller)
                            texts[(int)axisType].color = highlightTextColor;
                        }
                    }
                }
                oldTriggerValue = triggerValue;
            }

            // Secondary Button (Y-button): Toggle chirality
            if (changeChiralityByController)
            {
                bool secondaryButtonValue = false;
                if (device.TryGetFeatureValue(CommonUsages.secondaryButton, out secondaryButtonValue) && secondaryButtonValue)
                {
                    if (!oldSecondaryButtonValue)
                    {
                        chiral = !chiral;
                    }
                }
                oldSecondaryButtonValue = secondaryButtonValue;
            }

            // Stick pressed: Change manipulation (rotation) type
            bool primary2DAxisClickValue = false;
            if (device.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out primary2DAxisClickValue) && primary2DAxisClickValue)
            {
                if (!oldPrimary2DAxisClickValue&&allowChangeManipulationTypeByController)
                {
                    manipulationType++;

                    if ( (manipulationType == ManipulationType.TwoHands_by_Quaternion) && !(rightController))
                    {
                        manipulationType++;
                    }

                    if ((int)manipulationType > 3)
                    {
                        manipulationType = 0;
                    }

                    SetManipulationTypeText();
                }
            }
            oldPrimary2DAxisClickValue = primary2DAxisClickValue;
        }
    }
}
