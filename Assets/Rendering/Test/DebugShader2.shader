Shader "Hidden/DebugShader2"
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

            //RWTexture2D<float4> _ReadWriteTexture;
            //RWTexture2D<float4> _SomeOtherReadWriteTexture;
            sampler2D _TestTex;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 col2 = tex2D(_TestTex, i.uv);
                //col2.rgb = 1 - col2.rgb;
                // just invert the colors
                //col.rgb = 1 - col.rgb;
                col.rgb = (col.rgb - col2.rgb);
                //col.rgb = col2.rgb;
                return col;
            }
            ENDCG
        }
    }
}
