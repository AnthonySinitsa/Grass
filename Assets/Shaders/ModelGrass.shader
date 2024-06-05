Shader "Custom/ModelGrass"
{
    Properties
    {
        _Albedo1 ("Albedo 1", Color) = (1, 1, 1, 1)
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
            #include "UnityCG.cginc"

            struct GrassBlade
            {
                float3 position;
                float rotation;
                float height;
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

            fixed4 _Albedo1;

            v2f vert(appdata v)
            {
                v2f o;
                GrassBlade blade = grassBuffer[v.instanceID];

                float cosTheta = cos(blade.rotation);
                float sinTheta = sin(blade.rotation);
                float3 rotatedPosition = float3(
                    v.vertex.x * cosTheta - v.vertex.z * sinTheta,
                    v.vertex.y,
                    v.vertex.x * sinTheta + v.vertex.z * cosTheta
                );

                // Scale the vertex position by the blade's height
                rotatedPosition.y += blade.height;

                // Apply transformations based on instance data
                float4 worldPos = float4(blade.position + rotatedPosition, 1.0);
                o.pos = UnityObjectToClipPos(worldPos);
                o.uv = v.vertex.xy;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 normal = normalize(float3(0, 1, 0));

                float3 lightDir = normalize(float3(0.5, 1, 0.5));

                float diffuseFactor = max(0.0, dot(normal, lightDir));

                fixed4 diffuseColor = _Albedo1 * diffuseFactor;

                return diffuseColor;
            }
            ENDCG
        }
    }
}
