Shader "Hidden/JFASeed"
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

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            float4 frag(v2f i) : SV_Target
            {
                float2 coord = (i.uv - 0.375) * 4.0;
                float pixel = tex2D(_MainTex, coord).r;
                return (all(saturate(coord) == coord) && pixel < 0.5) ? float4(i.uv, 0.0, 0.0) : float4(-1.0, -1.0, 0.0, 0.0);
            }
            ENDCG
        }
    }
}
