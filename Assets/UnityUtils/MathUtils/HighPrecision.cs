using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Numerics;

public class HighPrecision
{
    public const int FIXED_BITS = 128;
    public static BigInteger One = new BigInteger(1 << FIXED_BITS);
    public static BigInteger ToFixed(double value)
    {
        return new BigInteger(value * (double)One);
    }

    public static double ToDouble(BigInteger value)
    {
        return (double)value / (double)One;
    }
}
