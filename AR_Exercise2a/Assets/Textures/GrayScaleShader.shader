﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/GrayScaleShader"
{
    Properties {
     _MainTex ("Texture", 2D) = "white" { }
    _NoiseTex ("Noise texture", 2D) = "grey" {}
 }
 SubShader {
     Pass {
 
		 CGPROGRAM
		 #pragma vertex vert
		 #pragma fragment frag
 
		 #include "UnityCG.cginc"
 
		 sampler2D _MainTex;
		 sampler2D _NoiseTex;
 
		 struct v2f {
			 float4  pos : SV_POSITION;
			 float2  uv : TEXCOORD0;
		 };
 
		 float4 _MainTex_ST;
 
		 v2f vert (appdata_base v)
		 {
			 v2f o;
			 o.pos = UnityObjectToClipPos (v.vertex);
			 o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
			 return o;
		 }
 
		 half4 frag (v2f i) : COLOR
		 {
			half2 uv = i.uv;

			// Noise
            half4 noisecol = tex2D(_NoiseTex, uv + 100* _Time.x*_Time.x * _Time.y*_Time.y);
			half4 texcol = tex2D(_MainTex, uv);
			half4 texWithNoisecol = lerp(noisecol, texcol, 0.98);

			//Gray Scale
			texWithNoisecol.rgb = dot(texWithNoisecol.rgb, float3(0.3, 0.59, 0.11));
			return texWithNoisecol;
		 }
		 ENDCG
		}
	}
 Fallback "VertexLit"
 } 