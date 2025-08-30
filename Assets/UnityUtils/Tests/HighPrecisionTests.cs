using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static FixedFloat;
using static BigComplex;
using System;

public class HighPrecisionTests
{

    private static System.Random random = new System.Random(1234);

    [Test]
    public void FixedFloatTest()
    {
        FixedFloat a = new FixedFloat(101.125);
        Assert.AreEqual(a.Value, FixedFloat.ToFixed(101.125));
        FixedFloat b = new FixedFloat(8.0);
        Assert.AreEqual((a * b).Value, FixedFloat.ToFixed(809));
        Assert.AreEqual((a / b).Value, FixedFloat.ToFixed(101.125 / 8));
        Assert.AreEqual((a + b).Value, FixedFloat.ToFixed(109.125));
        Assert.AreEqual((a - b).Value, FixedFloat.ToFixed(93.125));

        FixedFloat two = new FixedFloat(2.0);
        FixedFloat small = two ^ -30;
        FixedFloat small2 = two ^ -60;
        Assert.AreEqual(small * small2, two ^ -90);
        Assert.AreEqual(small / small2, two ^ 30);
        Assert.AreEqual(small ^ 2, small2);

        FixedFloat big = two ^ 100;
        FixedFloat big2 = two ^ 1000;
        Assert.AreEqual(big ^ 10, big2);
        Assert.AreEqual(big ^ 1000, big2 ^ 100);
    }

    [Test]
    public void BigComplexTest()
    {
        BigComplex one = BigComplex.ONE;
        BigComplex i = BigComplex.I;
        Assert.AreEqual(i * i, BigComplex.ParseDecimalString("-1"));
        Assert.AreEqual(i * i * i, BigComplex.ParseDecimalString("-i"));
        Assert.AreEqual(i * i * i * i, BigComplex.ParseDecimalString("1"));
        Assert.AreEqual(one + i, BigComplex.ParseDecimalString("1 + i"));
        Assert.AreEqual(one - i, BigComplex.ParseDecimalString("1 - i"));
        Assert.AreEqual(one * i, BigComplex.ParseDecimalString("i"));
        Assert.AreEqual(one / i, BigComplex.ParseDecimalString("-i"));
        Assert.AreEqual(one.Conjugate(), one);
        Assert.AreEqual(i.Conjugate(), -i);

        BigComplex onePlusI = one + i;
        Assert.AreEqual(onePlusI.Square(), BigComplex.ParseDecimalString("2i"));
        BigComplex oneMinusI = one - i;
        Assert.AreEqual(oneMinusI.Square(), BigComplex.ParseDecimalString("-2i"));
        Assert.AreEqual(onePlusI.Conjugate(), oneMinusI);
        Assert.AreEqual(oneMinusI.Conjugate(), onePlusI);
        Assert.AreEqual(onePlusI * oneMinusI, BigComplex.ParseDecimalString("2"));
        Assert.AreEqual(onePlusI / onePlusI, one);
        Assert.AreEqual(onePlusI / oneMinusI, BigComplex.ParseDecimalString("i"));
        Assert.AreEqual(oneMinusI / onePlusI, BigComplex.ParseDecimalString("-i"));
        Assert.AreEqual(onePlusI ^ 4, BigComplex.ParseDecimalString("-4"));
        Assert.AreEqual(onePlusI ^ 8, BigComplex.ParseDecimalString("16"));
        Assert.AreEqual(oneMinusI ^ 4, BigComplex.ParseDecimalString("-4"));
        Assert.AreEqual(oneMinusI ^ 8, BigComplex.ParseDecimalString("16"));
    }

    [Test]
    public void ParseDecimalStringTest()
    {
        BigComplex a = BigComplex.ParseDecimalString("0");
        Assert.AreEqual(a, BigComplex.ZERO);
        a = BigComplex.ParseDecimalString("1");
        Assert.AreEqual(a, BigComplex.ONE);
        a = BigComplex.ParseDecimalString("i");
        Assert.AreEqual(a, BigComplex.I);

        a = BigComplex.ParseDecimalString("1234 + 5678i");
        Assert.AreEqual(a, BigComplex.FromDouble(1234, 5678));
        a = BigComplex.ParseDecimalString("1234 - 5678i");
        Assert.AreEqual(a, BigComplex.FromDouble(1234, -5678));
        a = BigComplex.ParseDecimalString("-1234 - 5678i");
        Assert.AreEqual(a, BigComplex.FromDouble(-1234, -5678));
        a = BigComplex.ParseDecimalString("-1234 + 5678i");
        Assert.AreEqual(a, BigComplex.FromDouble(-1234, 5678));

        a = BigComplex.ParseDecimalString("5678i + 1234");
        Assert.AreEqual(a, BigComplex.FromDouble(1234, 5678));
        a = BigComplex.ParseDecimalString("5678i - 1234");
        Assert.AreEqual(a, BigComplex.FromDouble(-1234, 5678));
        a = BigComplex.ParseDecimalString("-5678i - 1234");
        Assert.AreEqual(a, BigComplex.FromDouble(-1234, -5678));
        a = BigComplex.ParseDecimalString("-5678i + 1234");
        Assert.AreEqual(a, BigComplex.FromDouble(1234, -5678));

        a = BigComplex.ParseDecimalString("1234.25 + 5678.75i");
        BigComplex b = BigComplex.FromDouble(1234.25, 5678.75);
        Assert.AreEqual(a, b);
    }


    public BigComplex GetRandomComplex()
    {
        return new BigComplex(random.Next(), random.Next());
    }

}
