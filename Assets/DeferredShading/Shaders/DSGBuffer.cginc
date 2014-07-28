
sampler2D _MainTex;
float4 _BaseColor;
float4 _GlowColor;


struct ia_out
{
	float4 vertex : POSITION;
	float4 normal : NORMAL;
};

struct vs_out
{
	float4 vertex : SV_POSITION;
	float4 screen_pos : TEXCOORD0;
	float4 position : TEXCOORD1;
	float4 normal : TEXCOORD2;
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
	o.normal = normalize(mul(_Object2World, float4(v.normal.xyz,0.0)));
	return o;
}

ps_out frag(vs_out i)
{
	ps_out o;
	o.normal = i.normal;
	o.position = float4(i.position.xyz, i.screen_pos.z);
	o.color = _BaseColor;
	o.glow = _GlowColor;
	return o;
}
