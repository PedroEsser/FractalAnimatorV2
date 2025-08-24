int MandelbrotIterations(QuadComplex c, int maxIter, float escapeRadiusSquared, out QuadComplex finalZ) {
    QuadComplex z;
    z.real = float4(0, 0, 0, 0);
    z.imag = float4(0, 0, 0, 0);

    for (int i = 0; i < maxIter; ++i) {
        z = AddQuadComplex(MulQuadComplex(z, z), c);
        float4 mag2 = AbsSqrQuadComplex(z);
        if (mag2.x > escapeRadiusSquared) {
            finalZ = z;
            return i;
        }
    }
    finalZ = z;
    return maxIter;
}