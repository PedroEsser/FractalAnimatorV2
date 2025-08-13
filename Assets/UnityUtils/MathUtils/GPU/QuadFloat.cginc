// A QuadFloat packed in float4: a is most significant, d is least
#define qf_a qf.x
#define qf_b qf.y
#define qf_c qf.z
#define qf_d qf.w

void Split(float x, out float hi, out float lo) {
    float t = x * 8193.0;
    hi = t - (t - x);
    lo = x - hi;
}

void TwoSum(float a, float b, out float sum, out float err) {
    sum = a + b;
    float bb = sum - a;
    err = (a - (sum - bb)) + (b - bb);
}

void TwoProduct(float a, float b, out float prod, out float err) {
    prod = a * b;
    float a_hi, a_lo, b_hi, b_lo;
    Split(a, a_hi, a_lo);
    Split(b, b_hi, b_lo);
    err = ((a_hi * b_hi - prod) + a_hi * b_lo + a_lo * b_hi) + a_lo * b_lo;
}

float4 Renormalize(float a, float b, float c, float d) {
    float s, e;

    TwoSum(a, b, s, e);
    a = s; b = e;
    TwoSum(a, c, s, e);
    a = s; c = e;
    TwoSum(a, d, s, e);
    a = s; d = e;

    return float4(a, b + c, d, 0.0);
}

float4 AddQuadFloat(float4 x, float4 y) {
    float s0, e0, s1, e1, s2, e2, s3, e3;
    TwoSum(x.x, y.x, s0, e0);
    TwoSum(x.y, y.y + e0, s1, e1);
    TwoSum(x.z, y.z + e1, s2, e2);
    TwoSum(x.w, y.w + e2, s3, e3);
    return Renormalize(s0, s1, s2, s3 + e3);
}

float4 MulQuadFloat(float4 x, float4 y) {
    float p00, e00, p01, e01, p02, e02, p03, e03;
    float p10, e10, p11, e11, p12, e12, p13, e13;

    TwoProduct(x.x, y.x, p00, e00);
    TwoProduct(x.x, y.y, p01, e01);
    TwoProduct(x.x, y.z, p02, e02);
    TwoProduct(x.x, y.w, p03, e03);

    TwoProduct(x.y, y.x, p10, e10);
    TwoProduct(x.y, y.y, p11, e11);
    TwoProduct(x.y, y.z, p12, e12);
    TwoProduct(x.y, y.w, p13, e13);

    float p20 = x.z * y.x;
    float p21 = x.z * y.y;
    float p22 = x.z * y.z;
    float p23 = x.z * y.w;

    float p30 = x.w * y.x;
    float p31 = x.w * y.y;
    float p32 = x.w * y.z;
    float p33 = x.w * y.w;

    float t0 = p00;
    float t1 = p01 + p10 + e00;
    float t2 = p02 + p11 + p20 + p12 + p21 + e01 + e10;
    float t3 = p03 + p13 + p22 + p30 + p23 + p31 + e02 + e11 + e12 + e13;
    float t4 = p32 + p33 + e03;

    return Renormalize(t0, t1, t2, t3 + t4);
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
