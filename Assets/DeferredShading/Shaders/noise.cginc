

float hash(float2 p)
{
    float h = dot(p,float2(127.1, 311.7));
    return frac(sin(h)*43758.5453123);
}
float hash(float3 p)
{
    float h = dot(p,float3(127.1, 311.7, 496.3));
    return frac(sin(h)*43758.5453123);
}

float noise( float2 p )
{
    float2 i = floor( p );
    float2 f = frac( p );
    float2 u = f*f*(3.0-2.0*f);
    return -1.0+2.0*lerp( lerp( hash( i + float2(0.0,0.0) ), 
                     hash( i + float2(1.0,0.0) ), u.x),
                lerp( hash( i + float2(0.0,1.0) ), 
                     hash( i + float2(1.0,1.0) ), u.x), u.y);
}

float noise( float3 p )
{
    float3 i = floor( p );
    float3 f = frac( p );
    float3 u = f*f*(3.0-2.0*f);
    return -1.0+2.0*lerp( lerp( hash( i + float2(0.0,0.0) ), 
                     hash( i + float2(1.0,0.0) ), u.x),
                lerp( hash( i + float2(0.0,1.0) ), 
                     hash( i + float2(1.0,1.0) ), u.x), u.y);
}

float sea_octave(float2 uv, float choppy)
{
    uv += noise(uv);
    float2 wv = 1.0-abs(sin(uv));
    float2 swv = abs(cos(uv));
    wv = lerp(wv,swv,wv);
    return pow(1.0-pow(wv.x * wv.y,0.65),choppy);
}

float sea_octave(float3 uv, float choppy)
{
    uv += noise(uv);
    float3 wv = 1.0-abs(sin(uv));
    float3 swv = abs(cos(uv));
    wv = lerp(wv,swv,wv);
    return pow(1.0-pow(wv.x * wv.y * wv.z,0.65),choppy);
}



float compute_octave(float3 pos, float scale)
{
    float o1 = sea_octave(pos.xzy*1.25*scale + float3(1.0,2.0,-1.5)*_Time.y*1.25 + sin(pos.xzy+_Time.y*8.3)*0.15, 4.0);
    float o2 = sea_octave(pos.xzy*2.50*scale + float3(2.0,-1.0,1.0)*_Time.y*-2.0 - sin(pos.xzy+_Time.y*6.3)*0.2, 8.0);
    return o1 * o2;
}

float3 guess_normal(float3 p, float scale)
{
    const float d = 0.1;
    return normalize( float3(
        compute_octave(p+float3(  d,0.0,0.0), scale)-compute_octave(p+float3( -d,0.0,0.0), scale),
        compute_octave(p+float3(0.0,  d,0.0), scale)-compute_octave(p+float3(0.0, -d,0.0), scale),
        compute_octave(p+float3(0.0,0.0,  d), scale)-compute_octave(p+float3(0.0,0.0, -d), scale) ));
}
