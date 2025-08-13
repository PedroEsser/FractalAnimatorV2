struct QuadComplex {
    float4 real; // Real part (QuadFloat)
    float4 imag; // Imaginary part (QuadFloat)
};


QuadComplex AddQuadComplex(QuadComplex a, QuadComplex b) {
    QuadComplex result;
    result.real = AddQuadFloat(a.real, b.real);
    result.imag = AddQuadFloat(a.imag, b.imag);
    return result;
}

QuadComplex MulQuadComplex(QuadComplex a, QuadComplex b) {
    float4 ac = MulQuadFloat(a.real, b.real);
    float4 bd = MulQuadFloat(a.imag, b.imag);
    float4 ad = MulQuadFloat(a.real, b.imag);
    float4 bc = MulQuadFloat(a.imag, b.real);

    QuadComplex result;
    result.real = AddQuadFloat(ac, MulQuadFloat(bd, -1.0)); // real = ac - bd
    result.imag = AddQuadFloat(ad, bc);                     // imag = ad + bc
    return result;
}

float4 AbsSqrQuadComplex(QuadComplex z) {
    float4 re2 = MulQuadFloat(z.real, z.real);
    float4 im2 = MulQuadFloat(z.imag, z.imag);
    return AddQuadFloat(re2, im2);
}
