Shader "BlueImpulse/GBufferBatched" {

Properties {
    _BaseColor ("BaseColor", Vector) = (0.15, 0.15, 0.2, 5.0)
    _GlowColor ("GlowColor", Vector) = (0.0, 0.0, 0.0, 0.0)
    line_color ("LineColor", Vector) = (0.45, 0.4, 2.0, 0.0)
    _Gloss ("Gloss", Float) = 1.0
}
SubShader {
    Tags { "RenderType"="Opaque" "Queue"="Geometry" }

CGINCLUDE
#define WITHOUT_COMMON_VERT_FRAG
#include "../../DeferredShading/Shaders/Compat.cginc"
#include "../../DeferredShading/Shaders/DS.cginc"
#include "../../DeferredShading/Shaders/DSGBuffer.cginc"
#include "../../BatchRenderer/Shaders/math.cginc"
#include "Glowline.cginc"

#if (defined(SHADER_API_D3D11) || defined(SHADER_API_PSSL)) && !defined(ALWAYS_USE_TEXTURE_DATA_SOURCE)
    #define WITH_STRUCTURED_BUFFER
#endif


int     g_num_instances;
float4  g_scale;
int     g_batch_begin;

int     GetInstanceID(float2 i) { return i.x + g_batch_begin; }
int     GetNumInstances()       { return g_num_instances; }
float3  GetBaseScale()          { return g_scale.xyz; }
int     GetBatchBegin()         { return g_batch_begin; }


#ifdef WITH_STRUCTURED_BUFFER

struct InstanceData
{
    float3 translation;
    float4 rotation;
    float3 scale;
    float time;
    float id;
};
StructuredBuffer<InstanceData> g_instance_buffer;

float3 GetInstanceTranslation(int i){ return g_instance_buffer[i].translation; }
float4 GetInstanceRotation(int i)   { return g_instance_buffer[i].rotation; }
float3 GetInstanceScale(int i)      { return g_instance_buffer[i].scale; }
float  GetInstanceTime(int i)       { return g_instance_buffer[i].time; }
float  GetInstanceID(int i)         { return g_instance_buffer[i].id; }

#else // WITH_STRUCTURED_BUFFER

float3 GetInstanceTranslation(int i){ return 0.0; }
float4 GetInstanceRotation(int i)   { return 0.0; }
float3 GetInstanceScale(int i)      { return 0.0; }
float  GetInstanceTime(int i)       { return 0.0; }
float  GetInstanceID(int i)         { return 0.0; }

#endif // WITH_STRUCTURED_BUFFER

void ApplyInstanceTransform(float2 id, inout float4 vertex, inout float3 normal)
{
    int instance_id = GetBatchBegin() + id.x;
    if(instance_id >= GetNumInstances()) {
        vertex.xyz *= 0.0;
        return;
    }

    vertex.xyz *= GetBaseScale();
    vertex.xyz *= GetInstanceScale(instance_id);
    {
        float3x3 rot = quaternion_to_matrix33(GetInstanceRotation(instance_id));
        vertex.xyz = mul(rot, vertex.xyz);
        normal.xyz = mul(rot, normal.xyz);
        //tangent.xyz = mul(rot, tangent.xyz);
    }
    vertex.xyz += GetInstanceTranslation(instance_id);
}


struct my_vs_out
{
    float4 vertex : SV_POSITION;
    float4 screen_pos : TEXCOORD0;
    float4 position : TEXCOORD1;
    float3 normal : TEXCOORD2;
    float3 local_position : TEXCOORD3;
    float3 local_normal : TEXCOORD4;
};

float4 line_color;

my_vs_out vert(ia_out v)
{
    my_vs_out o;
    float4 vmvp = mul(UNITY_MATRIX_VP, v.vertex);
    o.vertex = vmvp;
    o.screen_pos = vmvp;
    o.position = mul(_Object2World, v.vertex);
    o.normal = normalize(mul(_Object2World, float4(v.normal.xyz,0.0)).xyz);
    o.local_position = v.vertex.xyz;
    o.local_normal = v.normal.xyz;
    return o;
}

ps_out frag(my_vs_out i)
{
    float4 glow = _GlowColor;

    float r = hash(GetInstanceID());
    float2 dg = boxcell((i.local_position.xyz+r)*0.1, i.local_normal.xyz);

    float pc = 1.0-clamp(1.0 - max(min(dg.x, 2.0)-1.0, 0.0)*2.0, 0.0, 1.0);
    float d = -length(i.position.xyz)*0.15 - dg.y*0.5;
    float vg = max(0.0, frac(1.0-d*0.75-_Time.y*0.25)*3.0-2.0) * pc;
    glow += line_color * vg * 1.5;

    float extrude = dg.y*4.0 - 4.0 + dg.x*0.5;
    float3 sphere_pos = GetInstancePosition();
    float sphere_radius = GetInstanceTime() * (3.0 + dg.y*0.2) + extrude;
    float3 s_normal = normalize(_WorldSpaceCameraPos.xyz - i.position.xyz);
    float3 pos_rel = i.position.xyz - sphere_pos;
    float s_dist = dot(pos_rel, s_normal);
    float3 pos_proj = i.position.xyz - s_dist*s_normal;
    float dist_proj = length(pos_proj-sphere_pos);
    if(dist_proj>sphere_radius) {
        discard;
    }

    float l = (1.0-pc)*0.1+0.9;

    ps_out o;
    float len = length(pos_rel);
    if(len<sphere_radius) {
        o.normal = float4(i.normal.xyz, _Gloss*l);
        o.position = float4(i.position.xyz, i.screen_pos.z);
    }
    else {
        float s_dist2 = length(pos_proj-sphere_pos);
        float s_dist3 = sqrt(sphere_radius*sphere_radius - s_dist2*s_dist2);
        float3 ps = pos_proj + s_normal * s_dist3;

        float3 dir = normalize(ps-sphere_pos);
        float3 pos = sphere_pos+dir*sphere_radius;
        float4 spos = mul(UNITY_MATRIX_VP, float4(pos,1.0));
        o.normal = float4(dir, _Gloss*l);
        o.position = float4(pos, spos.z);
    }
    glow.rgb += float3(0.2, 0.2, 0.7) * max(1.0 - abs(dist_proj-sphere_radius), 0.0)*4.0;


    o.color = _BaseColor * l;
    o.glow = glow;
    return o;
}
    ENDCG
    
    Pass {
        Name "DepthPrePass"
        Tags { "RenderType"="Opaque" "Queue"="Geometry-1" }
        Cull Back
        ZWrite On
        ZTest Less

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
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
        ZWrite Off
        ZTest Equal

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
