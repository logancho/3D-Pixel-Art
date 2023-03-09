Shader "Custom/InstancingDemoShader2"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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

                //Move from world space, to object space, by calculating pos
                float4 pos = mul(_Properties[instanceID].mat, i.vertex);

                //Transfer pos to 
                o.vertex = UnityObjectToClipPos(pos);
                o.color = _Properties[instanceID].color;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}
