Shader"Custom/GrabPackOnTop"
{
    Properties
    {
        _MainTex ("Albedo", 2D) = "white" {}
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _Color ("Color Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+100" }

        LOD 200

        Tags {"Queue"="Geometry+100"}

        ZTest Always
        ZWrite On
        

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        sampler2D _MainTex;
        sampler2D _BumpMap;
        fixed4 _Color;

    struct Input
    {
        float2 uv_MainTex;
        float2 uv_BumpMap;
    };

    void surf(Input IN, inout SurfaceOutputStandard o)
    {
        fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
        o.Albedo = c.rgb;
        o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
        o.Alpha = c.a;
    }
    ENDCG
    }

    FallBack"Standard"
    
    
    
}