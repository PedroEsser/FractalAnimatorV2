using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Polynomial;
using static Complex;
using System;

public class PolynomialTests
{

    private static System.Random random = new System.Random(1234);

    [Test]
    public void CoefficientsTest()
    {
        int coefficientsCount = 1000;
        Complex[] coefficients = new Complex[coefficientsCount];
        for (int i = 0; i < coefficients.Length; i++)
            coefficients[i] = GetRandomComplex();
        Polynomial p = new Polynomial(coefficients);
        Assert.AreEqual(p.Degree, coefficientsCount - 1);
        for (int i = 0; i < coefficients.Length; i++)
            Assert.AreEqual(p.Coefficients[i], coefficients[i]);
    }

    [Test]
    public void RootsTest()
    {
        int rootsCount = 10;
        Complex[] roots = new Complex[rootsCount];
        for (int i = 0; i < roots.Length; i++)
            roots[i] = GetRandomComplex();
        Polynomial p = Polynomial.fromRoots(roots);
        Assert.AreEqual(p.Degree, rootsCount);
        for (int i = 0; i < roots.Length; i++)
            AreAlmostEqual(p.evaluate(roots[i]), Complex.ZERO);
    }

    [Test]
    public void DerivativeTest()
    {
        int degree = 10;
        Complex[] coefficients = new Complex[degree + 1];
        for (int i = 0; i < coefficients.Length; i++)
            coefficients[i] = GetRandomComplex();
        Polynomial p = new Polynomial(coefficients);
        Polynomial derivative = p.derivative();
        for (int i = 0; i < coefficients.Length - 1; i++)
            AreAlmostEqual(derivative.Coefficients[i], coefficients[i + 1] * (i + 1));
    }

    public Complex GetRandomComplex()
    {
        return new Complex(random.Next(), random.Next());
    }

    public static void AreAlmostEqual(Complex expected, Complex actual, double tolerance = 0.0001f, string message = "")
    {
        double realDiff = Math.Abs(expected.re - actual.re);
        double imagDiff = Math.Abs(expected.im - actual.im);

        if (realDiff > tolerance || imagDiff > tolerance)
        {
            Assert.Fail($"Expected {expected} but got {actual}. " +
                        $"Real diff = {realDiff}, Imag diff = {imagDiff}. {message}");
        }
    }

}
