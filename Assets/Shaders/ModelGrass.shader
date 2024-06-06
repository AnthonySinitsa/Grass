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

            #include "UnityPBSLighting.cginc"
            #include "AutoLight.cginc"
            #include "UnityCG.cginc"
            #include "../Resources/Bezier.cginc"

            struct GrassBlade
            {
                float3 position;
                float facing;
                float tilt;
                float3 midpoint;
            };

            StructuredBuffer<GrassBlade> grassBuffer;

            struct appdata
            {
                uint instanceID : SV_InstanceID;
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
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

                // Apply rotation
                float3 rotatedPosition = Rotate(tiltedPosition, blade.facing);

                // Scale the vertex position by the blade's height and add the blade's position
                rotatedPosition.y += blade.position.y;
                float4 worldPos = float4(blade.position + rotatedPosition, 1.0);
                o.pos = UnityObjectToClipPos(worldPos);
                o.uv = v.vertex.xy;

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
