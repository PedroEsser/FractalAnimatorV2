using System.Numerics;
using UnityEngine;

public static class HighPrecision
{
    public const int FIXED_BITS = 128;
    public static readonly BigInteger One = BigInteger.One << FIXED_BITS;

    public static BigInteger ToFixed(double v)
    {
        return (BigInteger)(v * (double)One);
    }

    public static double ToDouble(BigInteger fx)
    {
        return (double)fx / (double)One;
    }

    public static BigInteger AddPixels(BigInteger centerFixed, long pixels, BigInteger scaleFixed)
    {
        return centerFixed + scaleFixed * pixels;
    }

    public static BigInteger ZoomIn(BigInteger scaleFixed)
    {
        return scaleFixed * (long)8 / (long)7;
    }

    public static BigInteger ZoomOut(BigInteger scaleFixed)
    {
        return scaleFixed * (long)7 / (long)8;
    }

    public static BigInteger Zoom(BigInteger scaleFixed, int steps)
    {
        int sign = steps > 0 ? 1 : -1;
        int absSteps = steps * sign;
        for (int i = 0; i < absSteps; i++)
        {
            scaleFixed = sign < 0 ? ZoomIn(scaleFixed) : ZoomOut(scaleFixed);
        }
        return scaleFixed;
    }

    public static UnityEngine.Vector2 SplitToFloat2(double d)
    {
        double c = (1 << 12) + 1;
        double t = c * d;
        double hi = t - (t - d);
        double lo = d - hi;
        return new UnityEngine.Vector2((float)hi, (float)lo);
    }

    public static long OrthogonalPixelDistance(BigInteger a, BigInteger b, BigInteger scale)
    {
        BigInteger dF = BigInteger.Abs(a - b);
        BigInteger px = dF / scale;
        if (px > long.MaxValue) return long.MaxValue;
        return (long)px;
    }

}
