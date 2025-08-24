Shader "Fractals/QuadMandelbrot"
{
    Properties
    {
        _Center_Real ("Center Real", Vector) = (0, 0, 0, 0)
        _Center_Imag ("Center Imag", Vector) = (0, 0, 0, 0)
        _PixelDx ("Pixel Dx", Vector) = (0, 0, 0, 0)
        _PixelDy ("Pixel Dy", Vector) = (0, 0, 0, 0)
        _Iterations ("Iterations", Int) = 20
        _EscapeRadiusSquared ("Escape Radius Squared", Float) = 400
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets/UnityUtils/MathUtils/GPU/QuadFloat.cginc"
            #include "Assets/UnityUtils/MathUtils/GPU/QuadComplex.cginc"
            #include "Assets/UnityUtils/MathUtils/GPU/QuadFractals.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            sampler2D _MainTex;
            float4 _Center_Real;
            float4 _Center_Imag;
            float4 _PixelDx;
            float4 _PixelDy;
            int _Iterations;
            float _EscapeRadiusSquared;

            QuadComplex PixelToComplex(v2f i)
            {
                int2 pix = int2(i.pos.xy);
                int2 halfRes = int2(_ScreenParams.xy * 0.5);

                int ox = pix.x - halfRes.x;
                int oy = pix.y - halfRes.y;

                // Multiply every limb by the integer offset using the scalar overload.
                float4 c_real = AddQuadFloat(_Center_Real, MulQuadFloat(_PixelDx, (float)ox));
                float4 c_imag = AddQuadFloat(_Center_Imag, MulQuadFloat(_PixelDy, (float)oy));

                QuadComplex c;
                c.real = c_real;
                c.imag = c_imag;
                return c;
            }

            float4 QuadToColor(float4 q) {
                return float4(q.x, q.y * 1e34, q.z * 1e42, 1);
            }


            fixed4 frag (v2f i) : SV_Target
            {
                float4 r = MulQuadFloat(_PixelDx, 1);
                return QuadToColor(r);
                //return QuadToColor(MulQuadFloat(_PixelDx, 1.0f));
                //return QuadToColor(_PixelDx);
                //return QuadToColor(c.real);

                QuadComplex c = PixelToComplex(i);
                QuadComplex finalZ;
                int iterations = MandelbrotIterations(c, _Iterations, _EscapeRadiusSquared, finalZ);
                
                if (iterations >= _Iterations) {
                    return float4(0, 0, 0, 1); // Inside set
                } else {
                    // Get the magnitude of the final z value
                    float4 z_mag2 = AbsSqrQuadComplex(finalZ);
                    float z_mag = sqrt(z_mag2.x + z_mag2.y + z_mag2.z + z_mag2.w);
                    
                    // Safe smooth coloring using the final z magnitude
                    float smoothIter = (float)iterations;
                    if (z_mag > 1e-10) {
                        smoothIter = (float)iterations + 1.0 - log(log(z_mag)) / log(2.0);
                    }
                    
                    float color = smoothIter / (float)_Iterations;
                    color = saturate(color);
                    
                    // Create a nice color gradient
                    return float4(color, color * 0.7, color * 0.3, 1);
                }
            }
            ENDCG
        }
    }
}
