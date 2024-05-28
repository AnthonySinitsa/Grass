Shader "Unlit/BillboardGrass"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            struct GrassData
            {
                float3 position;
                float2 uv;
            };

            StructuredBuffer<GrassData> grassBuffer;

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (uint id : SV_VertexID)
            {
                GrassData data = grassBuffer[id / 6];

                v2f o;
                float3 billboardPos = data.position;
                float3 cameraRight = float3(1, 0, 0);
                float3 cameraUp = float3(0, 1, 0);

                float3 quadPos = billboardPos;
                quadPos += (id % 2 == 0 ? -0.5 : 0.5) * cameraRight;
                quadPos += (id / 2 % 2 == 0 ? -0.5 : 0.5) * cameraUp;

                o.pos = UnityObjectToClipPos(float4(quadPos, 1));
                o.uv = TRANSFORM_TEX(data.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
