using System;
using System.Numerics;

public readonly struct FixedFloat : IEquatable<FixedFloat>
{
    public static readonly int PrecisionBits = 256;
    public static readonly BigInteger Scale = BigInteger.One << PrecisionBits;
    public static readonly FixedFloat ZERO = new FixedFloat(BigInteger.Zero);
    public static readonly FixedFloat ONE = new FixedFloat(Scale);

    public BigInteger Value { get; }

    public FixedFloat(BigInteger value)
    {
        Value = value;
    }

    public FixedFloat(double value)
    {
        Value = ToFixed(value);
    }

    // ---- Operators ----
    public static FixedFloat operator +(FixedFloat a, FixedFloat b) =>
        new FixedFloat(a.Value + b.Value);

    public static FixedFloat operator -(FixedFloat a, FixedFloat b) =>
        new FixedFloat(a.Value - b.Value);

    public static FixedFloat operator -(FixedFloat a) =>
        new FixedFloat(-a.Value);

    public static FixedFloat operator *(FixedFloat a, FixedFloat b) =>
        new FixedFloat(a.Value * b.Value / Scale);

    public static FixedFloat operator /(FixedFloat a, FixedFloat b) =>
        new FixedFloat(a.Value * Scale / b.Value);

    public static FixedFloat operator *(FixedFloat a, double b) =>
        new FixedFloat(a.Value * ToFixed(b) / Scale);

    public static FixedFloat operator *(double a, FixedFloat b) => b * a;

    public static FixedFloat operator *(FixedFloat a, long b) =>
        new FixedFloat(a.Value * b);

    public static FixedFloat operator *(long a, FixedFloat b) => b * a;

    public static FixedFloat operator ^(FixedFloat a, long n) => a.Pow(n);

    // ---- Math ----
    public FixedFloat Pow(long n)
    {
        BigInteger result = Scale;
        BigInteger acc = Value;
        int sign = n < 0 ? -1 : 1;
        n *= sign;

        for (long i = 1; i <= n; i <<= 1)
        {
            if ((n & i) != 0)
            {
                result = result * acc >> PrecisionBits;
            }
            acc = acc * acc >> PrecisionBits;
        }

        if (sign < 0)
        {
            result = (Scale << PrecisionBits) / result;
        }

        return new FixedFloat(result);
    }

    public double ToDouble()
    {
        return (double)Value / (double)Scale;
    }
    public UnityEngine.Vector2 ToVector2()
    {
        double d = ToDouble();
        SplitDouble(d, out float hi, out float lo);
        return new UnityEngine.Vector2(hi, lo);
    }

    // ---- Static helpers ----
    public static BigInteger ToFixed(double v) => (BigInteger)(v * (double)Scale);

    public static void SplitDouble(double x, out float hi, out float lo)
    {
        float h = (float)x;
        hi = h;
        lo = (float)(x - (double)h);
    }

    public static FixedFloat ParseDecimalString(string s)
    {
        if (s == "")
            return ZERO;

        bool negative = s.StartsWith("-");
        if (negative) s = s.Substring(1);

        string[] parts = s.Split('.');
        BigInteger intPart = BigInteger.Parse(parts[0]);

        BigInteger result = intPart * Scale;

        if (parts.Length > 1)
        {
            string frac = parts[1];
            BigInteger fracInt = BigInteger.Parse(frac);
            BigInteger fracScale = BigInteger.Pow(10, frac.Length);

            result += fracInt * Scale / fracScale;
        }

        return new FixedFloat(negative ? -result : result);
    }

    // ---- Equality & ToString ----
    public override bool Equals(object obj) =>
        obj is FixedFloat other && Value == other.Value;

    public bool Equals(FixedFloat other) =>
        Value == other.Value;

    public override int GetHashCode() =>
        Value.GetHashCode();

    public override string ToString() =>
        Value.ToString();
}
