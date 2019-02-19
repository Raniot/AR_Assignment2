// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/ContourShader"
{
	Properties {
		_OutlineColor ("Outline Color", Color) = (0,0,0,1)
		_Outline ("Outline width", Range (0.0, 0.30)) = .3
		_MainTex ("Texture", 2D) = "white" { }
		_NoiseTex ("Noise texture", 2D) = "grey" {}
	}
 
CGINCLUDE
#include "UnityCG.cginc"
 
struct appdata {
	float4 vertex : POSITION;
	float3 normal : NORMAL;
};
 
struct v2f {
	float4 pos : POSITION;
	float4 color : COLOR;
	float2  uv : TEXCOORD0;
};
 
uniform float _Outline;
uniform float4 _OutlineColor;
sampler2D _MainTex;
sampler2D _NoiseTex;
float4 _MainTex_ST;
 
v2f vert(appdata_base v) {
	// just make a copy of incoming vertex data but scaled according to normal direction
	v2f o;
	o.pos = UnityObjectToClipPos(v.vertex);
 
	float3 norm   = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);
	float2 offset = TransformViewToProjection(norm.xy);
	o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
	o.pos.xy += offset * o.pos.z * _Outline;
	o.color = _OutlineColor;
	return o;
}
ENDCG
 
	SubShader {
		//Tags { "Queue" = "Transparent" }
 
		Pass {
			Material {
					Diffuse (0,1,1,1)
			}
			//Cull Back
		}
 
		Pass {
			Name "OUTLINE"
			//Tags { "LightMode" = "Always" }
			Cull Front
 
			Blend One OneMinusDstColor
 
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag

		half4 frag(v2f i) : COLOR {
			
			half2 uv = i.uv;
			half4 noisecol = tex2D(_NoiseTex, uv + 100* _Time.x*_Time.x * _Time.y*_Time.y);
			half4 texcol = tex2D(_MainTex, uv);
			half4 texWithNoisecol = lerp(noisecol, texcol, 0.98);
			return i.color + noisecol;
		}
		ENDCG
		}
 
 
	}
 	Fallback "Diffuse"
}