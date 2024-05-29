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
            Cull Off
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

            StructuredBuffer<float4> positionBuffer;
            StructuredBuffer<float4> rotationBuffer;

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata_full v, uint id : SV_InstanceID)
            {
                float4 position = positionBuffer[id];
                float4 rotation = rotationBuffer[id];

                float3 quadPos = v.vertex;

                // Apply rotation
                float4x4 rotationMatrix = float4x4(
                    cos(rotation.y), 0, sin(rotation.y), 0,
                    0, 1, 0, 0,
                    -sin(rotation.y), 0, cos(rotation.y), 0,
                    0, 0, 0, 1
                );
                quadPos = mul(rotationMatrix, float4(quadPos, 1)).xyz;

                // Apply position
                quadPos += position.xyz;

                v2f o;
                o.pos = UnityObjectToClipPos(float4(quadPos, 1));
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.uv);
                // Perform alpha clipping
                if (texColor.a < 0.5) discard; // Adjust the threshold as needed
                return texColor;
            }
            ENDCG
        }
    }
}
