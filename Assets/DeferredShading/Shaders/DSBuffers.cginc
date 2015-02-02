sampler2D _FrameBuffer;
sampler2D _NormalBuffer;
sampler2D _PositionBuffer;
sampler2D _AlbedoBuffer;
sampler2D _EmissionBuffer;

sampler2D _PrevFrameBuffer;
sampler2D _PrevNormalBuffer;
sampler2D _PrevPositionBuffer;
sampler2D _PrevAlbedoBuffer;
sampler2D _PrevEmissionBuffer;


float4 SampleFrame(float2 uv)   { return tex2D(_FrameBuffer, uv); }
float4 SampleNormal(float2 uv)  { return tex2D(_NormalBuffer, uv); }
float4 SamplePosition(float2 uv){ return tex2D(_PositionBuffer, uv); }
float4 SampleAlbedo(float2 uv)  { return tex2D(_AlbedoBuffer, uv); }
float4 SampleEmission(float2 uv){ return tex2D(_EmissionBuffer, uv); }

float4 SamplePrevFrame(float2 uv)   { return tex2D(_PrevFrameBuffer, uv); }
float4 SamplePrevNormal(float2 uv)  { return tex2D(_PrevNormalBuffer, uv); }
float4 SamplePrevPosition(float2 uv){ return tex2D(_PrevPositionBuffer, uv); }
float4 SamplePrevAlbedo(float2 uv)  { return tex2D(_PrevAlbedoBuffer, uv); }
float4 SamplePrevEmission(float2 uv){ return tex2D(_PrevEmissionBuffer, uv); }
