using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Complex = System.Numerics.Complex;

public class BaseComplexFunc : MonoBehaviour
{

    public Vector2 mul(Vector2 z, Vector2 w)
    {
        return new Vector2(z.x * w.x - z.y * w.y, z.x * w.y + z.y * w.x);
    }

    public Complex V2C(Vector2 v)
    {
        return new Complex(v.x, v.y);
    }

    public Vector2 C2V(Complex z)
    {
        return new Vector2((float)z.Real, (float)z.Imaginary);
    }

    public virtual Vector2 f(Vector2 z)
    {
        return Vector2.zero;
    }
}
