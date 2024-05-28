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
            #pragma multi_compile_instancing

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

            v2f vert (appdata_full v, uint id : SV_InstanceID)
            {
                GrassData data = grassBuffer[id];

                v2f o;
                float3 billboardPos = data.position;

                // For simplicity, just place the quad directly
                float3 quadPos = v.vertex + billboardPos;
                o.pos = UnityObjectToClipPos(float4(quadPos, 1));

                // Pass UVs from the vertex data
                o.uv = v.texcoord.xy;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv);
                // return float4(i.uv, 0, 1);
            }
            ENDCG
        }
    }
}
