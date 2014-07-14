Shader "DeferredShading/Fill" {
	Properties {
	}
	SubShader {
		Tags { "RenderType"="Transparent" }
		ZTest Always
		ZWrite Off
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha

		CGINCLUDE

		struct vs_in
		{
			float4 vertex : POSITION;
		};

		struct ps_in {
			float4 vertex : SV_POSITION;
			float4 screen_pos : TEXCOORD0;
		};

		struct ps_out
		{
			float4 color : COLOR0;
		};


		ps_in vert (vs_in v)
		{
			ps_in o;
			o.vertex = v.vertex;
			o.screen_pos = v.vertex;
			return o;
		}

		ps_out frag (ps_in i)
		{
			ps_out o;
			o.color = float4(0,0,0, 0.3);
			return o;
		}
		ENDCG

	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma target 3.0
		#pragma glsl
		ENDCG
	}
	} 
}
