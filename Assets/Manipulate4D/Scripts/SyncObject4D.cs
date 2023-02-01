using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncObject4D : MonoBehaviour
{
    public int objectID = 0;
    public GameObject[] objCollection;
    public GameObject[] syncObjectList;
    public bool allowChangeObjectByController = true;

    int oldObjectID = -1;

    bool oldPrimaryButtonValue = false;

    // Start is called before the first frame update
    void Start()
    {
        Distribute();
        oldObjectID = objectID;
    }

    // Update is called once per frame
    void Update()
    {
        // get left handed controller(s)
        var leftHandedControllers = new List<UnityEngine.XR.InputDevice>();
        var desiredCharacteristics = UnityEngine.XR.InputDeviceCharacteristics.HeldInHand
                                    | UnityEngine.XR.InputDeviceCharacteristics.Left
                                    | UnityEngine.XR.InputDeviceCharacteristics.Controller;
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, leftHandedControllers);

        if (leftHandedControllers.Count > 0)
        {
            // use only the first controller
            var device = leftHandedControllers[0];

            // Primary button (X-Button): Change object
            bool primaryButtonValue = false;
            if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out primaryButtonValue) && primaryButtonValue)
            {
                if (!oldPrimaryButtonValue&&allowChangeObjectByController)
                {
                    objectID++;
                    Distribute();
                }
            }
            oldPrimaryButtonValue = primaryButtonValue;
        }
        oldObjectID = objectID;
    }

    // Copy (instantiate) object to objCollection
    public void Distribute(bool force = false)
    {
        // check objectID is in the appropriate range
        if (objectID < 0)
        {
            objectID = objCollection.Length-1;
        }

        if (objectID >= objCollection.Length)
        {
            objectID = 0;
        }

        // do nothing if object is not changed
        if (!force && (objectID == oldObjectID)) return;

        GameObject sourceObj = objCollection[objectID];
        if (sourceObj) {
            foreach (var obj in syncObjectList) {
                // Clear children
                for (int i = obj.transform.childCount - 1; i >= 0; --i)
                {
                    var c = obj.transform.GetChild(i).gameObject;
                    Destroy(c);
                }

                // Copy
                var child = Instantiate(sourceObj, obj.transform) as GameObject;
                //child.GetComponent<MeshRenderer>().enabled = true;

                // Distribute render4D to children
                // (note that source object might have children)
                obj.GetComponent<Render4D>().DistributeSelfToChildren();
            }
        }
    }
}
