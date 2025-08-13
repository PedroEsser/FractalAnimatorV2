float borderThreshold = 1e-3;

float4 hsv_to_rgb(float3 HSV)
{
    float var_h = HSV.x * 6;
    float var_i = floor(var_h);
    float var_1 = HSV.z * (1.0 - HSV.y);
    float var_2 = HSV.z * (1.0 - HSV.y * (var_h - var_i));
    float var_3 = HSV.z * (1.0 - HSV.y * (1 - (var_h - var_i)));

    if (var_i == 0) { return float4(HSV.z, var_3, var_1, 1); }
    else if (var_i == 1) { return float4(var_2, HSV.z, var_1, 1); }
    else if (var_i == 2) { return float4(var_1, HSV.z, var_3, 1); }
    else if (var_i == 3) { return float4(var_1, var_2, HSV.z, 1); }
    else if (var_i == 4) { return float4(var_3, var_1, HSV.z, 1); }
    else { return float4(HSV.z, var_1, var_2, 1); }
}

bool AtBorder(float4 pos, float threshold) {
    return pos.x < threshold || pos.y < threshold || (1 - pos.x) < threshold || (1 - pos.y) < threshold;
}

bool OnCenterLines(float2 pos) {
    return abs(pos.x) < 0.005 || abs(pos.y) < 0.005;
}

bool OnGridLines(float2 pos) {
    pos.x = abs(pos.x);
    pos.y = abs(pos.y);
    pos *= 4;
    return (pos.x - floor(pos.x)) < .03 || (pos.y - floor(pos.y)) < .03;
}