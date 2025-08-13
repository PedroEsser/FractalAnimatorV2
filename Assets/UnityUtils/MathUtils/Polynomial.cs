using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Polynomial
{
    public Complex[] Coefficients { get; }
    public int Degree => Coefficients.Length - 1;

    public Polynomial(params Complex[] coefficients)
    {
        this.Coefficients = coefficients;
    }

    public Polynomial(params float[] coefficients)
    {
        Complex[] complexCoefficients = new Complex[coefficients.Length];
        for (int i = 0; i < coefficients.Length; i++)
            complexCoefficients[i] = new Complex(coefficients[i], 0);
        this.Coefficients = complexCoefficients;
    }

    public Polynomial(int degree)
    {
        this.Coefficients = new Complex[degree + 1];
        for (int i = 0; i < Coefficients.Length; i++)
            Coefficients[i] = Complex.ZERO;
    }

    

    public Polynomial addPolynomial(Polynomial p)
    {
        Polynomial higherDegree = this;
        if (this.Degree < p.Degree)
        {
            higherDegree = p;
            p = this;
        }
        Polynomial sum = higherDegree.clone();
        for (int i = 0; i < p.Coefficients.Length; i++)
            sum.Coefficients[i] += p.Coefficients[i];

        return sum;
    }

    public Polynomial minus()
    {
        Polynomial result = this.clone();
        for (int i = 0; i < result.Coefficients.Length; i++)
            result.Coefficients[i] *= -1;

        return result;
    }

    public Polynomial timesComplex(Complex c)
    {
        Polynomial result = this.clone();
        for (int i = 0; i < result.Coefficients.Length; i++)
            result.Coefficients[i] *= c;

        return result;
    }

    public Polynomial timesPowerX(int power = 1)
    {
        Polynomial result = new Polynomial(this.Degree + power);
        for (int i = power; i < result.Coefficients.Length; i++)
            result.Coefficients[i] = this.Coefficients[i - power];
        return result;
    }

    public Polynomial timesPolynomial(Polynomial p)
    {
        Polynomial result = new Polynomial(this.Degree + p.Degree);
        for (int i = 0; i < this.Coefficients.Length; i++)
            result += (p << i) * this.Coefficients[i];

        return result;
    }

    public Complex coefficientAt(int powerX) { return Coefficients[powerX]; }

    public Complex evaluate(Complex x)
    {
        Complex result = Complex.ZERO;
        Complex acc = Complex.ONE;

        for (int i = 0; i < Coefficients.Length; i++)
        {
            result += Coefficients[i] * acc;
            acc *= x;
        }

        return result;
    }

    public Polynomial derivative()
    {
        if (this.Degree == 0)
            return Polynomial.ZERO;
        Polynomial derivative = new Polynomial(this.Degree - 1);
        for (int i = 0; i < derivative.Coefficients.Length; i++)
            derivative.Coefficients[i] = this.Coefficients[i + 1] * (i + 1);
        return derivative;
    }

    public Polynomial addRoot(Complex root) { return (this << 1) - this * root; }

    public Polynomial clone()
    {
        Complex[] clonedCoefficients = new Complex[Coefficients.Length];
        for (int i = 0; i < Coefficients.Length; i++)
            clonedCoefficients[i] = Coefficients[i].clone();

        return new Polynomial(clonedCoefficients);
    }
    public static Polynomial fromRoots(IEnumerable<Complex> roots)
    {
        Polynomial p = Polynomial.ONE;
        foreach (Complex c in roots)
            p = p.addRoot(c);

        return p;
    }

    public static Polynomial operator +(Polynomial a, Polynomial b) { return a.addPolynomial(b); }

    public static Polynomial operator -(Polynomial a) { return a.minus(); }
    public static Polynomial operator -(Polynomial a, Polynomial b) { return a + -b; }
    public static Polynomial operator -(Polynomial a, Complex c) { return a - new Polynomial(c); }

    public static Polynomial operator *(Polynomial a, Complex c) { return a.timesComplex(c); }
    public static Polynomial operator *(Complex c, Polynomial a) { return a * c; }
    public static Polynomial operator *(Polynomial a, Polynomial b) { return a.timesPolynomial(b); }

    public static Polynomial operator <<(Polynomial a, int powerX) { return a.timesPowerX(powerX); }

    public static Polynomial ZERO = new Polynomial(new Complex[1] { Complex.ZERO });
    public static Polynomial ONE = new Polynomial(new Complex[1] { Complex.ONE });

    public static Polynomial rootsOfUnity(int rootsCount) { return (Polynomial.ONE << rootsCount) - Complex.ONE; }

    public override string ToString()
    {
        string s = "";
        for(int i = Coefficients.Length-1; i > 0; i--)
        {
            s += "(" + Coefficients[i] + ")*x^" + i + " + ";
        }
        return s + Coefficients[0];
    }

    public override bool Equals(object obj)
    {
        if (!(obj is Polynomial p))
            return false;
        if (this.Degree != p.Degree)
            return false;

        for (int i = 0; i < Coefficients.Length; i++)
            if (!Coefficients[i].Equals(p.Coefficients[i]))
                return false;

        return true;
    }

}
