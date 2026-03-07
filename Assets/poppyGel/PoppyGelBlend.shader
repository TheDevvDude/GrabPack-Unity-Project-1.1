Shader"Custom/PoppyGelBlend"
{
    Properties
    {
        _BaseTex ("Base Albedo", 2D) = "white" {}
        _BaseNormal ("Base Normal", 2D) = "bump" {}

        _GelTex ("Gel Albedo", 2D) = "white" {}
        _GelNormal ("Gel Normal", 2D) = "bump" {}

        _MaskTex ("Mask (ID Map)", 2D) = "black" {}

        _GelSmoothness ("Gel Smoothness", Range(0,1)) = 0.9
        _BaseSmoothness ("Base Smoothness", Range(0,1)) = 0.3

        _GelEmissionMap ("Gel Emission Map", 2D) = "black" {}
        _GelEmissionStrength ("Gel Emission Strength", Range(0,5)) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        sampler2D _BaseTex;
        sampler2D _BaseNormal;
        sampler2D _GelTex;
        sampler2D _GelNormal;
        sampler2D _MaskTex;
        sampler2D _GelEmissionMap;
        half _GelEmissionStrength;

        half _GelSmoothness;
        half _BaseSmoothness;

        struct Input
        {
            float2 uv_BaseTex;
        };

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 baseCol = tex2D(_BaseTex, IN.uv_BaseTex);
            fixed4 gelCol = tex2D(_GelTex, IN.uv_BaseTex);
            fixed mask = tex2D(_MaskTex, IN.uv_BaseTex).r;

            fixed3 baseNormal = UnpackNormal(tex2D(_BaseNormal, IN.uv_BaseTex));
            fixed3 gelNormal = UnpackNormal(tex2D(_GelNormal, IN.uv_BaseTex));
    
            fixed3 emissionTex = tex2D(_GelEmissionMap, IN.uv_BaseTex).rgb;
            o.Emission = emissionTex * _GelEmissionStrength * mask;

            o.Albedo = lerp(baseCol.rgb, gelCol.rgb, mask);
            o.Normal = lerp(baseNormal, gelNormal, mask);
            o.Smoothness = lerp(_BaseSmoothness, _GelSmoothness, mask);
            o.Metallic = 0;
        }
        ENDCG
    }

    FallBack"Standard"
}