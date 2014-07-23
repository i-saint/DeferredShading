Shader "Custom/PostEffect_Reflection" {
Properties {
	_Intensity ("Intensity", Float) = 1.0
	_RayAdvance ("RayAdvance", Float) = 1.0
}
SubShader {
	Tags { "RenderType"="Opaque" }
	ZTest Always
	ZWrite Off
	Cull Back

	CGINCLUDE

	sampler2D _FrameBuffer;
	sampler2D _PositionBuffer;
	sampler2D _NormalBuffer;
	float _Intensity;
	float _RayAdvance;

	float  modc(float  a, float  b) { return a - b * floor(a/b); }
	float2 modc(float2 a, float2 b) { return a - b * floor(a/b); }
	float3 modc(float3 a, float3 b) { return a - b * floor(a/b); }
	float4 modc(float4 a, float4 b) { return a - b * floor(a/b); }


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

	ps_out frag_dumb(vs_out i)
	{
		float2 coord = (i.screen_pos.xy / i.screen_pos.w + 1.0) * 0.5;
		// see: http://docs.unity3d.com/Manual/SL-PlatformDifferences.html
		#if UNITY_UV_STARTS_AT_TOP
			coord.y = 1.0-coord.y;
		#endif

		float4 p = tex2D(_PositionBuffer, coord);
		if(p.w==0.0) { discard; }

		float4 n = tex2D(_NormalBuffer, coord);
		float3 camDir = normalize(p.xyz - _WorldSpaceCameraPos);


		ps_out r;
		r.color = 0.0;

		int NumRays = 8;
		float3 refdir = reflect(camDir, n.xyz);
		float s = _Intensity / NumRays;
		float3 noises[9] = {
			float3(0.0, 0.0, 0.0),
			float3(0.1080925165271518, -0.9546740999616308, -0.5485116160762447),
			float3(-0.4753686437884934, -0.8417212473681748, 0.04781893710693619),
			float3(0.7242715177221273, -0.6574584801064549, -0.7170447827462747),
			float3(-0.023355087558461607, 0.7964400038854089, 0.35384090347421204),
			float3(-0.8308210026544296, -0.7015103725420933, 0.7781031130099072),
			float3(0.3243705688309195, 0.2577797517167695, 0.012345938868925543),
			float3(0.31851240326305463, -0.22207894547397555, 0.42542751740434204),
			float3(-0.36307729185097637, -0.7307245945773899, 0.6834118993358385)
		};
		for(int j=0; j<NumRays; ++j) {
			float4 tpos = mul(UNITY_MATRIX_MVP, float4(p.xyz+(refdir+noises[j]*0.04)*_RayAdvance, 1.0) );
			float2 tcoord = (tpos.xy / tpos.w + 1.0) * 0.5;
			#if UNITY_UV_STARTS_AT_TOP
				tcoord.y = 1.0-tcoord.y;
			#endif
			float4 reffragpos = tex2D(_PositionBuffer, tcoord);
			r.color.xyz += tex2D(_FrameBuffer, tcoord).xyz * s;
		}
		return r;
	}

	ps_out frag_precise(vs_out i)
	{
		float2 coord = (i.screen_pos.xy / i.screen_pos.w + 1.0) * 0.5;
		// see: http://docs.unity3d.com/Manual/SL-PlatformDifferences.html
		#if UNITY_UV_STARTS_AT_TOP
			coord.y = 1.0-coord.y;
		#endif

		float4 p = tex2D(_PositionBuffer, coord);
		if(p.w==0.0) { discard; }

		float4 n = tex2D(_NormalBuffer, coord);
		float3 camDir = normalize(p.xyz - _WorldSpaceCameraPos);


		ps_out r;
		r.color = 0.0;
			
		const int Marching1 = 12;
		const int Marching2 = 4;
		const float RcpMarchDistance = 1.0/_RayAdvance;
		const float MarchSpan1 = _RayAdvance / Marching1;
		const float MarchSpan2 = MarchSpan1 / Marching2;
		float3 refdir = reflect(camDir, n.xyz);
		for(int k=0; k<Marching1; ++k) {
			float adv = MarchSpan1 * (k+1);
			float4 tpos = mul(UNITY_MATRIX_MVP, float4((p+refdir*adv), 1.0) );
			float2 tcoord = (tpos.xy / tpos.w + 1.0) * 0.5;
			#if UNITY_UV_STARTS_AT_TOP
				tcoord.y = 1.0-tcoord.y;
			#endif
			float4 reffragpos = tex2D(_PositionBuffer, tcoord);
			if(reffragpos.w!=0 && reffragpos.w<tpos.z && reffragpos.w>tpos.z-MarchSpan1) {
				float attenuation = 1.0-adv*RcpMarchDistance*0.99;
				r.color.xyz += tex2D(_FrameBuffer, tcoord).xyz * _Intensity * attenuation;
				break;
			}
			if(tcoord.x>1.0 || tcoord.x<0.0 || tcoord.y>1.0 || tcoord.y<0.0) {
				break;
			}
		}
		return r;
	}
	ENDCG

	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag_dumb
		#pragma target 3.0
		#ifdef SHADER_API_OPENGL 
			#pragma glsl
		#endif
		ENDCG
	}
	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag_precise
		#pragma target 3.0
		#ifdef SHADER_API_OPENGL 
			#pragma glsl
		#endif
		ENDCG
	}
}
}
