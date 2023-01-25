using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Complex = System.Numerics.Complex;

public class ComplexFuncGraph : MonoBehaviour
{
    public BaseComplexFunc func = null;
    public Vector2 x = new Vector2(-2, 2);
    public Vector2 y = new Vector2(-2, 2);

    public bool cropRange = true;
    public Vector2 zRange = new Vector2(-2, 2);
    public Vector2 wRange = new Vector2(-2, 2);

    public Vector2Int size = new Vector2Int(200, 200);

    Mesh CreateMesh()
    {
        int N = (size.x + 1) * (size.y + 1);

        Vector3[] vertices = new Vector3[N];
        Color[] colors = new Color[N];
        Vector2[] uvs = new Vector2[N];

        int[] triangles = new int[size.x*size.y*2*3];

        double dx = ((double) (x.y - x.x)) / size.x;
        double dy = ((double) (y.y - y.x)) / size.y;

        int k = 0, k1 = 0;
        for (int j=0; j<= size.y; j++)
        {
            for (int i=0; i<= size.x; i++)
            {
                Vector2 z = new Vector2((float) (i * dx + x.x), (float) (j * dy + y.x));
                Vector2 w = func.f(z);
                if (cropRange && ((w.x < zRange.x) || (w.x > zRange.y) || (w.y < wRange.x) || (w.y > wRange.y)) )
                {
                    vertices[k] = new Vector3(float.NaN, float.NaN, float.NaN);
                }
                else
                {
                    vertices[k] = new Vector3(z.x, w.x, z.y);
                }
                colors[k] = new Color(((float)i) / size.x, ((float)j) / size.y, 0.8f);
                uvs[k] = new Vector2(w.y, 0);

                if ((j < size.y) && (i < size.x))
                {
                    triangles[k1 * 6] = k;
                    triangles[k1 * 6 + 1] = k + 1;
                    triangles[k1 * 6 + 2] = k + size.x+1;
                    triangles[k1 * 6 + 3] = k + size.x + 1;
                    triangles[k1 * 6 + 4] = k + 1;
                    triangles[k1 * 6 + 5] = k + size.x + 2;
                    k1++;
                }
                
                k++;
            }
        }

        var mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // for more than 65535 vertices
        mesh.vertices = vertices;
        mesh.uv2 = uvs;
        mesh.colors = colors;
        mesh.triangles = triangles;
        return mesh;
    }

    // Start is called before the first frame update
    void Start()
    {
        var mesh = CreateMesh();
        gameObject.GetComponent<MeshFilter>().mesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            var mesh = CreateMesh();
            gameObject.GetComponent<MeshFilter>().mesh = mesh;
        } else
        {
            // reduce the size of the scene
            if (GetComponent<MeshFilter>())
            {
                GetComponent<MeshFilter>().mesh = null;
            }
        }
    }
}
