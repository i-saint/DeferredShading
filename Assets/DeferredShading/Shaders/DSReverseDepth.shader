Shader "DeferredShading/ReverseDepth" {

Properties {
}
SubShader {
	CGINCLUDE

	struct vs_in
	{
		float4 vertex : POSITION;
	};

	struct ps_in {
		float4 vertex : SV_POSITION;
		float4 screen_pos : TEXCOORD0;
	};

	struct ps_out
	{
		float4 color : COLOR0;
	};


	ps_in vert (vs_in v)
	{
		ps_in o;
		float4 t = mul(UNITY_MATRIX_MVP, v.vertex);
		o.vertex = t;
		o.screen_pos = t;
		return o;
	}

	ps_out frag (ps_in i)
	{
		ps_out o;
		o.color = i.screen_pos.z;
		return o;
	}
	ENDCG

	Pass {
		Tags { "RenderType"="Opaque" "Queue"="Geometry" }
		ZTest Greater
		ZWrite On
		Cull Front

		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma target 3.0
		#pragma glsl
		ENDCG
	}
} 

}
