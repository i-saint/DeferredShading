Shader "GPUParticle/Kernel" {
Properties {
}
SubShader {
	Tags { "RenderType"="Opaque" }
	ZTest Always
	ZWrite Off
	Cull Back

	CGINCLUDE

	sampler2D gbuffer_position;
	sampler2D gbuffer_normal;
	sampler2D sphere_colliders;
	sampler2D capsule_colliders;
	sampler2D box_colliders;
	sampler2D particle_position;
	sampler2D particle_velocity;
	sampler2D particle_params;

	float4 max_quantities; // [0]:max_particles, [1]:max_sphere_colliders, [2]:max_capsule_colliders, [3]:max_box_colliders
	float4 quantities; // [0]:num_particles, [1]:num_sphere_colliders, [2]:num_capsule_colliders, [3]:num_box_colliders


	struct ia_out
	{
		float4 vertex : POSITION;
	};

	struct vs_out
	{
		float4 vertex : SV_POSITION;
		float4 screen_pos : TEXCOORD0;
	};

	struct ps_out_v
	{
		float4 velocity : COLOR0;
	};

	struct ps_out_pvp
	{
		float4 position : COLOR0;
		float4 velocity : COLOR1;
		float4 params : COLOR2;
	};


	vs_out vert(ia_out v)
	{
		vs_out o;
		o.vertex = v.vertex;
		o.screen_pos = v.vertex;
		return o;
	}

	ps_out_v process_gbuffer_collider(vs_out vo)
	{
		float2 coord = (vo.screen_pos.xy / vo.screen_pos.w + 1.0) * 0.5;
		// see: http://docs.unity3d.com/Manual/SL-PlatformDifferences.html
		#if UNITY_UV_STARTS_AT_TOP
			coord.y = 1.0-coord.y;
		#endif

		float4 params = tex2D(particle_params, coord);
		if(params.x==0.0) { discard; }
		float4 position = tex2D(particle_position, coord);
		float4 velocity = tex2D(particle_velocity, coord);

		ps_out_v r;
		r.velocity = velocity;
		return r;
	}

	ps_out_v process_colliders(vs_out vo)
	{
		float2 coord = (vo.screen_pos.xy / vo.screen_pos.w + 1.0) * 0.5;
		#if UNITY_UV_STARTS_AT_TOP
			coord.y = 1.0-coord.y;
		#endif

		float4 params = tex2D(particle_params, coord);
		if(params.x==0.0) { discard; }
		float4 position = tex2D(particle_position, coord);
		float4 velocity = tex2D(particle_velocity, coord);

		int i;
		int num_sphere_colliders = quantities[1];
		int num_capsule_colliders = quantities[2];
		int num_box_colliders = quantities[3];
		for(i=0; i<num_sphere_colliders; ++i) {
		}
		for(i=0; i<num_capsule_colliders; ++i) {
		}
		for(i=0; i<num_box_colliders; ++i) {
		}

		ps_out_v r;
		r.velocity = velocity;
		return r;
	}
	
	ps_out_pvp integrate(vs_out vo)
	{
		float2 coord = (vo.screen_pos.xy / vo.screen_pos.w + 1.0) * 0.5;
		#if UNITY_UV_STARTS_AT_TOP
			coord.y = 1.0-coord.y;
		#endif

		float4 params = tex2D(particle_params, coord);
		if(params.x==0.0) { discard; }
		float4 position = tex2D(particle_position, coord);
		float4 velocity = tex2D(particle_velocity, coord);

		ps_out_pvp r;
		r.position = position;
		r.velocity = velocity;
		r.params = params;
		return r;
	}

	ENDCG

	Pass {
		ZWrite Off
		ZTest Always
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment process_gbuffer_collider
		#pragma target 3.0
		#ifdef SHADER_API_OPENGL 
			#pragma glsl
		#endif
		ENDCG
	}
	Pass {
		ZWrite Off
		ZTest Always
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment process_colliders
		#pragma target 3.0
		#ifdef SHADER_API_OPENGL 
			#pragma glsl
		#endif
		ENDCG
	}
	Pass {
		ZWrite Off
		ZTest Always
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment integrate
		#pragma target 3.0
		#ifdef SHADER_API_OPENGL 
			#pragma glsl
		#endif
		ENDCG
	}
}
}
