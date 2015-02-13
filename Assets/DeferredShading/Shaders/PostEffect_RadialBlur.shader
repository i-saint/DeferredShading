Shader "DeferredShading/PostEffect/RadialBlur" {

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
float g_intensity;
float g_radius;
float4 g_center;
float4 g_threshold;


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

    float4 center = mul(UNITY_MATRIX_VP, float4(g_center.xyz, 1.0));
    center.xy = (center.xy/center.w)*0.5+0.5;
    #if UNITY_UV_STARTS_AT_TOP
        center.y = 1.0-center.y;
    #endif

    const int iter = 32;
    float2 dir = normalize(coord-center);
    float step = length(coord-center)*g_radius / iter;

    float4 ref_color = tex2D(g_frame_buffer, coord);
    float4 color = 0.0;
    float blend_rate = 0.0;
    for(int k=0; k<iter; ++k) {
        float r = 1.0 - (1.0/iter*k);
        blend_rate += r;
        color.rgb += max(tex2D(g_frame_buffer, coord - dir*(step*k)).rgb - g_threshold.rgb, 0.0) * r * g_intensity;
    }
    color.rgb = ref_color + color.rgb/blend_rate;

    ps_out r = {color};
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
