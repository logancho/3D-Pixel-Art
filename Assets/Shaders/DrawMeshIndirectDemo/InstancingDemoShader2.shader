Shader "Custom/InstancingDemoShader2"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white"
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100

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
                float4 vertex   : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color    : COLOR;
            };
                        
            struct MeshProperties {
                float4x4 mat;
                float4 color;
            };


            StructuredBuffer<MeshProperties> _Properties;

            v2f vert (appdata i, uint instanceID: SV_InstanceID)
            {
                v2f o;

                //Finding correct object space coordinate of vertex
                float4 pos = mul(_Properties[instanceID].mat, i.vertex);
                float3 vpos = mul((float3x3)unity_ObjectToWorld, 5 * i.vertex.xyz);

				float4 worldCoord = float4(_Properties[instanceID].mat._m03,
                    _Properties[instanceID].mat._m13,
                    _Properties[instanceID].mat._m23, 1);

				float4 viewPos = mul(UNITY_MATRIX_V, worldCoord) + float4(vpos, 0);
				float4 outPos = mul(UNITY_MATRIX_P, viewPos);
                o.vertex = outPos;
                o.color = _Properties[instanceID].color;
                o.uv = i.uv;

                return o;
            }

            sampler2D _MainTex;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
                //return i.color;
            }
            ENDCG
        }
    }
}
