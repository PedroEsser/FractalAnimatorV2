using UnityEngine;
using System;
using System.Numerics;

public class BigComplex
{
    private static readonly int PrecisionBits = 128;
    private static readonly BigInteger Scale = BigInteger.One << PrecisionBits;

    public BigInteger Real { get; private set; }
    public BigInteger Imag { get; private set; }

    public BigComplex(BigInteger real, BigInteger imag)
    {
        Real = real;
        Imag = imag;
    }

    public ComplexDouble toComplexDouble()
    {
        return new ComplexDouble(ToDouble(Real), ToDouble(Imag));
    }

    public static BigComplex operator +(BigComplex a, BigComplex b) =>
        new BigComplex(a.Real + b.Real, a.Imag + b.Imag);

    public static BigComplex operator -(BigComplex a, BigComplex b) =>
        new BigComplex(a.Real - b.Real, a.Imag - b.Imag);

    public static BigComplex operator *(BigComplex a, BigComplex b)
    {
        BigInteger re = (a.Real * b.Real - a.Imag * b.Imag) / Scale;
        BigInteger im = (a.Real * b.Imag + a.Imag * b.Real) / Scale;
        return new BigComplex(re, im);
    }

    public static BigComplex operator *(BigComplex a, double b)
    {
        return new BigComplex(a.Real * ToFixed(b) / Scale, a.Imag * ToFixed(b) / Scale);
    }

    public static BigComplex operator *(double a, BigComplex b){ return b * a; }

    public static BigComplex operator /(BigComplex a, double b)
    {
        return new BigComplex(a.Real * Scale / ToFixed(b), a.Imag * Scale / ToFixed(b));
    }

    public static BigComplex operator /(BigComplex a, BigComplex b){
        BigComplex conjugate = b.Conjugate();
        BigComplex numerator = a * conjugate;
        BigInteger denominator = b.NormSquared();
        return new BigComplex(numerator.Real / denominator, numerator.Imag / denominator);
    }

    public BigComplex Conjugate()
    {
        return new BigComplex(Real, -Imag);
    }

    public BigComplex Square()
    {
        BigInteger re = (Real * Real - Imag * Imag) / Scale;
        BigInteger im = (2 * Real * Imag) / Scale;
        return new BigComplex(re, im);
    }

    public BigInteger NormSquared()
    {
        return (Real * Real + Imag * Imag) / Scale;
    }

    public double Abs()
    {
        double re = ToDouble(Real);
        double im = ToDouble(Imag);
        return Math.Sqrt(re * re + im * im);
    }





    public static BigInteger ToFixed(double v)
    {
        return (BigInteger)(v * (double)Scale);
    }

    public static double ToDouble(BigInteger fx)
    {
        return (double)fx / (double)Scale;
    }

    public static BigComplex FromDouble(double re, double im)
    {
        return new BigComplex(ToFixed(re), ToFixed(im));
    }

    public static BigInteger ParseDecimalString(string s)
    {
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

        return negative ? -result : result;
    }

    public static BigComplex FromDecimalString(string s)
    {
        s = s.Replace(" ", "");
        bool negativeFirst = s.StartsWith("-");
        if(s.StartsWith("+") || negativeFirst) s = s.Substring(1);

        bool negativeSecond = s.Contains("-");
        string[] parts;
        if(negativeSecond){
            parts = s.Split('-');
        }
        else{
            parts = s.Split('+');
        }
        if(parts.Length == 1){
            if(parts[0].EndsWith("i")){
                Debug.Log("imaginary " + parts[0]);
                return new BigComplex(BigInteger.Zero, ParseDecimalString(parts[0].Substring(0, parts[0].Length - 1)));
            }
            else{
                Debug.Log("real " + parts[0]);
                return new BigComplex(ParseDecimalString(parts[0]), BigInteger.Zero);
            }
        }
        else if(parts.Length == 2){
            if(parts[0].EndsWith("i") && !parts[1].EndsWith("i")){
                Debug.Log("imaginary: " + parts[0] + " real: " + parts[1]);
                return new BigComplex(ParseDecimalString(parts[0].Substring(0, parts[0].Length - 1)), ParseDecimalString(parts[1]));
            }
            else if(!parts[0].EndsWith("i") && parts[1].EndsWith("i")){
                Debug.Log("real: " + parts[0] + " imaginary: " + parts[1]);
                return new BigComplex(ParseDecimalString(parts[0]), ParseDecimalString(parts[1].Substring(0, parts[1].Length - 1)));
            }
            else{
                throw new Exception("Invalid complex number string: " + s);
            }
        }
        else{
            throw new Exception("Invalid complex number string: " + s);
        }
    }

    public override string ToString(){
        if(Imag == BigInteger.Zero){
            return $"{ToDouble(Real)}";
        }
        else if(Real == BigInteger.Zero){
            return $"{ToDouble(Imag)}i";
        }
        double im = ToDouble(Imag);
        if(im > 0){
            return $"{ToDouble(Real)}+{im}i";
        }
        else{
            return $"{ToDouble(Real)}-{im}i";
        }
    }

}
