Shader "DeferredShading/GBufferRM" {

Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_BaseColor ("BaseColor", Vector) = (0.15, 0.15, 0.2, 1.0)
	_GlowColor ("GlowColor", Vector) = (0.75, 0.75, 1.0, 1.0)
}
SubShader {
	Tags { "RenderType"="Opaque" }
	Cull Back

	CGINCLUDE

	sampler2D _MainTex;
	float4 _BaseColor;
	float4 _GlowColor;

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
		float4 normal : COLOR0;
		float4 position : COLOR1;
		float4 color : COLOR2;
		float4 glow : COLOR3;
	};

		
	float  modc(float  a, float  b) { return a - b * floor(a/b); }
	float2 modc(float2 a, float2 b) { return a - b * floor(a/b); }
	float3 modc(float3 a, float3 b) { return a - b * floor(a/b); }
	float4 modc(float4 a, float4 b) { return a - b * floor(a/b); }


	float sdSphere( float3 p, float r )
	{
		return length(p) - r;
	}

	float sdSphere( float2 p, float r )
	{
		return length(p) - r;
	}

	float sdBox( float3 p, float3 b )
	{
		float3 d = abs(p) - b;
		return min(max(d.x,max(d.y,d.z)),0.0) + length(max(d,0.0));
	}

	float sdBox( float2 p, float2 b )
	{
		float2 d = abs(p) - b;
		return min(max(d.x,d.y),0.0) + length(max(d,0.0));
	}


	float3 nrand3( float2 co )
	{
		float3 a = frac( cos( co.x*8.3e-3 + co.y )*float3(1.3e5, 4.7e5, 2.9e5) );
		float3 b = frac( sin( co.x*0.3e-3 + co.y )*float3(8.1e5, 1.0e5, 0.1e5) );
		float3 c = lerp(a, b, 0.5);
		return c;
	}

	float map(float3 p)
	{
		float time = _Time.x * 20.0;
		float h = 1.8;
		float rh = 0.5;
		float grid = 2.0;
		float grid_half = grid*0.5;
		float cube = 0.75;
		float3 orig = p;

		float3 g1 = float3(ceil((orig.x)/grid), ceil((orig.y)/grid), ceil((orig.z)/grid));
		float3 rxz =  nrand3(g1.xz);
		float3 ryz =  nrand3(g1.yz);

		float3 di = ceil(p/4.8);
		p = abs(p+float3(0,time*3.0,0));
		float d1 = p.y + h;
		float d2 = p.x + h;

		float3 p1 = modc(p, grid) - grid_half;
		float c1 = sdBox(p1, cube);

		float3 sphereCenter = float3(-0.0, 2.0, -5.0);
		float3 sc = orig - sphereCenter /*+ float3(sin(time*0.5)*5.0, sin(time*0.75)*3.0, 15.0+cos(time*0.5)*3.0)*/;
		float s1 = sdSphere(sc, 5.0+sin(time*2.0)*1.0 );
		float s2 = sdSphere(sc, 4.0 );
		float sphere = min(max(c1,s1),s2);
	
		return sphere;
	}



	float3 genNormal(float3 p)
	{
		const float d = 0.01;
		return normalize( float3(
			map(p+float3(  d,0.0,0.0))-map(p+float3( -d,0.0,0.0)),
			map(p+float3(0.0,  d,0.0))-map(p+float3(0.0, -d,0.0)),
			map(p+float3(0.0,0.0,  d))-map(p+float3(0.0,0.0, -d)) ));
	}


	vs_out vert (ia_out v)
	{
		vs_out o;
		o.vertex = v.vertex;
		o.screen_pos = v.vertex;
		return o;
	}


	ps_out frag (vs_out i)
	{
		float time = _Time.x * 30.0;
		float2 pos = i.screen_pos.xy ;
		// see: http://docs.unity3d.com/Manual/SL-PlatformDifferences.html
		#if UNITY_UV_STARTS_AT_TOP
			pos.y = 1.0-pos.y;
		#endif
			
		float aspect = _ScreenParams.x / _ScreenParams.y;
		pos.y /= aspect;

		float3 camPos = _WorldSpaceCameraPos.xyz;
		float3 canLookat = 0.0f;
		float3 camDir = normalize(canLookat-camPos);
		float3 camUp  = normalize(float3(0.0, 1.0, 0.0));
		float3 camSide = -cross(camDir, camUp);
		float focus = 1.0;

		float3 rayDir = normalize(camSide*pos.x + camUp*pos.y + camDir*focus);
		float3 ray = camPos;
		int march = 0;
		float d = 0.0;

		float total_d = 0.0;
		const int MAX_MARCH = 32;
		const float MAX_DIST = _ProjectionParams.z;
		int mi = 0;
		for(; mi<=MAX_MARCH; ++mi) {
			d = map(ray);
			march=mi;
			total_d += d;
			ray += rayDir * d;
			if(d<0.001) {break; }
		}
		if(mi==MAX_MARCH || total_d>MAX_DIST) {
			discard;
		}

		float4 rpos = mul(UNITY_MATRIX_MVP, float4(ray, 1.0));
		float3 n = genNormal(ray);

		ps_out o;
		o.normal = float4(n, 0.0);
		o.position = float4(ray, rpos.z);
		o.color = _BaseColor;
		o.glow = _GlowColor;
		return o;
	}
	ENDCG

	Pass {
		ZWrite Off
		ZTest Always

		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma target 3.0
		#pragma glsl
		ENDCG
	}
	Pass {
		Name "DepthPrePass"
		Tags { "DepthPrePass" = "DepthPrePass" }
		ColorMask 0
		ZWrite On
		ZTest LEqual
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma target 3.0
		#pragma glsl
		ENDCG
	}
}
}
