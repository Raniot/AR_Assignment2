Shader "Custom/ContourShader2"
{
	Properties{
		_MainTex("Texture", 2D) = "white" { }
		_NoiseTex("Noise texture", 2D) = "grey" {}
		_ColorBlack("Color", Color) = (0,0,0, 1)
		_ColorWhite("Color", Color) = (1,1,1, 1)
		_randValue("My float", Float) = 0.5
	}
		SubShader{
			Pass {

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				sampler2D _MainTex;
				sampler2D _NoiseTex;
				vector _randValue;
				float3 forward; 
				fixed4 _ColorBlack;
				fixed4 _ColorWhite;


				struct v2f {
					float4  pos : SV_POSITION;
					float2  uv : TEXCOORD0;
					half3 normal : NORMAL;
				};

				float4 _MainTex_ST;

				v2f vert(appdata_base v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
					o.normal = UnityObjectToWorldNormal(v.normal);
					return o;
				}

				half4 frag(v2f i) : COLOR
				{
					half2 uv = i.uv;
					// Noise
					half4 noisecol = tex2D(_NoiseTex, (uv + 10 * _randValue.x));

					forward = mul((float3x3)unity_CameraToWorld, float3(0, 0, 1));
					float dotProduct = dot(i.normal, forward);
					if (dotProduct < 0.5 && dotProduct > -0.5)
					{
						return noisecol + _ColorBlack;
					}
					else
					{
						return noisecol + _ColorWhite;
					}
					
				}
				ENDCG
				}
		}
	Fallback "VertexLit"
}