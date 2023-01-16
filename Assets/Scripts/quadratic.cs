using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class quadratic : MonoBehaviour
{
    public Vector2 a = new Vector2(1, 0);
    public Vector2 b = new Vector2(0, 0);
    public Vector2 c = new Vector2(0, 0);

    public Vector2 x = new Vector2(-2, 2);
    public Vector2 y = new Vector2(-2, 2);

    public Vector2Int size = new Vector2Int(200, 200);

    Vector2 mul(Vector2 z, Vector2 w)
    {
        return new Vector2(z.x * w.x - z.y * w.y, z.x * w.y + z.y * w.x);
    }

    Vector2 f(Vector2 z)
    {
        return mul(mul(a, z) + b, z) + c;
    }

    Mesh CreateMesh()
    {
        int N = (size.x + 1) * (size.y + 1);

        Vector3[] vertices = new Vector3[N];
        Color[] colors = new Color[N];
        Vector2[] uvs = new Vector2[N];

        int[] triangles = new int[size.x*size.y*2*3];

        float dx = (x.y - x.x) / size.x;
        float dy = (y.y - y.x) / size.y;

        int k = 0, k1 = 0;
        for (int j=0; j<= size.y; j++)
        {
            for (int i=0; i<= size.x; i++)
            {
                Vector2 z = new Vector2(i * dx + x.x, j * dy + y.x);
                Vector2 w = f(z);
                vertices[k] = new Vector3(z.x, z.y, w.x);
                colors[k] = new Color(((float)i) / size.x, ((float)j) / size.y, 0.8f);
                uvs[k] = new Vector2(w.y, 0);

                if ((j < size.y) && (i < size.x))
                {
                    triangles[k1 * 6] = k;
                    triangles[k1 * 6 + 1] = k + 1;
                    triangles[k1 * 6 + 2] = k + size.x;
                    triangles[k1 * 6 + 3] = k + 1;
                    triangles[k1 * 6 + 4] = k + size.x;
                    triangles[k1 * 6 + 5] = k + size.x + 1;
                    k1++;
                }
                
                k++;
            }
        }

        var mesh = new Mesh();
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
}
