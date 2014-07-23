Shader "Custom/GBParticle" {

Properties {
	_BaseColor ("BaseColor", Vector) = (0.15, 0.15, 0.2, 5.0)
	_GlowColor ("GlowColor", Vector) = (0.0, 0.0, 0.0, 0.0)
	_HeatColor ("HeatColor", Vector) = (0.25, 0.05, 0.025, 0.0)
	_HeatThreshold ("HeatThreshold", Float) = 2.5
	_HeatIntensity ("HeatIntensity", Float) = 1.0
}
SubShader {
	Tags { "RenderType"="Opaque" }

	Pass {
	Cull Back
	ZWrite On
	ZTest Less

	CGPROGRAM
	#pragma target 5.0
	#pragma vertex vert
	#pragma fragment frag 
	#include "UnityCG.cginc"

	float4 _BaseColor;
	float4 _GlowColor;
	float4 _HeatColor;
	float _HeatThreshold;
	float _HeatIntensity;



	struct Particle
	{
		float3 position;
		float3 velocity;
		float speed;
		float lifetime;
		int owner_objid;
		int hit_objid;
	};
	struct Vertex
	{
		float3 position;
		float3 normal;
	};
	StructuredBuffer<Particle> particles;
	StructuredBuffer<Vertex> vertices;

	struct ia_out {
		uint vertexID : SV_VertexID;
		uint instanceID : SV_InstanceID;
	};

	struct vs_out {
		float4 vertex : SV_POSITION;
		float4 screen_pos : TEXCOORD0;
		float4 position : TEXCOORD1;
		float4 normal : TEXCOORD2;
		float4 emission : TEXCOORD3;
	};

	struct ps_out
	{
		float4 normal : COLOR0;
		float4 position : COLOR1;
		float4 color : COLOR2;
		float4 glow : COLOR3;
	};

	vs_out vert(ia_out io)
	{

		float3 ipos = particles[io.instanceID].position;
		float4 v = float4(vertices[io.vertexID].position+ipos, 1.0);
		float4 n = float4(vertices[io.vertexID].normal, 0.0);
		float4 vp = mul(UNITY_MATRIX_VP, v);
		//vp.y *= -1.0;

		vs_out o;
		o.vertex = vp;
		o.screen_pos = vp;
		o.position = v;
		o.normal = normalize(n);


		float speed = particles[io.instanceID].speed;
		float heat = max(speed-_HeatThreshold, 0.0) * _HeatIntensity;
		o.emission = _GlowColor + _HeatColor * heat;
		o.emission.w = particles[io.instanceID].lifetime<=0.0f ? 0.0 : 1.0;
		return o;
	}

	ps_out frag(vs_out vo)
	{
		if(vo.emission.w==0.0) {
			discard;
		}
		ps_out o;
		o.normal = vo.normal;
		o.position = float4(vo.position.xyz, vo.screen_pos.z);
		o.color = _BaseColor;
		o.glow = vo.emission;
		return o;
	}

	ENDCG
	}
}
Fallback Off

}
