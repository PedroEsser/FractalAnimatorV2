using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class Utils
{

    



    public static Matrix4x4 CreateTRS(float tx, float ty, float rotation, float sx, float sy)
    {
        float cos = Mathf.Cos(rotation * Mathf.Deg2Rad);
        float sin = Mathf.Sin(rotation * Mathf.Deg2Rad);

        Matrix4x4 m = Matrix4x4.zero;
        m.m00 = sx * cos;
        m.m01 = sy * -sin;
        m.m02 = tx;
        m.m10 = sx * sin;
        m.m11 = sy * cos;
        m.m12 = ty;
        m.m20 = 0;
        m.m21 = 0;
        m.m22 = 1;
        m.m33 = 1;
        return m;

    }

}
