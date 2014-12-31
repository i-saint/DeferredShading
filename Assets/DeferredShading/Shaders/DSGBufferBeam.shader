Shader "DeferredShading/GBufferBeam" {

Properties {
    _BaseColor ("BaseColor", Vector) = (0.3, 0.3, 0.3, 10.0)
    _GlowColor ("GlowColor", Vector) = (0.0, 0.0, 0.0, 0.0)
}
SubShader {
    Tags { "RenderType"="Opaque" "Queue"="Geometry" }

CGINCLUDE
#include "Compat.cginc"
#include "DS.cginc"
#include "DSGBuffer.cginc"

float4 beam_direction; // xyz: direction w: length
float3 base_position;
    
vs_out vert_beam(ia_out v)
{
    vs_out o;

    float4 pos1 = mul(_Object2World, v.vertex);
    float4 pos2 = pos1;
    pos2.xyz += beam_direction.xyz * beam_direction.w;

    float3 vel = pos1.xyz - pos2.xyz;
    float3 vel_dir = normalize(vel);
    float3 n = normalize(mul(_Object2World, float4(v.normal.xyz,0.0)).xyz);
    float4 pos = dot(-beam_direction.xyz, n.xyz)>0.0 ? pos1 : pos2;

    float4 vmvp = mul(UNITY_MATRIX_VP, pos);
    o.vertex = vmvp;
    o.screen_pos = vmvp;
    o.position = pos;
    o.normal = n;
    return o;
}



float pattern(float3 p)
{
    float3 grid = 0.25;
    float3 b = grid*0.5;

    p = modc(p, 0.25);
    float3 d = abs(p) - b;
    return min(max(d.x,max(d.y,d.z)),0.0) + length(max(d,0.0)) * 10.0;
}

float3 iq_rand( float3 p )
{
    p = float3( dot(p,float3(127.1,311.7,311.7)), dot(p,float3(269.5,183.3,183.3)), dot(p,float3(269.5,183.3,183.3)) );
    return frac(sin(p)*43758.5453)*2.0-1.0;
}

ps_out frag_beam(vs_out i)
{
    ps_out o;
    o.normal = float4(i.normal.xyz, _Gloss);
    o.position = float4(i.position.xyz, i.screen_pos.z);
    o.color = _BaseColor;

    float3 camDir = normalize(i.position.xyz - _WorldSpaceCameraPos);
    float d = min(max(abs(dot(camDir, i.normal.xyz))*1.0, 0.0), 1.0);
    float pt = abs(iq_rand(i.position.xyz-beam_direction.xyz*beam_direction.w));
    float blend = lerp(pt, 1.0, d);
    o.glow = _GlowColor * blend;
    //o.glow = _GlowColor;
    return o;
}
ENDCG

    Pass {
        Name "DepthPrePass"
        Tags { "RenderType"="Opaque" "Queue"="Geometry-1" }
        ColorMask 0
        ZWrite On
        ZTest Less

        CGPROGRAM
        #pragma vertex vert_beam
        #pragma fragment frag_beam
        #pragma target 3.0
        #ifdef SHADER_API_OPENGL 
            #pragma glsl
        #endif
        ENDCG
    }

    Pass {
        Name "GBuffer"
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Cull Back
        ZWrite On
        ZTest Equal

        CGPROGRAM
        #pragma vertex vert_beam
        #pragma fragment frag_beam
        #pragma target 3.0
        #ifdef SHADER_API_OPENGL 
            #pragma glsl
        #endif
        ENDCG
    }
}
Fallback Off
}
