Shader "Custom/PostEffect_Water" {
Properties {
}
SubShader {
    Tags { "RenderType"="Opaque" }
    Blend Off
    ZTest Less
    ZWrite Off
    Cull Back

CGINCLUDE
#include "Compat.cginc"
#include "DSBuffers.cginc"
#include "noise.cginc"

struct ia_out
{
    float4 vertex : POSITION;
};

struct vs_out
{
    float4 vertex : SV_POSITION;
    float4 screen_pos : TEXCOORD0;
    float4 world_pos : TEXCOORD1;
};

struct ps_out
{
    float4 color : COLOR0;
};


vs_out vert(ia_out v)
{
    float4 spos = mul(UNITY_MATRIX_MVP, v.vertex);
    vs_out o;
    o.vertex = spos;
    o.screen_pos = spos;
    o.world_pos = mul(_Object2World, v.vertex);
    return o;
}


float compute_octave(float3 pos)
{
    float o1 = sea_octave(pos.xzy*1.25 + float3(1.0,2.0,-1.5)*_Time.y*1.5 + sin(pos.xzy+_Time.y*8.3)*0.15, 4.0);
    float o2 = sea_octave(pos.xzy*2.50 + float3(2.0,-1.0,1.0)*_Time.y*-2.5 - sin(pos.xzy+_Time.y*6.3)*0.2, 8.0);
    return o1 * o2;
}

ps_out frag(vs_out i)
{
    float2 coord = (i.screen_pos.xy / i.screen_pos.w + 1.0) * 0.5;
    #if UNITY_UV_STARTS_AT_TOP
        coord.y = 1.0-coord.y;
    #endif

    float4 pos = SamplePosition(coord);
    float d = min(length(pos.xyz - i.world_pos.xyz), 1.0);
    if(pos.w==0.0) { d=0.0; }

    float o = compute_octave(pos.xyz);

    coord.x += o*d * 0.01;
    coord.y -= o*d * 0.01;

    ps_out r;
    r.color = SampleFrame(coord);
    r.color.a = 1.0;
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
