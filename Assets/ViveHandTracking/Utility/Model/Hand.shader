Shader "ViveHandTracking/Hand"
{
  Properties
  {
    _MainColor("Main Color", Color) = (1, 1, 1, 1)
     _RimColor("Rim Color", Color) = (0.17, 0.36, 0.81, 0.0)
    _RimPower("Rim Power", Range(0.6, 36.0)) = 8.0
    _RimIntensity("Rim Intensity", Range(0.0, 100.0)) = 1.0
  }

  SubShader
  {

    Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "IngnoreProjector" = "True" }
    ZWrite Off
    Blend SrcAlpha OneMinusSrcAlpha
    CGPROGRAM
    #pragma surface surf Lambert  alpha

    struct Input
    {
       float3 viewDir;
    };


    float4 _MainColor;
    float4 _RimColor;
    float _RimPower;
    float _RimIntensity;

    void surf(Input IN, inout SurfaceOutput o) {
      o.Albedo = _MainColor.rgb;
      half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
      o.Emission = _RimColor.rgb * pow(rim, _RimPower)*_RimIntensity;
      o.Alpha = _MainColor.a;
    }
    ENDCG
  }

   Fallback "Diffuse"
}
