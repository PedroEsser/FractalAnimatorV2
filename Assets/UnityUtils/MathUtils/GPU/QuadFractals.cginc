int MandelbrotIterations(QuadComplex c, int maxIter, float escapeRadiusSquared) {
    QuadComplex z;
    z.real = float4(c.real.x, 0, 0, 0);
    z.imag = float4(c.imag.x, 0, 0, 0);

    for (int i = 0; i < maxIter; ++i) {
        z = AddQuadComplex(MulQuadComplex(z, z), c);
        float4 mag2 = AbsSqrQuadComplex(z);
        if (mag2.x > escapeRadiusSquared) return i;
    }
    return maxIter;
}