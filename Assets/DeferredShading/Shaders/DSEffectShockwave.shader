Shader "DeferredShading/Shockwave" {

Properties {
    _BaseColor ("BaseColor", Vector) = (0.3, 0.3, 0.3, 10.0)
    _GlowColor ("GlowColor", Vector) = (0.0, 0.0, 0.0, 0.0)
}
SubShader {
    Tags { "RenderType"="Opaque" "Queue"="Geometry" }

CGINCLUDE
#include "Compat.cginc"

sampler2D frame_buffer;
float4 shockwave_params; 
float3 base_position;

struct ia_out
{
    float4 vertex : POSITION;
    float4 normal : NORMAL;
};

struct vs_out
{
    float4 vertex : SV_POSITION;
    float4 spos : TEXCOORD0;
    float4 position : TEXCOORD1;
    float4 normal : TEXCOORD2;
};

struct ps_out
{
    float4 color : COLOR0;
};

vs_out vert(ia_out v)
{
    vs_out o;

    float4 p = float4(v.vertex.xyz, 1.0);
    o.vertex = mul(UNITY_MATRIX_MVP, p);
    o.position = mul(UNITY_MATRIX_VP, p);
    o.normal = normalize(mul(_Object2World, float4(v.normal.xyz,0.0)));

    float4 wp = mul(_Object2World, p);
    o.spos = mul(UNITY_MATRIX_VP, wp+float4(v.normal.xyz*shockwave_params.x*shockwave_params.y, 0.0));
    return o;
}


ps_out frag(vs_out i)
{
    float2 coord = screen_to_texcoord(i.spos);
#if UNITY_UV_STARTS_AT_TOP
    coord.y = 1.0 - coord.y;
#endif

    ps_out o;
    o.color = tex2D(frame_buffer, coord);
    float3 eyedir = normalize(i.position.xyz - _WorldSpaceCameraPos);
    float d = dot(eyedir, i.normal.xyz);
    o.color.a = (1.0-d);
    o.color.a = 1.0;
    return o;
}
ENDCG

    Pass {
        Cull Back
        ZWrite Off
        ZTest LEqual
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 3.0
        #ifdef SHADER_API_OPENGL 
            #pragma glsl
        #endif
        ENDCG
    }
}
Fallback Off
}
