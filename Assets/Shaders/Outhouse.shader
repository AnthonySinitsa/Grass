Shader "Custom/OuthouseShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0, 1)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        sampler2D _MainTex;
        sampler2D _NormalMap;
        float _NormalStrength;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_NormalMap;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo
            half4 c = tex2D (_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;

            // Normal Map with adjustable strength
            half3 normal = UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap));
            normal = lerp(half3(0, 0, 1), normal, _NormalStrength);
            o.Normal = normal;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
