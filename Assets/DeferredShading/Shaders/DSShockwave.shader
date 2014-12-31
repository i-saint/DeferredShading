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
float4 stretch_params;
float3 base_position;

struct ia_out
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
};

struct vs_out
{
    float4 vertex : SV_POSITION;
    float4 refpos : TEXCOORD0;
    float4 spos : TEXCOORD1;
    float4 position : TEXCOORD2;
    float3 normal : TEXCOORD3;
};

struct ps_out
{
    float4 color : COLOR0;
};

vs_out vert(ia_out v)
{
    float3 n = normalize(mul(_Object2World, float4(v.normal.xyz,0.0)).xyz);
    float4 wpos = mul(_Object2World, float4(v.vertex.xyz, 1.0));

    float4 expand = 0.0;
    if(stretch_params.w!=0.0) {
        float d = max(dot(stretch_params.xyz, n.xyz), 0.0);
        expand.xyz = stretch_params.xyz * stretch_params.w * d;
        wpos+=expand;
    }


    vs_out o;
    o.vertex = o.spos = mul(UNITY_MATRIX_VP, wpos);
    o.position = wpos;
    o.normal = n;
    o.refpos = mul(UNITY_MATRIX_VP, wpos+float4(v.normal.xyz*shockwave_params.x*shockwave_params.y, 0.0));
    return o;
}


ps_out frag(vs_out i)
{
    float2 coord1 = screen_to_texcoord(i.refpos);
    float2 coord2 = screen_to_texcoord(i.spos);
#if UNITY_UV_STARTS_AT_TOP
    coord1.y = 1.0 - coord1.y;
    coord2.y = 1.0 - coord2.y;
#endif

    ps_out o;
    float3 eyedir = normalize(_WorldSpaceCameraPos-i.position.xyz);
    float d = dot(eyedir, i.normal.xyz);
    d = 1.0 - d*d;
    if(stretch_params.w!=0.0) {
        float e = 1.0 - max(dot(stretch_params.xyz, i.normal.xyz)-0.2, 0.0)*1.222;
        e = e*e;
        d = lerp(d, 1.0, e);
    }
    o.color = tex2D(frame_buffer, lerp(coord1, coord2, d));
    o.color.a = 1.0;
    //o.color.rgb = d;
    return o;
}
ENDCG

    Pass {
        Cull Back
        ZWrite Off
        ZTest LEqual

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
