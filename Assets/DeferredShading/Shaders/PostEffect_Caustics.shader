Shader "Custom/PostEffect_Caustics" {
Properties {
}
SubShader {
    Tags { "RenderType"="Opaque" }
    Blend One One
    ZTest Greater
    ZWrite Off
    Cull Back

CGINCLUDE
#include "Compat.cginc"
#include "DSBuffers.cginc"
#include "ClassicNoise3D.cginc"


float3 hash( float3 p )
{
    p = float3( dot(p,float3(127.1,311.7,311.7)), dot(p,float3(269.5,183.3,183.3)), dot(p,float3(269.5,183.3,183.3)) );
    return frac(sin(p)*43758.5453);
}

float voronoi( in float3 x )
{
    float3 n = floor(x);
    float3 f = frac(x);
    float3 mg, mr;

    float md = 8.0;
    {
        for( int j=-1; j<=1; j++ ) {
        for( int i=-1; i<=1; i++ ) {
        for( int k=-1; k<=1; k++ ) {
            float3 g = float3(float(i),float(j),float(k));
            float3 o = hash( n + g );
            float3 r = g + o - f;
            float d = dot(r,r);
            if( d<md ) {
                md = d;
                mr = r;
                mg = g;
            }
        }}}
    }

    md = 8.0;
    {
        for( int j=-1; j<=1; j++ ) {
        for( int i=-1; i<=1; i++ ) {
        for( int k=-1; k<=1; k++ ) {
            float3 g = mg + float3(float(i),float(j),float(k));
            float3 o = hash( n + g );
            float3 r = g + o - f;
            if( dot(mr-r,mr-r)>0.000001 ) {
                float d = dot( 1.5*(mr+r), normalize(r-mr) );
                md = min( md, d );
            }
        }}}
    }

    return md;
}

struct ia_out
{
    float4 vertex : POSITION;
};

struct vs_out
{
    float4 vertex : SV_POSITION;
    float4 screen_pos : TEXCOORD1;
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
    return o;
}

ps_out frag(vs_out i)
{
    float2 coord = (i.screen_pos.xy / i.screen_pos.w + 1.0) * 0.5;
    #if UNITY_UV_STARTS_AT_TOP
        coord.y = 1.0-coord.y;
    #endif

    float4 pos = SamplePosition(coord);
    if(pos.w==0.0) discard;
    float n = pnoise(pos.xyz+_Time.y, 4.0);
    ps_out r;
    r.color = n;
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
