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

    public static BigInteger ZoomPow2(BigInteger scaleFixed, int steps)
    {
        if (steps > 0) return scaleFixed >> steps;     // divide by 2^steps
        if (steps < 0) return scaleFixed << -steps;    // multiply by 2^(-steps)
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
