using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Complex = System.Numerics.Complex;

public class Quadratic : BaseComplexFunc
{
    public Vector2 a = new Vector2(1, 0);
    public Vector2 b = new Vector2(0, 0);
    public Vector2 c = new Vector2(0, 0);

    public override Vector2 f(Vector2 z)
    {
        Complex x = V2C(z);
        x = (V2C(a) * x + V2C(b)) * x + V2C(c);

        return C2V(x);
        //return mul(mul(a, z) + b, z) + c;
    }
}
