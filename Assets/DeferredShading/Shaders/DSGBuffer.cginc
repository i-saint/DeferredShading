
#ifdef SHADER_API_PSSL
#	define COLOR0 SV_Target0
#	define COLOR1 SV_Target1
#	define COLOR2 SV_Target2
#	define COLOR3 SV_Target3
#endif

sampler2D _Albedo;
sampler2D _Gloss;
sampler2D _Normal;
sampler2D _Specular;
float4 _BaseColor;
float4 _GlowColor;


struct ia_out
{
	float4 vertex : POSITION;
	float4 normal : NORMAL;
	float2 texcoord : TEXCOORD0;
	float4 tangent : TANGENT;
};

struct vs_out
{
	float4 vertex : SV_POSITION;
	float4 screen_pos : TEXCOORD0;
	float4 position : TEXCOORD1;
	float3 normal : TEXCOORD2;
	float2 texcoord : TEXCOORD3;
	float4 tangent : TEXCOORD4;
	float3 binormal : TEXCOORD5;
};

struct ps_out
{
	float4 normal : COLOR0;
	float4 position : COLOR1;
	float4 color : COLOR2;
	float4 glow : COLOR3;
};


vs_out vert(ia_out v)
{
	vs_out o;
	float4 vmvp = mul(UNITY_MATRIX_MVP, v.vertex);
	o.vertex = vmvp;
	o.screen_pos = vmvp;
	o.position = mul(_Object2World, v.vertex);
	o.normal = normalize(mul(_Object2World, float4(v.normal.xyz,0.0)).xyz);
	o.tangent = float4(normalize(mul(_Object2World, float4(v.tangent.xyz,0.0)).xyz), v.tangent.w);
	o.binormal = normalize(cross(o.normal, o.tangent) * v.tangent.w);
	o.texcoord = v.texcoord;
	return o;
}

ps_out frag(vs_out i)
{
	ps_out o;
	o.normal = float4(i.normal.xyz, 1.0);
	o.position = float4(i.position.xyz, i.screen_pos.z);
	o.color = _BaseColor;
	o.glow = _GlowColor;
	return o;
}
