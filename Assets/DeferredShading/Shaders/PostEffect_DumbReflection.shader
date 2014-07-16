Shader "Custom/PostEffect_DumbReflection" {
	Properties {
		_Intensity ("Intensity", Float) = 0.3
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		Blend One One
		ZTest Always
		ZWrite Off
		Cull Back

		CGINCLUDE

		sampler2D _FrameBuffer;
		sampler2D _PositionBuffer;
		sampler2D _NormalBuffer;
		float _Intensity;

		float  modc(float  a, float  b) { return a - b * floor(a/b); }
		float2 modc(float2 a, float2 b) { return a - b * floor(a/b); }
		float3 modc(float3 a, float3 b) { return a - b * floor(a/b); }
		float4 modc(float4 a, float4 b) { return a - b * floor(a/b); }



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
			o.vertex = v.vertex;
			o.screen_pos = v.vertex;
			return o;
		}

		ps_out frag (ps_in i)
		{
			float2 coord = (i.screen_pos.xy / i.screen_pos.w + 1.0) * 0.5;
			// see: http://docs.unity3d.com/Manual/SL-PlatformDifferences.html
			#if UNITY_UV_STARTS_AT_TOP
			//	coord.y = 1.0-coord.y;
			#endif

			float4 p = tex2D(_PositionBuffer, coord);

			float4 n = tex2D(_NormalBuffer, coord);
			float3 camDir = normalize(p.xyz - _WorldSpaceCameraPos);


			ps_out r;
			r.color.xyz = tex2D(_FrameBuffer, coord).xyz;
			
			const int Marching1 = 1;
			const int Marching2 = 8;
			const float MarchDistance = 1.0;
			const float RcpMarchDistance = 1.0/MarchDistance;
			const float MarchSpan1 = MarchDistance / Marching1;
			const float MarchSpan2 = MarchSpan1 / Marching2;
			float3 refdir = reflect(camDir, n.xyz);


			if(p.w!=0.0) {
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
				for(int j=0; j<Marching2; ++j) {
					float adv = MarchSpan1;
					float4 tpos = mul(UNITY_MATRIX_MVP, float4(p+(refdir+noises[j]*0.04)*adv, 1.0) );
					float2 tcoord = (tpos.xy / tpos.w + 1.0) * 0.5;

					#if UNITY_UV_STARTS_AT_TOP
					//	tcoord.y = 1.0-coord.y;
					#endif
					float4 reffragpos = tex2D(_PositionBuffer, tcoord);

					r.color.xyz += tex2D(_FrameBuffer, tcoord).xyz * (_Intensity/Marching2);
				}
			}


			r.color.w = 1.0;
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
}
