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

            #include "UnityPBSLighting.cginc"
            #include "AutoLight.cginc"
            #include "UnityCG.cginc"
            #include "../Resources/Random.cginc"

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float saturationLevel : TEXCOORD1;
            };
            

            StructuredBuffer<float4> grassBuffer;

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata_full v, uint id : SV_InstanceID)
            {
                float4 position = grassBuffer[id * 2];
                float4 rotation = grassBuffer[id * 2 + 1];

                float3 quadPos = v.vertex;

                // Apply rotation
                float cosAngle = cos(radians(rotation.y));
                float sinAngle = sin(radians(rotation.y));
                float4x4 rotationMatrix = float4x4(
                    cosAngle, 0, sinAngle, 0,
                    0, 1, 0, 0,
                    -sinAngle, 0, cosAngle, 0,
                    0, 0, 0, 1
                );
                quadPos = mul(rotationMatrix, float4(quadPos, 1)).xyz;

                // Apply position
                quadPos += position.xyz;

                v2f o;
                o.pos = UnityObjectToClipPos(float4(quadPos, 1));
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.saturationLevel = 1.0 - ((grassBuffer[id].w - 1.0f) / 1.5f);
                o.saturationLevel = max(o.saturationLevel, 0.5f);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                clip(-(0.5 - col.a));

                float luminance = LinearRgbToLuminance(col);

                float saturation = lerp(1.0f, i.saturationLevel, i.uv.y * i.uv.y * i.uv.y);
                col.r /= saturation;
                
             
                float3 lightDir = _WorldSpaceLightPos0.xyz;
                float ndotl = DotClamped(lightDir, normalize(float3(0, 1, 0)));
                
                return col * ndotl;
            }
            ENDCG
        }
    }
}
