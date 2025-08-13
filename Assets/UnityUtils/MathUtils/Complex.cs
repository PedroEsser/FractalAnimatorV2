
using UnityEngine;
using System;

[Serializable()]
public class Complex
{
    public float re { get; }
    public float im { get; }

    public Complex() : this(0, 0) { }
    public Complex(float re, float im)
    {
        this.re = re;
        this.im = im;
    }

    public float abs() { return Mathf.Sqrt(squaredNorm()); }
    public float squaredNorm() { return re * re + im * im; }
    public Complex conjugate() { return new Complex(re, -im); }
    public float arg() { return Mathf.Atan2(im, re); }
    public Complex clone() { return new Complex(re, im); }

    public Complex integerPower(int power)
    {
        Complex result = Complex.ONE;
        Complex acc = this.clone();
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

    public Complex complexPower(Complex power)
    {
        if (this.IsZero())
            return ZERO;
        Complex exp = Mathf.Log(this.abs()) * power + Complex.I * this.arg() * power;
        return Polar(Mathf.Exp(exp.re), exp.im);
    }

    public Vector4 AsVector() { return new Vector4(re, im, 0 , 0); }

    public static Complex Polar(float angle) { return new Complex(Mathf.Cos(angle), Mathf.Sin(angle)); }
    public static Complex Polar(float radius, float angle) { return Polar(angle) * radius; }

    public static Complex[] rootsOfUnity(int rootCount)
    {
        Complex[] roots = new Complex[rootCount];

        for (int i = 0; i < rootCount; i++)
            roots[i] = Polar(2 * Mathf.PI * i / rootCount);

        return roots;
    }

    public static readonly Complex ZERO = new Complex(0, 0);
    public static readonly Complex ONE = new Complex(1, 0);
    public static readonly Complex I = new Complex(0, 1);

    public static Complex operator +(Complex a, Complex b) { return new Complex(a.re + b.re, a.im + b.im); }
    public static Complex operator -(Complex a, Complex b) { return new Complex(a.re - b.re, a.im - b.im); }
    public static Complex operator -(Complex a) { return new Complex(-a.re, -a.im); }

    public static Complex operator *(Complex a, Complex b) { return new Complex(a.re * b.re - a.im * b.im, a.re * b.im + a.im * b.re); }
    public static Complex operator *(Complex a, float b) { return new Complex(a.re * b, a.im * b); }
    public static Complex operator *(float b, Complex a) { return new Complex(a.re * b, a.im * b); }
    public static Complex operator ^(Complex a, int power) { return a.integerPower(power); }
    public static Complex operator ^(Complex a, float power) { return a.complexPower(new Complex(power, 0)); }
    public static Complex operator ^(Complex a, Complex power) { return a.complexPower(power); }

    public static Complex operator /(Complex a, float b) { return new Complex(a.re / b, a.im / b); }
    public static Complex operator /(float b, Complex a) { return new Complex(a.re / b, a.im / b); }
    public static Complex operator /(Complex a, Complex b) { return a * b.conjugate() / b.squaredNorm(); }

    public static Complex operator ~(Complex a) { return a.conjugate(); }
    //public static explicit operator Complex(float a) { return new Complex(a, 0); }
    public static explicit operator Complex(Vector2 v) { return new Complex(v.x, v.y); }
    public static implicit operator Vector4(Complex c) => new Vector4(c.re, c.im, 0, 0);
    public static implicit operator Vector2(Complex c) => new Vector2(c.re, c.im);

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

    public bool IsNaN()
    {
        return float.IsNaN(re) || float.IsNaN(im) || float.IsInfinity(re) || float.IsInfinity(im);
    }
    public bool IsZero()
    {
        return re == 0 && im == 0;
    }
    public override bool Equals(object obj)
    {
        if (!(obj is Complex complex))
            return false;
        return complex.re == this.re && complex.im == this.im;
    }

    public override int GetHashCode()
    {
        return new Vector2(re, im).GetHashCode();
    }

}
