using UnityEngine;
using System;

[Serializable()]
public class ComplexDouble
{
    public double re { get; }
    public double im { get; }

    public ComplexDouble() : this(0, 0) { }
    public ComplexDouble(double re, double im)
    {
        this.re = re;
        this.im = im;
    }

    public static readonly ComplexDouble ZERO = new ComplexDouble(0, 0);
    public static readonly ComplexDouble ONE = new ComplexDouble(1, 0);
    public static readonly ComplexDouble I = new ComplexDouble(0, 1);

    public static ComplexDouble operator +(ComplexDouble a, ComplexDouble b) { return new ComplexDouble(a.re + b.re, a.im + b.im); }
    public static ComplexDouble operator -(ComplexDouble a, ComplexDouble b) { return new ComplexDouble(a.re - b.re, a.im - b.im); }
    public static ComplexDouble operator -(ComplexDouble a) { return new ComplexDouble(-a.re, -a.im); }

    public static ComplexDouble operator *(ComplexDouble a, ComplexDouble b) { return new ComplexDouble(a.re * b.re - a.im * b.im, a.re * b.im + a.im * b.re); }
    public static ComplexDouble operator *(ComplexDouble a, double b) { return new ComplexDouble(a.re * b, a.im * b); }
    public static ComplexDouble operator *(double b, ComplexDouble a) { return new ComplexDouble(a.re * b, a.im * b); }
    public static ComplexDouble operator ^(ComplexDouble a, int power) { return a.integerPower(power); }
    public static ComplexDouble operator ^(ComplexDouble a, double power) { return a.complexPower(new ComplexDouble(power, 0)); }
    public static ComplexDouble operator ^(ComplexDouble a, ComplexDouble power) { return a.complexPower(power); }

    public static ComplexDouble operator /(ComplexDouble a, double b) { return new ComplexDouble(a.re / b, a.im / b); }
    public static ComplexDouble operator /(double b, ComplexDouble a) { return new ComplexDouble(a.re / b, a.im / b); }
    public static ComplexDouble operator /(ComplexDouble a, ComplexDouble b) { return a * b.conjugate() / b.squaredNorm(); }

    public static ComplexDouble operator ~(ComplexDouble a) { return a.conjugate(); }
    public static explicit operator ComplexDouble(Vector2 v) { return new ComplexDouble(v.x, v.y); }

    public double abs() { return Math.Sqrt(squaredNorm()); }
    public double squaredNorm() { return re * re + im * im; }
    public ComplexDouble conjugate() { return new ComplexDouble(re, -im); }
    public double arg() { return Math.Atan2(im, re); }
    public ComplexDouble clone() { return new ComplexDouble(re, im); }

    public ComplexDouble integerPower(int power)
    {
        ComplexDouble result = ComplexDouble.ONE;
        ComplexDouble acc = this.clone();
        int p2 = 1;
        while (p2 <= power)
        {
            if ((p2 & power) != 0)
                result *= acc;
            acc *= acc;
            p2 <<= 1;
        }
        return result;
    }

    public ComplexDouble complexPower(ComplexDouble power)
    {
        if (this.IsZero())
            return ZERO;
        ComplexDouble exp = Math.Log(this.abs()) * power + ComplexDouble.I * this.arg() * power;
        return Polar(Math.Exp(exp.re), exp.im);
    }

    public static ComplexDouble Polar(double angle) { return new ComplexDouble(Math.Cos(angle), Math.Sin(angle)); }
    public static ComplexDouble Polar(double radius, double angle) { return Polar(angle) * radius; }

    public static ComplexDouble[] rootsOfUnity(int rootCount)
    {
        ComplexDouble[] roots = new ComplexDouble[rootCount];

        for (int i = 0; i < rootCount; i++)
            roots[i] = Polar(2 * Math.PI * i / rootCount);

        return roots;
    }

    public bool IsNaN()
    {
        return double.IsNaN(re) || double.IsNaN(im) || double.IsInfinity(re) || double.IsInfinity(im);
    }
    public bool IsZero()
    {
        return re == 0 && im == 0;
    }

    public override string ToString()
    {
        if (re == 0 && im == 0)
            return "0";
        if (re != 0 && im != 0)
            return re + " + " + im + "i";
        if (im == 0)
            return re + "";
        return im + "i";
    }
    public override bool Equals(object obj)
    {
        if (!(obj is ComplexDouble complex))
            return false;
        return complex.re == this.re && complex.im == this.im;
    }

    public override int GetHashCode()
    {
        return this.AsVector4().GetHashCode();
    }

    public Vector4 AsVector4()
    {
        float re_hi, re_lo, im_hi, im_lo;
        SplitDouble(re, out re_hi, out re_lo);
        SplitDouble(im, out im_hi, out im_lo);
        return new Vector4(re_hi, re_lo, im_hi, im_lo);
    }

    public static void SplitDouble(double x, out float hi, out float lo)
    {
        float h = (float)x;
        hi = h;
        lo = (float)(x - (double)h);
    }
}
