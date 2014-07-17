Shader "DeferredShading/Fill" {

Properties {
}
SubShader {
	Tags { "RenderType"="Transparent" }
	ZTest Always
	ZWrite Off
	Cull Back
	Blend SrcAlpha OneMinusSrcAlpha

	CGINCLUDE

	struct ia_out
	{
		float4 vertex : POSITION;
	};

	struct vs_out
	{
		float4 vertex : SV_POSITION;
		float4 screen_pos : TEXCOORD0;
	};

	struct ps_out
	{
		float4 color : COLOR0;
	};


	vs_out vert (ia_out v)
	{
		vs_out o;
		o.vertex = v.vertex;
		o.screen_pos = v.vertex;
		return o;
	}

	ps_out frag (vs_out i)
	{
		ps_out o;
		o.color = float4(0,0,0, 0.3);
		return o;
	}
	ENDCG

	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma target 3.0
		#pragma glsl
		ENDCG
	}
}
}
