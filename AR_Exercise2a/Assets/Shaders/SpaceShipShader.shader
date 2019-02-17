Shader "AR/ShaderSpaceShip"
{
	//Exposed properties (similar to public members in MonoBehaviour scripts)
	Properties{
		_Color("Color", Color) = (1,1,0, 1)
	}
		Subshader{

		//Geometry(default) - this is used for most objects.Opaque geometry uses this queue.
		Tags{ "RenderType" = "Opaque" }

		Pass{

		CGPROGRAM
		//SHADER STARTS HERE

		fixed4 _Color;

#pragma vertex vert
#pragma fragment frag

	struct Input
	{
		float4 position : POSITION;
		float4 vertColor : COLOR;
	};

	//Define output of vertex, which is input for each fragment
	struct VertexToFragment
	{
		float4 position: SV_POSITION;
		float4 vertColor : COLOR;
	};



	VertexToFragment vert(Input IN)
	{
		VertexToFragment OUT;
		OUT.position = UnityObjectToClipPos(IN.position);
		OUT.vertColor = IN.vertColor;
		return OUT;
	}

	fixed4 frag(VertexToFragment IN) : SV_Target{
		return IN.vertColor;
	}

		//An alternative approach to the one above would be to use the built-in ComputeScreenPos function

		ENDCG
	}

	}

}
