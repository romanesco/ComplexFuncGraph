using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Render4D : MonoBehaviour
{
    public GameObject manipulator4D;

    public Manipulator4D.AxisType axisType = Manipulator4D.AxisType.XYZ;

    // Update is called once per frame
    void Update()
    {
        if (manipulator4D)
        {
            // Get rotation matrix and chirality from Manipulator4D
            Matrix4x4 A = Manipulator4D.permutationMatrices[(int)axisType];
            A = A * manipulator4D.GetComponent<Manipulator4D>().rotation;

            bool chiral = manipulator4D.GetComponent<Manipulator4D>().chiral;

            var renderer = GetComponent<Renderer>();
            if (renderer)
            {
                renderer.sharedMaterial.SetMatrix("_Rotation4D", A);
                renderer.sharedMaterial.SetFloat("_Chiral", chiral ? 1 : 0);
            }
        }
    }

    // Distribute itself to children recursively
    public void DistributeSelfToChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; --i)
        {
            var child = transform.GetChild(i).gameObject;
            if (!child.GetComponent<Render4D>())
            {
                child.AddComponent<Render4D>();
            }
            Render4D childRender4D = child.GetComponent<Render4D>();
            childRender4D.manipulator4D = manipulator4D;
            childRender4D.axisType = axisType;

            if (child.GetComponent<MeshRenderer>())
            {
                child.GetComponent<MeshRenderer>().enabled = true;
            }

            childRender4D.DistributeSelfToChildren();
        }
    }
}
