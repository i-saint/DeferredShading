Shader "DeferredShading/GBufferTextured" {

Properties {
    _AlbedoMap ("Albedo", 2D) = "white" {}
    _GlossMap ("Gloss", 2D) = "white" {}
    _NormalMap ("Normal", 2D) = "white" {}
    _SpecularMap ("Specular", 2D) = "white" {}
    _BaseColor ("BaseColor", Vector) = (0.15, 0.15, 0.2, 1.0)
    _GlowColor ("GlowColor", Vector) = (0.0, 0.0, 0.0, 0.0)
}
SubShader {
    Tags { "RenderType"="Opaque" "Queue"="Geometry" }

    CGINCLUDE
    #include "Compat.cginc"
    #include "DS.cginc"
    #include "DSGBuffer.cginc"
    ENDCG

    Pass {
        Name "DepthPrePass"
        Tags { "RenderType"="Opaque" "Queue"="Geometry-1" }
        Cull Back
        ZWrite On
        ZTest Less

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag_textured
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
        #pragma fragment frag_textured
        #pragma target 3.0
        #ifdef SHADER_API_OPENGL 
            #pragma glsl
        #endif
        ENDCG
    }
}
Fallback Off
}
