Shader "DeferredShading/RadialBlur" {

Properties {
    _BaseColor ("BaseColor", Vector) = (0.3, 0.3, 0.3, 10.0)
    _GlowColor ("GlowColor", Vector) = (0.0, 0.0, 0.0, 0.0)
}
SubShader {
    Tags { "RenderType"="Opaque" "Queue"="Geometry" }

CGINCLUDE
#include "Compat.cginc"

sampler2D frame_buffer;
float4 radialblur_params; 
float3 base_position;

struct ia_out
{
    float4 vertex : POSITION;
};

struct vs_out
{
    float4 vertex : SV_POSITION;
    float4 spos : TEXCOORD0;
};

struct ps_out
{
    float4 color : COLOR0;
};

vs_out vert(ia_out v)
{
    vs_out o;
    o.vertex = o.spos = mul(UNITY_MATRIX_MVP, float4(v.vertex.xyz, 1.0));
    return o;
}

ps_out frag(vs_out i)
{
    ps_out o;
    o.color = tex2D(frame_buffer, screen_to_texcoord(i.spos));
    o.color.a = radialblur_params.x;
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
