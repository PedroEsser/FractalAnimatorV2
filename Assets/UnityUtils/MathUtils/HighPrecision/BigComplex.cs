using UnityEngine;
using System;
using System.Numerics;
using static FixedFloat;

public class BigComplex
{
    public static readonly BigComplex ZERO = FromDouble(0, 0);
    public static readonly BigComplex ONE = FromDouble(1, 0);
    public static readonly BigComplex I = FromDouble(0, 1);


    public FixedFloat Real { get; }
    public FixedFloat Imag { get; }

    public BigComplex(FixedFloat real, FixedFloat imag)
    {
        Real = real;
        Imag = imag;
    }

    public BigComplex(BigInteger real, BigInteger imag)
    {
        Real = new FixedFloat(real);
        Imag = new FixedFloat(imag);
    }

    public ComplexDouble toComplexDouble()
    {
        return new ComplexDouble(Real.ToDouble(), Imag.ToDouble());
    }

    public static BigComplex operator +(BigComplex a, BigComplex b) =>
        new BigComplex(a.Real + b.Real, a.Imag + b.Imag);

    public static BigComplex operator -(BigComplex a) => new BigComplex(-a.Real, -a.Imag);

    public static BigComplex operator -(BigComplex a, BigComplex b) => a + (-b);

    public static BigComplex operator *(BigComplex a, BigComplex b)  //Using raw BigInteger to avoid multiple divisions by Scale.
    {
        BigInteger re = (a.Real.Value * b.Real.Value - a.Imag.Value * b.Imag.Value) / FixedFloat.Scale;
        BigInteger im = (a.Real.Value * b.Imag.Value + a.Imag.Value * b.Real.Value) / FixedFloat.Scale;
        return new BigComplex(re, im);
    }

    public static BigComplex operator *(BigComplex a, double b)
    {
        FixedFloat fixedB = new FixedFloat(b);
        return new BigComplex(a.Real * fixedB, a.Imag * fixedB);
    }

    public static BigComplex operator *(double a, BigComplex b){ return b * a; }

    public static BigComplex operator /(BigComplex a, double b)
    {
        FixedFloat fixedB = new FixedFloat(b);
        return new BigComplex(a.Real / fixedB, a.Imag / fixedB);
    }

    public static BigComplex operator /(BigComplex a, BigComplex b){
        BigComplex conjugate = b.Conjugate();
        BigComplex numerator = a * conjugate;
        FixedFloat denominator = b.NormSquared();
        return new BigComplex(numerator.Real / denominator, numerator.Imag / denominator);
    }

    public static BigComplex operator ^(BigComplex a, int n){ return a.Pow(n); }

    public BigComplex Conjugate()
    {
        return new BigComplex(Real, -Imag);
    }

    public BigComplex Square()
    {
        BigInteger re = (Real.Value * Real.Value - Imag.Value * Imag.Value) / FixedFloat.Scale;
        BigInteger im = (2 * Real.Value * Imag.Value) / FixedFloat.Scale;
        return new BigComplex(re, im);
    }

    public BigComplex Pow(int n)
    {
        BigComplex result = ONE;
        BigComplex acc = this;
        int sign = n < 0 ? -1 : 1;
        n *= sign;
        for(int i = 1; i <= n; i<<=1){
            if((n & i) != 0){
                result *= acc;
            }
            acc *= acc;
        }
        if(sign < 0){
            result = ONE / result;
        }
        return result;
    }

    public FixedFloat NormSquared()
    {
        BigInteger squaredNorm = (Real.Value * Real.Value + Imag.Value * Imag.Value) / FixedFloat.Scale;
        return new FixedFloat(squaredNorm);
    }

    public double NormSquaredAsDouble()
    {
        double re = Real.ToDouble();
        double im = Imag.ToDouble();
        return re * re + im * im;
    }

    public double Abs()
    {
        return Math.Sqrt(NormSquaredAsDouble());
    }

    public static BigComplex FromDouble(double re, double im)
    {
        return new BigComplex(new FixedFloat(re), new FixedFloat(im));
    }

    public UnityEngine.Vector4 ToVector4(){
        UnityEngine.Vector2 re = Real.ToVector2();
        UnityEngine.Vector2 im = Imag.ToVector2();
        return new UnityEngine.Vector4(re.x, re.y, im.x, im.y);
    }

    public static BigComplex ParseDecimalString(string s)
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

        FixedFloat real = FixedFloat.ZERO;
        FixedFloat imag = FixedFloat.ZERO;
        if(parts.Length == 1){
            if(parts[0].EndsWith("i")){
                string imagStr = parts[0].Substring(0, parts[0].Length - 1);
                if(imagStr == ""){
                    imagStr = "1";
                }
                imag = negativeFirst ? -FixedFloat.ParseDecimalString(imagStr) : FixedFloat.ParseDecimalString(imagStr);
            }else{
                real = negativeFirst ? -FixedFloat.ParseDecimalString(parts[0]) : FixedFloat.ParseDecimalString(parts[0]);
            }
        }
        else if(parts.Length == 2){
            if(parts[0].EndsWith("i") && !parts[1].EndsWith("i")){
                string imagStr = parts[0].Substring(0, parts[0].Length - 1);
                if(imagStr == ""){
                    imagStr = "1";
                }
                imag = negativeFirst ? -FixedFloat.ParseDecimalString(imagStr) : FixedFloat.ParseDecimalString(imagStr);
                real = negativeSecond ? -FixedFloat.ParseDecimalString(parts[1]) : FixedFloat.ParseDecimalString(parts[1]);
            }
            else if(!parts[0].EndsWith("i") && parts[1].EndsWith("i")){
                string imagStr = parts[1].Substring(0, parts[1].Length - 1);
                if(imagStr == ""){
                    imagStr = "1";
                }
                real = negativeFirst ? -FixedFloat.ParseDecimalString(parts[0]) : FixedFloat.ParseDecimalString(parts[0]);
                imag = negativeSecond ? -FixedFloat.ParseDecimalString(imagStr) : FixedFloat.ParseDecimalString(imagStr);
            }
            else{
                throw new Exception("Invalid complex number string: " + s);
            }
        }
        else{
            throw new Exception("Invalid complex number string: " + s);
        }
        return new BigComplex(real, imag);
    }


    public override bool Equals(object obj)
    {
        return obj is BigComplex other && Real.Equals(other.Real) && Imag.Equals(other.Imag);
    }

    public override int GetHashCode(){
        return Real.GetHashCode() ^ Imag.GetHashCode();
    }

    public override string ToString(){
        if(Imag.Value == BigInteger.Zero){
            return $"{Real.ToDouble()}";
        }
        else if(Real.Value == BigInteger.Zero){
            return $"{Imag.ToDouble()}i";
        }
        double im = Imag.ToDouble();
        if(im > 0){
            return $"{Real.ToDouble()}+{im}i";
        }
        else{
            return $"{Real.ToDouble()}{im}i";
        }
    }

}
