Shader "DeferredShading/GBufferAnd" {

Properties {
}
SubShader {
	Tags { "RenderType"="Opaque" }
	ZTest Always
	ZWrite On
	Cull Back

	CGINCLUDE

	sampler2D _DepthBuffer1;
	sampler2D _NormalBuffer1;
	sampler2D _PositionBuffer1;
	sampler2D _ColorBuffer1;
	sampler2D _GlowBuffer1;

	sampler2D _DepthBuffer2;
	sampler2D _NormalBuffer2;
	sampler2D _PositionBuffer2;
	sampler2D _ColorBuffer2;
	sampler2D _GlowBuffer2;


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
		float4 normal : COLOR0;
		float4 position : COLOR1;
		float4 color : COLOR2;
		float4 glow : COLOR3;
		float depth : DEPTH;
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
		float2 coord = (i.screen_pos.xy / i.screen_pos.w + 1.0) * 0.5;
		// see: http://docs.unity3d.com/Manual/SL-PlatformDifferences.html
		#if UNITY_UV_STARTS_AT_TOP
			coord.y = 1.0-coord.y;
		#endif

		float depth1 = tex2D(_DepthBuffer1, coord).x;
		float depth2 = tex2D(_DepthBuffer2, coord).x;
		float4 pos1 = tex2D(_PositionBuffer1, coord);
		float4 pos2 = tex2D(_PositionBuffer2, coord);

		ps_out r;
		if(pos1.w==0.0 || pos2.w==0.0 || depth1<pos2.w || depth2<pos1.w) {
			r.normal	= 0.0;
			r.position	= 0.0;
			r.color		= 0.0;
			r.glow		= 0.0;
			r.depth		= 1.0;
		}
		else if(pos1.w<pos2.w) {
			r.position	= pos2;
			r.normal	= tex2D(_NormalBuffer2, coord);
			r.color		= tex2D(_ColorBuffer2, coord);
			r.glow		= tex2D(_GlowBuffer2, coord);
			r.depth = 0.1;
		}
		else {
			r.position	= pos1;
			r.normal	= tex2D(_NormalBuffer1, coord);
			r.color		= tex2D(_ColorBuffer1, coord);
			r.glow		= tex2D(_GlowBuffer1, coord);
			r.depth = 0.1;
		}
		return r;
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
FallBack Off
}
