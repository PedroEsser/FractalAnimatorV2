// ============================================================================
// DD_Complex.hlsl
// Double-Double (hi, lo) arithmetic + Complex built on top
// ============================================================================

// ----- Double-Double base type: float2(hi, lo) -----
float2 dd_make(float x) {
    return float2(x, 0.0);
}

// Add two dd numbers
float2 dd_add(float2 a, float2 b) {
    float s = a.x + b.x;
    float v = s - a.x;
    float t = ((b.x - v) + (a.x - (s - v))) + a.y + b.y;
    float r_hi = s + t;
    float r_lo = t - (r_hi - s);
    return float2(r_hi, r_lo);
}

// Subtract two dd numbers
float2 dd_sub(float2 a, float2 b) {
    return dd_add(a, float2(-b.x, -b.y));
}

// Multiply dd by scalar float
float2 dd_mul_float(float2 a, float b) {
    float p = a.x * b;
    float e = (a.x * b - p) + a.y * b;
    float r_hi = p + e;
    float r_lo = e - (r_hi - p);
    return float2(r_hi, r_lo);
}

// Multiply two dd numbers
float2 dd_mul(float2 a, float2 b) {
    float p = a.x * b.x;
    float e = (a.x * b.x - p); // always ~0 in float
    e += a.x * b.y + a.y * b.x;
    float r_hi = p + e;
    float r_lo = e - (r_hi - p);
    return float2(r_hi, r_lo);
}

// Square a dd number
float2 dd_sqr(float2 a) {
    return dd_mul(a, a);
}

// Fused multiply-add in dd: a*b + c
float2 dd_fma(float2 a, float2 b, float2 c) {
    float2 ab = dd_mul(a, b);
    return dd_add(ab, c);
}

// ============================================================================
// Complex numbers with double-double components
// ============================================================================

struct DDComplex {
    float2 re;  // (hi, lo)
    float2 im;  // (hi, lo)
};

// Add two dd complex numbers
DDComplex dd_complex_add(DDComplex a, DDComplex b) {
    DDComplex r;
    r.re = dd_add(a.re, b.re);
    r.im = dd_add(a.im, b.im);
    return r;
}

// Subtract two dd complex numbers
DDComplex dd_complex_sub(DDComplex a, DDComplex b) {
    DDComplex r;
    r.re = dd_sub(a.re, b.re);
    r.im = dd_sub(a.im, b.im);
    return r;
}

// Multiply two dd complex numbers
DDComplex dd_complex_mul(DDComplex a, DDComplex b) {
    // (ar + i ai)*(br + i bi) = (ar*br - ai*bi) + i(ar*bi + ai*br)
    float2 arbr = dd_mul(a.re, b.re);
    float2 aibi = dd_mul(a.im, b.im);
    float2 real = dd_sub(arbr, aibi);

    float2 arbi = dd_mul(a.re, b.im);
    float2 aibr = dd_mul(a.im, b.re);
    float2 imag = dd_add(arbi, aibr);

    DDComplex r;
    r.re = real;
    r.im = imag;
    return r;
}

// Square a dd complex number
DDComplex dd_complex_sqr(DDComplex a) {
    // z^2
    float2 ar2 = dd_sqr(a.re);
    float2 ai2 = dd_sqr(a.im);
    float2 real = dd_sub(ar2, ai2);

    float2 two = dd_make(2.0);
    float2 imag = dd_mul(two, dd_mul(a.re, a.im));

    DDComplex r;
    r.re = real;
    r.im = imag;
    return r;
}
