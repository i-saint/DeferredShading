sampler2D g_frame_buffer;
sampler2D g_normal_buffer;
sampler2D g_position_buffer;
sampler2D g_albedo_buffer;
sampler2D g_emission_buffer;

sampler2D g_prev_frame_buffer;
sampler2D g_prev_normal_buffer;
sampler2D g_prev_position_buffer;
sampler2D g_prev_albedo_buffer;
sampler2D g_prev_emission_buffer;


float4 SampleFrame(float2 uv)   { return tex2D(g_frame_buffer, uv); }
float4 SampleNormal(float2 uv)  { return tex2D(g_normal_buffer, uv); }
float4 SamplePosition(float2 uv){ return tex2D(g_position_buffer, uv); }
float4 SampleAlbedo(float2 uv)  { return tex2D(g_albedo_buffer, uv); }
float4 SampleEmission(float2 uv){ return tex2D(g_emission_buffer, uv); }

float4 SamplePrevFrame(float2 uv)   { return tex2D(g_prev_frame_buffer, uv); }
float4 SamplePrevNormal(float2 uv)  { return tex2D(g_prev_normal_buffer, uv); }
float4 SamplePrevPosition(float2 uv){ return tex2D(g_prev_position_buffer, uv); }
float4 SamplePrevAlbedo(float2 uv)  { return tex2D(g_prev_albedo_buffer, uv); }
float4 SamplePrevEmission(float2 uv){ return tex2D(g_prev_emission_buffer, uv); }
