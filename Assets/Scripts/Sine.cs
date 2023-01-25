using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Complex = System.Numerics.Complex;

public class Sine : BaseComplexFunc
{
    public override Vector2 f(Vector2 z)
    {
        Complex x = V2C(z);

        return C2V(System.Numerics.Complex.Sin(x));
    }
}
