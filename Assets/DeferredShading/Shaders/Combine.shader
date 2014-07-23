Shader "Custom/Combine" {

Properties {
}
SubShader {
	Tags { "RenderType"="Opaque" }
	ZTest Always
	ZWrite Off
	Cull Back

	CGINCLUDE
	sampler2D _MainTex;

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


	vs_out vert (ia_out io)
	{
		vs_out o;
		o.vertex = io.vertex;
		o.screen_pos = io.vertex;
		return o;
	}

	ps_out frag(vs_out vo)
	{
		float2 coord = (vo.screen_pos.xy / vo.screen_pos.w + 1.0) * 0.5;
		#if UNITY_UV_STARTS_AT_TOP
			coord.y = 1.0-coord.y;
		#endif
		ps_out po = { tex2D(_MainTex, coord) };
		return po;
	}

	ps_out frag2(vs_out vo)
	{
		float2 coord = (vo.screen_pos.xy / vo.screen_pos.w + 1.0) * 0.5;
		ps_out po = { tex2D(_MainTex, coord) };
		return po;
	}
	ENDCG

	Pass {
		Blend One One

		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma target 3.0
		#ifdef SHADER_API_OPENGL 
			#pragma glsl
		#endif
		ENDCG
	}
	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag2
		#pragma target 3.0
		#ifdef SHADER_API_OPENGL 
			#pragma glsl
		#endif
		ENDCG
	}
}

}
