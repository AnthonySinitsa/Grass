Shader "Unlit/BillboardGrass"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WindStrength ("Wind Strength", Range(0.5, 50.0)) = 1
        _CullingBias ("Cull Bias", Range(0.1, 1.0)) = 0.5
        _LODCutoff ("LOD Cutoff", Range(10.0, 500.0)) = 100
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
            
            struct GrassData {
                float4 position;
                float4 rotation;
            };

            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            StructuredBuffer<GrassData> grassBuffer;
            float _Rotation, _WindStrength, _CullingBias, _DisplacementStrength, _LODCutoff;



            // Function to rotate individual grass blades
            float4 RotateAroundYInDegrees (float4 vertex, float degrees) {
                float alpha = degrees * UNITY_PI / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float4(mul(m, vertex.xz), vertex.yw).xzyw;
            }

            bool VertexIsBelowClipPlane(float3 p, int planeIndex, float bias) {
                float4 plane = unity_CameraWorldClipPlanes[planeIndex];

                return dot(float4(p, 1), plane) < bias;
            }   

            bool cullVertex(float3 p, float bias) {
                return  distance(_WorldSpaceCameraPos, p) > _LODCutoff ||
                        VertexIsBelowClipPlane(p, 0, bias) ||
                        VertexIsBelowClipPlane(p, 1, bias) ||
                        VertexIsBelowClipPlane(p, 2, bias) ||
                        VertexIsBelowClipPlane(p, 3, -max(1.0f, _DisplacementStrength));
            }



            v2f vert (appdata_full v, uint id : SV_InstanceID)
            {
                v2f o;

                float4 grassPosition = grassBuffer[id].position;
                float4 grassRotation = grassBuffer[id].rotation;
                float3 localPosition = RotateAroundYInDegrees(v.vertex, grassRotation).xyz;
                float localWindVariance = min(max(0.4, randValue(id)), 0.75);

                float cosTime;
                if (localWindVariance > 0.6)
                    cosTime = cos(_Time.y * (_WindStrength - (grassPosition.w - 1.0)));
                else
                    cosTime = cos(_Time.y * ((_WindStrength - (grassPosition.w - 1.0)) + localWindVariance * 0.1));

                float trigValue = ((cosTime * cosTime) * 0.65) - localWindVariance * 0.5;

                localPosition.x += v.texcoord.y * trigValue * grassPosition.w * localWindVariance * 0.6;
                localPosition.z += v.texcoord.y * trigValue * grassPosition.w * 0.4;
                localPosition.y *= v.texcoord.y * (0.5 + grassPosition.w);

                float4 worldPosition = float4(grassPosition.xyz + localPosition, 1.0);

                if (cullVertex(worldPosition, -_CullingBias * max(1.0, _DisplacementStrength)))
                    o.pos = 0.0;
                else
                    o.pos = UnityObjectToClipPos(worldPosition);



                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.saturationLevel = 1.0 - ((grassBuffer[id].position.w - 1.0) / 1.5);
                o.saturationLevel = max(o.saturationLevel, 0.5);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                clip(-(0.5 - col.a));

                float luminance = LinearRgbToLuminance(col);

                float saturation = lerp(1.0, i.saturationLevel, i.uv.y * i.uv.y * i.uv.y);
                col.r /= saturation;
                
             
                float3 lightDir = _WorldSpaceLightPos0.xyz;
                float ndotl = DotClamped(lightDir, normalize(float3(0, 1, 0)));
                
                return col * ndotl;
            }
            ENDCG
        }
    }
}
