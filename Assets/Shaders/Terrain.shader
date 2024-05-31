Shader "Custom/Terrain"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0, 10)) = 1.0
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
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);

            // Adjust brightness by multiplying the albedo color with a factor
            float brightnessFactor = 0.5; // Adjust this value as needed
            c.rgb *= brightnessFactor;

            o.Albedo = c.rgb;

            // Normal map
            fixed4 normalTex = tex2D(_NormalMap, IN.uv_MainTex);
            normalTex = normalTex * 2.0 - 1.0; // Expand from [0,1] to [-1,1]
            normalTex.rgb = normalize(normalTex.rgb);
            o.Normal = normalize(o.Normal + normalTex.rgb * _NormalStrength);
        }

        ENDCG
    }

    FallBack "Diffuse"
}
