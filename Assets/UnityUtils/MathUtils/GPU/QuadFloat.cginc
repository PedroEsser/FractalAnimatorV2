static const float SPLIT = 8193.0;

void Split(float x, out float hi, out float lo) {
    float t = x * SPLIT;
    hi = t - (t - x);
    lo = x - hi;
}

void TwoSum(float a, float b, out float s, out float err) {
    s = a + b;
    float bb = s - a;
    err = (a - (s - bb)) + (b - bb);
}

void QuickTwoSum(float a, float b, out float s, out float e) {
    s = a + b;
    e = b - (s - a);
}

void TwoProduct(float a, float b, out float p, out float err) {
    p = a * b;
    float a_hi, a_lo, b_hi, b_lo;
    Split(a, a_hi, a_lo);
    Split(b, b_hi, b_lo);
    err = ((a_hi * b_hi - p) + a_hi * b_lo + a_lo * b_hi) + a_lo * b_lo;
}

// ============================================================
// QuadFloat operations (robust version)
// ============================================================

// Robust 4-term renormalizer (Shewchuk style cascade)
float4 Renormalize4_exact(float a, float b, float c, float d) {
    float s0,e0; TwoSum(a,b,s0,e0);
    float s1,e1; TwoSum(s0,c,s1,e1);
    float s2,e2; TwoSum(s1,d,s2,e2);
    return float4(s2, e2, e1, e0);
}

// =========================
// Addition
// =========================
float4 AddQuadFloat(float4 x, float4 y) {
    float s0,e0; TwoSum(x.x, y.x, s0,e0);
    float s1,e1; TwoSum(x.y, y.y, s1,e1);
    float s2,e2; TwoSum(x.z, y.z, s2,e2);
    float s3,e3; TwoSum(x.w, y.w, s3,e3);

    // Collect into limbs + carry errors
    float t0 = s0;
    float t1 = s1 + e0;
    float t2 = s2 + e1;
    float t3 = s3 + e2 + e3;

    return Renormalize4_exact(t0,t1,t2,t3);
}

// =========================
// Multiply by scalar
// =========================
float4 MulQuadFloat(float4 x, float y) {
    float p0,e0; TwoProduct(x.x,y,p0,e0);
    float p1,e1; TwoProduct(x.y,y,p1,e1);
    float p2,e2; TwoProduct(x.z,y,p2,e2);
    float p3,e3; TwoProduct(x.w,y,p3,e3);

    float t0 = p0;
    float t1 = p1 + e0;
    float t2 = p2 + e1;
    float t3 = p3 + e2 + e3;

    return Renormalize4_exact(t0,t1,t2,t3);
}

// =========================
// Multiply by another quad
// =========================
float4 MulQuadFloat(float4 x, float4 y) {
    float p00,e00; TwoProduct(x.x,y.x,p00,e00);

    float p01,e01; TwoProduct(x.x,y.y,p01,e01);
    float p10,e10; TwoProduct(x.y,y.x,p10,e10);

    float p02,e02; TwoProduct(x.x,y.z,p02,e02);
    float p11,e11; TwoProduct(x.y,y.y,p11,e11);
    float p20,e20; TwoProduct(x.z,y.x,p20,e20);

    float p03,e03; TwoProduct(x.x,y.w,p03,e03);
    float p12,e12; TwoProduct(x.y,y.z,p12,e12);
    float p21,e21; TwoProduct(x.z,y.y,p21,e21);
    float p30,e30; TwoProduct(x.w,y.x,p30,e30);

    // Collect terms roughly by magnitude
    float t0 = p00;
    float t1 = p01 + p10 + e00;
    float t2 = p02 + p11 + p20 + e01 + e10;
    float t3 = p03 + p12 + p21 + p30 + e02 + e11 + e20
               + e03 + e12 + e21 + e30;

    return Renormalize4_exact(t0,t1,t2,t3);
}


float4 ReciprocalQuadFloat(float4 y) {
    float y_approx = 1.0 / y.x;
    float4 qy = float4(y_approx, 0, 0, 0);
    float4 one = float4(1, 0, 0, 0);
    float4 yqy = MulQuadFloat(y, qy);
    float4 diff = AddQuadFloat(one, MulQuadFloat(qy, AddQuadFloat(one, MulQuadFloat(yqy, -1.0))));
    return MulQuadFloat(qy, diff);
}

float4 DivQuadFloat(float4 x, float4 y) {
    return MulQuadFloat(x, ReciprocalQuadFloat(y));
}