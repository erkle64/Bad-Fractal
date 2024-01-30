Shader "Hidden/JuliaSet"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
            #pragma target 3.0
            #pragma exclude_renderers d3d11_9x
            #pragma exclude_renderers d3d9


            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float _Aspect;
            float4 _Trap;
            float4 _Seed;
            float2 _Speed;
            float _Fade;
            float3 _Background;

            float3 HSVtoRGB(float3 hsv)
            {
                const float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(hsv.xxx + K.xyz) * 6.0 - K.www);
                return hsv.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), hsv.y);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                const int iterations = 200;
                int limit = iterations;
                float4 col = float4(0.0, 0.0, 0.0, 0.0);
                float2 c = float2(_Seed.x - _Seed.z * cos(_Time.y * _Speed.x), _Seed.y + _Seed.w * sin(_Time.y * _Speed.y));
                float2 z = (i.uv * 2.0 - 1.0) * float2(_Aspect, 1.0);
                z = /*float2(0.5, -0.05) + */z * 0.75;
                float minDistance = 1.0;
                float brightness = 1.0;
                [loop]
                while (limit-- > 0)
                {
                    z = float2(z.x * z.x - z.y * z.y, 2.0 * z.x * z.y) + c;
                    if (length(z) > 4.0) break;
                    float2 t = z * _Trap.xy + _Trap.zw;
                    if (all(saturate(t) == t))
                    {
                        float distance = tex2Dlod(_MainTex, float4(t, 0.0, 0.0)).r;
                        if (distance == 0)
                        {
                            col = float4(brightness, brightness, brightness, 1.0);
                            break;
                        }
                        minDistance = min(distance, minDistance);
                    }
                    brightness *= _Fade;
                }

                return lerp(float4(HSVtoRGB(float3(minDistance * _Background.x, _Background.y, _Background.z)), 1.0), col, col.a);
            }
            ENDCG
        }
    }
}
