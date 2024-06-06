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

            struct GrassBlade
            {
                float3 position;
                float facing;
                float tilt;
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

            v2f vert(appdata v)
            {
                v2f o;
                GrassBlade blade = grassBuffer[v.instanceID];

                float cosTheta = cos(blade.facing);
                float sinTheta = sin(blade.facing);

                // Rotate the vertex position based on the facing direction
                float3 rotatedPosition = float3(
                    v.vertex.x * cosTheta - v.vertex.z * sinTheta,
                    v.vertex.y,
                    v.vertex.x * sinTheta + v.vertex.z * cosTheta
                );

                // Apply tilt around the local X axis
                float tiltAngle = blade.tilt;
                float cosTilt = cos(tiltAngle);
                float sinTilt = sin(tiltAngle);
                float3 tiltedPosition = float3(
                    rotatedPosition.x,
                    rotatedPosition.y * cosTilt - rotatedPosition.z * sinTilt,
                    rotatedPosition.y * sinTilt + rotatedPosition.z * cosTilt
                );

                // Scale the vertex position by the blade's height and add the blade's position
                tiltedPosition.y += blade.position.y;
                

                // Apply transformations based on instance data
                float4 worldPos = float4(blade.position + tiltedPosition, 1.0);
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
