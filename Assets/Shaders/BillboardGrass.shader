Shader "Unlit/BillboardGrass" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader {
        Cull Off
        Zwrite On

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma target 4.5

            struct VertexData {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            struct GrassData {
                float4 position;
                float2 uv;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            StructuredBuffer<GrassData> _GrassDataBuffer;
            int _Dimension;

            v2f vert (VertexData v, uint instanceID : SV_InstanceID) {
                v2f o;
            
                GrassData data = _GrassDataBuffer[instanceID];
                float4 worldPosition = float4(data.position.x, 0.0, data.position.z, 1.0);

                o.vertex = UnityObjectToClipPos(worldPosition);
                o.uv = v.uv;
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }

            ENDCG
        }
    }
}
