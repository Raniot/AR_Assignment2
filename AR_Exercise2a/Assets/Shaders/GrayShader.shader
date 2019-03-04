Shader "Custom/GrayShader"
{
	Properties{
		_MainTex("Texture", 2D) = "white" { }
		_NoiseTex("Noise texture", 2D) = "grey" {}
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
				
				struct VertexIn {
					float4 position : POSITION;
				};

				struct v2f {
					float4  pos : SV_POSITION;
					float4  normDeviceCoords : TEXCOORD0;
				};

				float4 _MainTex_ST;

				v2f vert(VertexIn v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos(v.position);
					o.normDeviceCoords = UnityObjectToClipPos(v.position);
					return o;
				}

				half4 frag(v2f i) : COLOR
				{
					float2 uv = float2(i.normDeviceCoords.x,i.normDeviceCoords.y) / i.normDeviceCoords.w;

					// Noise
					half4 noisecol = tex2D(_NoiseTex, (uv * _randValue.x));
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