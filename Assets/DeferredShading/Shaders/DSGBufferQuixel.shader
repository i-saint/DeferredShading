Shader "DeferredShading/GBufferQuixel" {

Properties {
	_Albedo ("Albedo", 2D) = "white" {}
	_Gloss ("Gloss", 2D) = "white" {}
	_Normal ("Normal", 2D) = "white" {}
	_Specular ("Specular", 2D) = "white" {}
	_BaseColor ("BaseColor", Vector) = (0.15, 0.15, 0.2, 1.0)
	_GlowColor ("GlowColor", Vector) = (0.0, 0.0, 0.0, 0.0)
}
SubShader {
	Tags { "RenderType"="Opaque" "Queue"="Geometry" }

	CGINCLUDE
	#include "Compat.cginc"
	#include "DS.cginc"
	#include "DSGBuffer.cginc"
	ENDCG

	Pass {
		Name "Shading"
		Cull Back
		ZWrite On
		ZTest LEqual

		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag2
		#pragma target 3.0
		#ifdef SHADER_API_OPENGL 
			#pragma glsl
		#endif


ps_out frag2(vs_out i)
{
	float2 coord = i.texcoord;

	float3 albedo = tex2D(_Albedo, coord).rgb;

	float3x3 tbn = float3x3( i.tangent.xyz, i.binormal, i.normal.xyz);
	float3 normal = tex2D(_Normal, coord).rgb*2.0-1.0;
	//normal = normalize(mul(_Object2World, float4(normal,0.0)).xyz);
	normal = normalize(mul(normal, tbn));
	//float3 normal = i.normal.xyz;

	float gloss = tex2D(_Gloss, coord).r;
	float spec = tex2D(_Specular, coord).r;

	ps_out o;
	o.normal = float4(normal, gloss);
	o.position = float4(i.position.xyz, i.screen_pos.z);
	o.color = float4(albedo*_BaseColor.rgb, spec);
	o.glow = _GlowColor;
	return o;
}

		ENDCG
	}
}
Fallback Off
}
