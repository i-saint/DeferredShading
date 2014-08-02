Shader "DeferredShading/GBufferCopy" {

Properties {
}
SubShader {
	Tags { "RenderType"="Opaque" }
	ZTest Always
	ZWrite On
	Cull Back

	CGINCLUDE

	sampler2D _NormalBuffer;
	sampler2D _PositionBuffer;
	sampler2D _AlbedoBuffer;
	sampler2D _GlowBuffer;

	struct ia_out
	{
		float4 vertex : POSITION;
	};

	struct vs_out
	{
		float4 vertex : SV_POSITION;
		float4 screen_pos : TEXCOORD0;
	};

	struct ps_out_np
	{
		float4 normal : COLOR0;
		float4 position : COLOR1;
	};

	struct ps_out_npag
	{
		float4 normal : COLOR0;
		float4 position : COLOR1;
		float4 albedo : COLOR2;
		float4 glow : COLOR3;
	};


	vs_out vert(ia_out v)
	{
		vs_out o;
		o.vertex = v.vertex;
		o.screen_pos = v.vertex;
		return o;
	}

	ps_out_np frag_np(vs_out i)
	{
		float2 coord = (i.screen_pos.xy / i.screen_pos.w + 1.0) * 0.5;
		// see: http://docs.unity3d.com/Manual/SL-PlatformDifferences.html
		#if UNITY_UV_STARTS_AT_TOP
			coord.y = 1.0-coord.y;
		#endif

		ps_out_np r;
		r.position	= tex2D(_PositionBuffer, coord);
		r.normal	= tex2D(_NormalBuffer, coord);
		return r;
	}

	ps_out_npag frag_npag(vs_out i)
	{
		float2 coord = (i.screen_pos.xy / i.screen_pos.w + 1.0) * 0.5;
		#if UNITY_UV_STARTS_AT_TOP
			coord.y = 1.0-coord.y;
		#endif

		ps_out_npag r;
		r.position	= tex2D(_PositionBuffer, coord);
		r.normal	= tex2D(_NormalBuffer, coord);
		r.albedo	= tex2D(_AlbedoBuffer, coord);
		r.glow		= tex2D(_GlowBuffer, coord);
		return r;
	}
	ENDCG

	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag_np
		#pragma target 3.0
		#ifdef SHADER_API_OPENGL 
			#pragma glsl
		#endif
		ENDCG
	}
	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag_npag
		#pragma target 3.0
		#ifdef SHADER_API_OPENGL 
			#pragma glsl
		#endif
		ENDCG
	}
}
FallBack Off
}
