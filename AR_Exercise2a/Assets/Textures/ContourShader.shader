// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/ContourShader"
{
    Properties
    {
        _MainTex("", 2D) = "white" {}
		_NoiseTex ("Noise texture", 2D) = "grey" {}
        _LineColor("", Color) = (0, 0, 0, 1)
        _FillColor1("", Color) = (0, 0, 1)
        _FillColor2("", Color) = (1, 0, 0)
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _CameraGBufferTexture2;

    sampler2D _MainTex;
	sampler2D _NoiseTex;
    float4 _MainTex_TexelSize;

    float4 _LineColor;
    float3 _FillColor1;
    float3 _FillColor2;
    float2 _Threshold;

    fixed RobertsCross(sampler2D tex, float2 uv)
    {
        float4 duv = float4(0, 0, _MainTex_TexelSize.xy);

        half n11 = tex2D(tex, uv + duv.xy).g;
        half n12 = tex2D(tex, uv + duv.zy).g;
        half n21 = tex2D(tex, uv + duv.xw).g;
        half n22 = tex2D(tex, uv + duv.zw).g;

        half gx = n11 - n22;
        half gy = n12 - n21;

        half g = sqrt(gx * gx + gy * gy);

        return saturate((g - _Threshold.x) * _Threshold.y);
    }

    fixed4 frag(v2f_img i) : SV_Target
    {
        fixed edge = RobertsCross(_CameraGBufferTexture2, i.uv);
        fixed luma = dot(tex2D(_MainTex, i.uv).rgb, 1.0 / 2);
        fixed3 fill = luma > 0.50 ? _FillColor1 : _FillColor2;
        fixed4 _ContourTex = fixed4(lerp(fill, _LineColor.rgb, edge * _LineColor.a), 1);

		half4 noisecol = tex2D(_NoiseTex, i.uv + 100* _Time.x*_Time.x * _Time.y*_Time.y);
		half4 texWithNoisecol = lerp(noisecol, _ContourTex, 0.90);

		return _ContourTex;
    }

    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            ENDCG
        }
    }
}