Shader "DeferredShading/PostEffect/Fog" {

Properties {
    _Color ("Color", Vector) = (0.0, 0.0, 0.0, 0.0)
    _Near ("Near", Float) = 5.0
    _Far ("Far", Float) = 10.0
}
SubShader {
    Tags { "Queue"="Transparent" }

    CGINCLUDE
    #include "Compat.cginc"

    sampler2D g_position_buffer;
    float _Near;
    float _Far;
    float4 _Color;


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


    vs_out vert (ia_out v)
    {
        vs_out o;
        o.vertex = v.vertex;
        o.screen_pos = v.vertex;
        return o;
    }

    ps_out frag (vs_out i)
    {
        float2 coord = (i.screen_pos.xy / i.screen_pos.w + 1.0) * 0.5;
        // see: http://docs.unity3d.com/Manual/SL-PlatformDifferences.html
        #if UNITY_UV_STARTS_AT_TOP
            coord.y = 1.0-coord.y;
        #endif

        float4 pos = tex2D(g_position_buffer, coord);
        float d = pos.z-_WorldSpaceCameraPos.z;
        float n = max((d-_Near)/(_Far-_Near), 0.0);
        if(pos.w==0.0) n = 1.0;
        n = max(n, max(abs(pos.y)-10.0, 0.0)/10.0);
        n = min(n ,1.0);
        float4 c = float4(_Color.xyz, n);
        ps_out r = {c};
        return r;
    }
    ENDCG

    Pass {
        Blend SrcAlpha OneMinusSrcAlpha
        ZTest Always
        ZWrite Off
        Cull Back

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
