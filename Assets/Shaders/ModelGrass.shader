Shader "Custom/ModelGrass"
{
    Properties
    {
        _Albedo1 ("Albedo 1", Color) = (1, 1, 1, 1)
        _Albedo2 ("Albedo 2", Color) = (1, 1, 1, 1)
        _AOColor ("Ambient Occlusion", Color) = (1, 1, 1)
        _TipColor ("Tip Color", Color) = (1, 1, 1)
    }
    SubShader
    {
        Cull Off
        Zwrite On

        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "UnityPBSLighting.cginc"
            #include "AutoLight.cginc"
            #include "UnityCG.cginc"
            #include "../Resources/Bezier.cginc"

            struct GrassBlade
            {
                float3 position;
                float facing;
                float tilt;
                float bend;
                float width;
            };

            StructuredBuffer<GrassBlade> grassBuffer;
            StructuredBuffer<float3> windBuffer;

            struct appdata
            {
                uint instanceID : SV_InstanceID;
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
            };

            fixed4 _Albedo1, _Albedo2, _AOColor, _TipColor;


            float3 Tilt(float3 vertex, float tiltAngle)
            {
                float cosTilt = cos(tiltAngle);
                float sinTilt = sin(tiltAngle);

                return float3(
                    vertex.x,
                    vertex.y * cosTilt - vertex.z * sinTilt,
                    vertex.y * sinTilt + vertex.z * cosTilt
                );
            }

            float3 Rotate(float3 vertex, float facingAngle)
            {
                float cosTheta = cos(facingAngle);
                float sinTheta = sin(facingAngle);

                return float3(
                    vertex.x * cosTheta - vertex.z * sinTheta,
                    vertex.y,
                    vertex.x * sinTheta + vertex.z * cosTheta
                );
            }


            v2f vert(appdata v)
            {
                v2f o;
                GrassBlade blade = grassBuffer[v.instanceID];
                
                // Apply tilt
                float3 tiltedPosition = Tilt(v.vertex.xyz, blade.tilt);
                
                // Get wind effect
                float3 windEffect = windBuffer[v.instanceID];
                
                // Apply rotation
                float3 rotatedPosition = Rotate(tiltedPosition, blade.facing);
                
                // Base position stays fixed
                float3 basePos = blade.position;
                
                // Identify base vertex (assuming it's the lowest y-value vertex)
                bool isBaseVertex = v.vertex.y < 0.01; // Small threshold to account for floating point precision
                
                // Control points are affected by wind with increasing influence based on height
                float windInfluence = saturate(v.vertex.y); // Clamp between 0 and 1
                
                // If this is a base vertex, don't apply any wind or movement
                float3 controlPos1 = isBaseVertex ? basePos : basePos + float3(0, 0.5, 0);
                float3 controlPos2 = isBaseVertex ? basePos : basePos + Rotate(float3(0, 0.7, blade.bend), blade.facing) + 
                                     windEffect * (windInfluence * windInfluence) * 0.5;
                float3 tipPos = isBaseVertex ? basePos : basePos + float3(0, 1.0, 0) + 
                                windEffect * windInfluence;
                
                // Calculate t based on vertex's y position
                float t = rotatedPosition.y;
                
                // Calculate the position on the Bezier curve
                float3 curvePos = CurveSolve(basePos, controlPos1, controlPos2, tipPos, t);
                
                // Final position
                float4 worldPos = float4(rotatedPosition + curvePos, 1.0);
                o.pos = UnityObjectToClipPos(worldPos);
                o.uv = v.uv;
                
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 col = lerp(_Albedo1, _Albedo2, i.uv.y);
                float3 lightDir = _WorldSpaceLightPos0.xyz;
                float ndotl = DotClamped(lightDir, normalize(float3(0, 1, 0)));

                float4 ao = lerp(_AOColor, 1.0, i.uv.y);
                float4 tip = lerp(0.0, _TipColor, i.uv.y * i.uv.y);

                float4 grassColor = (col + tip) * ndotl * ao;

                return grassColor;
            }
            ENDCG
        }
    }
}
