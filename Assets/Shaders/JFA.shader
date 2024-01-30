Shader "Hidden/JFA"
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

            sampler2D_float _MainTex;
            float4 _MainTex_TexelSize;
            float _KernelSize;

            float4 frag(v2f i) : SV_Target
            {
                float minDistance = 100000000.0;
                float2 nearestSeed = float2(-1.0, -1.0);
                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        float2 coord = i.uv + float2(x, y) * _KernelSize * _MainTex_TexelSize.xy;
                        if (all(saturate(coord) == coord))
                        {
                            float2 seed = tex2D(_MainTex, coord).rg;
                            if (seed.x > -0.5)
                            {
                                float distance = length(i.uv - seed);
                                if (distance < minDistance)
                                {
                                    minDistance = distance;
                                    nearestSeed = seed;
                                }
                            }
                        }
                    }
                }

                return float4(nearestSeed, 0.0, 1.0);
            }
            ENDCG
        }
    }
}
