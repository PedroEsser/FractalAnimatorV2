using UnityEngine;

public struct QuadFloat
{
    public float a, b, c, d; // high to low parts

    public QuadFloat(float a, float b, float c, float d)
    {
        this.a = a; this.b = b; this.c = c; this.d = d;
    }

    // Construct from a single float (low precision init)
    public QuadFloat(float value)
    {
        a = value;
        b = 0f;
        c = 0f;
        d = 0f;
    }

    // Add two QuadFloats (simple version)
    public static QuadFloat operator +(QuadFloat x, QuadFloat y)
    {
        float s0, e0, s1, e1, s2, e2, s3, e3;
        TwoSum(x.a, y.a, out s0, out e0);
        TwoSum(x.b, y.b + e0, out s1, out e1);
        TwoSum(x.c, y.c + e1, out s2, out e2);
        TwoSum(x.d, y.d + e2, out s3, out e3);
        return Renormalize(s0, s1, s2, s3 + e3);
    }

    public static QuadFloat operator -(QuadFloat x, QuadFloat y)
    {
        return x + y * -1f;
    }

    // Multiply by scalar float (simple version)
    public static QuadFloat operator *(QuadFloat x, float y)
    {
        float p0, e0, p1, e1, p2, e2, p3, e3;
        TwoProduct(x.a, y, out p0, out e0);
        TwoProduct(x.b, y, out p1, out e1);
        TwoProduct(x.c, y, out p2, out e2);
        TwoProduct(x.d, y, out p3, out e3);
        return Renormalize(p0, p1 + e0, p2 + e1, p3 + e2 + e3);
    }

    public static QuadFloat operator *(QuadFloat x, QuadFloat y)
    {
        float p00, e00, p01, e01, p02, e02, p03, e03;
        float p10, e10, p11, e11, p12, e12, p13, e13;

        TwoProduct(x.a, y.a, out p00, out e00);
        TwoProduct(x.a, y.b, out p01, out e01);
        TwoProduct(x.a, y.c, out p02, out e02);
        TwoProduct(x.a, y.d, out p03, out e03);

        TwoProduct(x.b, y.a, out p10, out e10);
        TwoProduct(x.b, y.b, out p11, out e11);
        TwoProduct(x.b, y.c, out p12, out e12);
        TwoProduct(x.b, y.d, out p13, out e13);

        float p20 = x.c * y.a;
        float p21 = x.c * y.b;
        float p22 = x.c * y.c;
        float p23 = x.c * y.d;

        float p30 = x.d * y.a;
        float p31 = x.d * y.b;
        float p32 = x.d * y.c;
        float p33 = x.d * y.d;

        float t0 = p00;
        float t1 = p01 + p10 + e00;
        float t2 = p02 + p11 + p20 + p12 + p21 + e01 + e10;
        float t3 = p03 + p13 + p22 + p30 + p23 + p31 + e02 + e11 + e12 + e13;
        float t4 = p32 + p33 + e03;

        return Renormalize(t0, t1, t2, t3 + t4);
    }

    // Convert to Vector4 to send to shader
    public Vector4 ToVector4() => new Vector4(a, b, c, d);

    // Helpers: TwoSum and TwoProduct
    private static void TwoSum(float a, float b, out float sum, out float err)
    {
        sum = a + b;
        float bb = sum - a;
        err = (a - (sum - bb)) + (b - bb);
    }

    private static void TwoProduct(float a, float b, out float prod, out float err)
    {
        prod = a * b;
        float a_hi, a_lo, b_hi, b_lo;
        Split(a, out a_hi, out a_lo);
        Split(b, out b_hi, out b_lo);
        err = ((a_hi * b_hi - prod) + a_hi * b_lo + a_lo * b_hi) + a_lo * b_lo;
    }

    private static void Split(float x, out float hi, out float lo)
    {
        float t = x * 8193f;
        hi = t - (t - x);
        lo = x - hi;
    }

    private static QuadFloat Renormalize(float a, float b, float c, float d)
    {
        float s, e;
        TwoSum(a, b, out s, out e);
        a = s; b = e;
        TwoSum(a, c, out s, out e);
        a = s; c = e;
        TwoSum(a, d, out s, out e);
        a = s; d = e;
        return new QuadFloat(a, b + c, d, 0);
    }

    public static string ToString(QuadFloat q)
    {
        return $"({q.a}, {q.b}, {q.c}, {q.d})";
    }
}
