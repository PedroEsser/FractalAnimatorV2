using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadComplex
{
    public QuadFloat real;
    public QuadFloat imag;

    public QuadComplex(QuadFloat real, QuadFloat imag)
    {
        this.real = real;
        this.imag = imag;
    }

    public static readonly QuadComplex ZERO = new QuadComplex(new QuadFloat(0), new QuadFloat(0));
    public static readonly QuadComplex ONE = new QuadComplex(new QuadFloat(1), new QuadFloat(0));
    public static readonly QuadComplex MINUS_ONE = -ONE;
    public static readonly QuadComplex I = new QuadComplex(new QuadFloat(0), new QuadFloat(1));
    public static readonly QuadComplex MINUS_I = -I;

    public QuadComplex Conjugate()
    {
        return new QuadComplex(real, -imag);
    }

    public QuadFloat AbsSqr()
    {
        return real * real + imag * imag;
    }


    public static QuadComplex operator +(QuadComplex a, QuadComplex b)
    {
        return new QuadComplex(a.real + b.real, a.imag + b.imag);
    }

    public static QuadComplex operator -(QuadComplex a)
    {
        return new QuadComplex(-a.real, -a.imag );
    }

    public static QuadComplex operator -(QuadComplex a, QuadComplex b) { return a + (-b); }

    public static QuadComplex operator +(QuadComplex a) { return a; }

    public static QuadComplex operator *(QuadComplex a, QuadComplex b)
    {
        return new QuadComplex(a.real * b.real - a.imag * b.imag, a.real * b.imag + a.imag * b.real);
    }

    public static QuadComplex operator *(QuadComplex a, QuadFloat b)
    {
        return new QuadComplex(a.real * b, a.imag * b);
    }

    public static QuadComplex operator *(QuadComplex a, float b)
    {
        return new QuadComplex(a.real * b, a.imag * b);
    }

    public static QuadComplex operator *(float a, QuadComplex b) { return b * a; }



    public static QuadComplex operator /(QuadComplex a, QuadFloat b)
    {
        return new QuadComplex(a.real / b, a.imag / b);
    }

    public static QuadComplex operator /(QuadComplex a, float b)
    {
        return new QuadComplex(a.real / b, a.imag / b);
    }
    public static QuadComplex operator /(float a, QuadComplex b) { return (~b) / b.AbsSqr() * a; }

    public static QuadComplex operator /(QuadComplex a, QuadComplex b)
    {
        return a * (~b) / b.AbsSqr();
    }

    public static QuadComplex operator ~(QuadComplex a) { return a.Conjugate(); }
    
}
