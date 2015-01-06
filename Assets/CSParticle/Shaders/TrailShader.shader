Shader "MassParticle/TrailShader" {

Properties {
    _BaseColor ("BaseColor", Vector) = (0.15, 0.15, 0.2, 5.0)
    _FadeTime ("FadeTime", Float) = 0.1
}
SubShader {
    Tags { "RenderType"="Opaque" }

    CGINCLUDE
    #include "Compat.cginc"
    #include "UnityCG.cginc"
    #include "ParticleDataType.cginc"

    float4 _BaseColor;
    float _FadeTime;
    StructuredBuffer<Particle> particles;
    StructuredBuffer<TrailParams> params;
    StructuredBuffer<TrailVertex> vertices;

    struct ia_out {
        uint vertexID : SV_VertexID;
        uint instanceID : SV_InstanceID;
    };

    struct vs_out {
        float4 vertex : SV_POSITION;
        float4 color : TEXCOORD0;
    };

    struct ps_out
    {
        float4 color : COLOR0;
    };

    vs_out vert(ia_out io)
    {
        float lifetime = particles[io.instanceID].lifetime;
        float fade = min(lifetime/_FadeTime, 1.0);

        uint ii = (particles[io.instanceID].id % params[0].max_entities) * params[0].max_history * 2;
        TrailVertex tv = vertices[ii + io.vertexID];

        float4 v = float4(tv.position, 1.0);
        float4 vp = mul(UNITY_MATRIX_VP, v);

        float density = particles[io.instanceID].density;
        vs_out o;
        o.vertex = vp;
        o.color = _BaseColor * fade;
        o.color.w = particles[io.vertexID].lifetime==0.0 ? 0.0 : 1.0;

        return o;
    }

    ps_out frag(vs_out vo)
    {
        if(vo.color.w==0.0) { discard; }
        ps_out o;
        o.color = vo.color;
        return o;
    }

    ENDCG

    Pass {
        Cull Off
        ZWrite Off
        ZTest LEqual
        Blend One One

        CGPROGRAM
        #pragma target 5.0
        #pragma vertex vert
        #pragma fragment frag 
        ENDCG
    }
}
Fallback Off
}
