float2 complex_mul(float2 a, float2 b)
{
    return float2(a.x * b.x - a.y * b.y, a.x * b.y + a.y * b.x);
}

float2 complex_square(float2 a)
{
    return float2(a.x * a.x - a.y * a.y, 2 * a.x * a.y);
}

float2 complex_div(float2 a, float2 b)
{
    float2 aux = complex_mul(a, float2(b.x, -b.y));
    return aux / dot(b, b);
}

float2 complex_pow_int(float2 z, int pow)
{
    float2 answer = float2(1, 0);
    float2 aux = z.xy;
    int p2 = 1;
    while (p2 <= pow) {
        if ((p2 & pow) != 0)
            answer = complex_mul(answer, aux);
        aux = complex_square(aux);
        p2 <<= 1;
    }
    return answer;
}

float2 polar_to_rect(float2 r_theta)
{
    return float2(cos(r_theta.y), sin(r_theta.y)) * r_theta.x;
}

float2 polar_to_rect(float theta)
{
    return float2(cos(theta), sin(theta));
}

float2 rect_to_polar(float2 rect)
{
    return float2(length(rect), atan2(rect.y, rect.x));
}

float2 complex_pow(float2 z, float2 pow)
{
    float2 polar = complex_mul(float2(log(length(z)), atan2(z.y, z.x)), pow);
    polar.x = exp(polar.x);
    return polar_to_rect(polar);
}

float2 complex_exp(float2 z) {
    return float2(cos(z.y), sin(z.y)) * exp(z.x);
}

float2 complex_log(float2 val) {
    return float2(.5 * log(dot(val, val)), atan2(val.y, val.x));
}

float2 complex_polynomial(int count, float2 x, float4 coefficients[16]) 
{
    float2 answer = float2(0, 0);
    float2 acc = float2(1, 0);

    for (int i = 0; i < count; i++) {
        answer += complex_mul(acc, coefficients[i].xy);
        acc = complex_mul(acc, x);
    }
    return answer;
}

float2 complex_sin(float2 z) {
    float r = -(exp(-z.y) - exp(z.y)) / 2;
    return r * float2(sin(z.x), cos(z.x));
}

float2 complex_cos(float2 z) {
    float2 exps = float2(exp(-z.y), exp(z.y));
    return float2((exps.x + exps.y) * cos(z.x), (exps.x - exps.y) * sin(z.x)) / 2;
}