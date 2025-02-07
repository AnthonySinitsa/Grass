Shader "Custom/ModelGrass"
{
    Properties
    {
        _Albedo1 ("Albedo 1", Color) = (1, 1, 1, 1)
        _Albedo2 ("Albedo 2", Color) = (1, 1, 1, 1)
        _AOColor ("Ambient Occlusion", Color) = (1, 1, 1)
        _TipColor ("Tip Color", Color) = (1, 1, 1)
        _Control1Height ("Control Point 1 Height", Range(0, 1)) = 0.2
        _Control2Height ("Control Point 2 Height", Range(0, 1)) = 0.7
        _TipHeight ("Tip Height", Range(0, 2)) = 1.0
        _Control2Offset ("Control Point 2 Offset", Range(-1, 1)) = 0
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
            float _Control1Height, _Control2Height, _TipHeight, _Control2Offset;


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


            float3 RotateAroundAxis(float3 position, float3 axis, float angle) 
            {
                float3 a = normalize(axis);
                float s = sin(angle);
                float c = cos(angle);
                float r = 1.0 - c;
                
                float3x3 m = float3x3(
                    a.x * a.x * r + c,
                    a.y * a.x * r + a.z * s,
                    a.z * a.x * r - a.y * s,
                    a.x * a.y * r - a.z * s,
                    a.y * a.y * r + c,
                    a.z * a.y * r + a.x * s,
                    a.x * a.z * r + a.y * s,
                    a.y * a.z * r - a.x * s,
                    a.z * a.z * r + c
                );
                
                return mul(m, position);
            }

            float3 PreserveLengthBend(float3 basePos, float3 tipPos, float3 windEffect, float t) 
            {
                // Original length
                float originalLength = length(tipPos - basePos);
                
                // Calculate wind direction and strength
                float3 windDir = normalize(windEffect);
                float windStrength = length(windEffect);
                
                // Calculate rotation axis (perpendicular to both up vector and wind direction)
                float3 rotationAxis = normalize(cross(float3(0, 1, 0), windDir));
                
                // Calculate bend angle based on wind strength
                float bendAngle = atan(windStrength) * t;
                
                // Get direction to tip
                float3 tipDir = normalize(tipPos - basePos);
                
                // Rotate the tip direction around the rotation axis
                float3 bentDir = normalize(RotateAroundAxis(tipDir, rotationAxis, bendAngle));
                
                // Calculate new position maintaining original length
                float3 newPos = lerp(basePos, basePos + bentDir * originalLength, t);
                
                return newPos;
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
                
                // Identify base vertex
                bool isBaseVertex = v.vertex.y < 0.01;
                
                // Wind influence increases with height
                float windInfluence = saturate(v.vertex.y);
                
                // Original tip position without wind
                float3 originalTipPos = basePos + float3(0, _TipHeight, 0);
                
                // Calculate bent position preserving length
                float3 bentPos = PreserveLengthBend(basePos, originalTipPos, windEffect * windInfluence, v.vertex.y);
                
                // Control points with exposed parameters
                float baseToTip = bentPos - basePos;
                float3 controlPos1 = basePos + baseToTip * _Control1Height;
                float3 controlPos2 = basePos + baseToTip * _Control2Height + 
                                    float3(_Control2Offset * (1.0 - v.vertex.y), 0, blade.bend) * (1.0 - windInfluence);
                
                if (isBaseVertex) {
                    controlPos1 = basePos;
                    controlPos2 = basePos;
                    bentPos = basePos;
                }
                
                // Calculate position on the Bezier curve
                float t = rotatedPosition.y;
                float3 curvePos = CurveSolve(basePos, controlPos1, controlPos2, bentPos, t);
                
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
