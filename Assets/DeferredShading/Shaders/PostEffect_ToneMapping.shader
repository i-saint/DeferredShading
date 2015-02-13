Shader "DeferredShading/PostEffect/ToneMapping" {

Properties {
}
SubShader {
    Tags { "RenderType"="Opaque" }
    Blend Off
    ZTest Always
    ZWrite Off
    Cull Back

CGINCLUDE
#include "Compat.cginc"

sampler2D g_frame_buffer;
float4 g_range_min;
float4 g_range_max;
float4 g_pow;

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


vs_out vert(ia_out v)
{
    vs_out o;
    o.vertex = v.vertex;
    o.screen_pos = v.vertex;
    return o;
}

ps_out frag(vs_out i)
{
    float2 coord = (i.screen_pos.xy / i.screen_pos.w + 1.0) * 0.5;
    #if UNITY_UV_STARTS_AT_TOP
        coord.y = 1.0-coord.y;
    #endif

    float4 c = tex2D(g_frame_buffer, coord);
    c.rgb = (c.rgb - g_range_min.rgb) * (1.0/(g_range_max.rgb-g_range_min.rgb));
    c.rgb = pow(c.rgb, g_pow.rgb);
    ps_out r = {c};
    return r;
}
ENDCG

    Pass {
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
}
