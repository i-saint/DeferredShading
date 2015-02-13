Shader "DeferredShading/PostEffect/Vignette" {

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
float g_darkness;
float g_monochrome;
float g_scanline;
float g_scanline_scale;
float4 g_color_shearing;

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
    i.screen_pos.xy /= i.screen_pos.w;
    float2 coord = (i.screen_pos.xy + 1.0) * 0.5;
    #if UNITY_UV_STARTS_AT_TOP
        coord.y = 1.0-coord.y;
    #endif

    float4 c = float4(
        tex2D(g_frame_buffer, coord+float2(g_color_shearing.x, 0.0)).r,
        tex2D(g_frame_buffer, coord+float2(g_color_shearing.y, 0.0)).g,
        tex2D(g_frame_buffer, coord+float2(g_color_shearing.z, 0.0)).b,
        1.0 );
    if(g_monochrome>0.0) {
        c.rgb = lerp(c.rgb, dot(c.rgb, float3(0.2125, 0.7154, 0.0721)), clamp(g_monochrome, 0.0, 1.0));
    }
    if(g_scanline > 0.0) {
        c.rgb *= 1.0 - (abs(fmod(coord.y*_ScreenParams.y*g_scanline_scale, 4.0)-1.5)*0.5*g_scanline);
    }
    c *= (1.0-pow(max(length(i.screen_pos.xy)-0.2, 0.0), 2.0)*g_darkness);
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
