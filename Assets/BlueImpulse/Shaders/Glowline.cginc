
float2 boxcell(float3 p3, float3 n)
{
    float2 p;
    float t = 0.7;
    if(abs(n.x)>t) {
        p = p3.yz;
    }
    else if(abs(n.z)>t) {
        p = p3.xy;
    }
    else {
        p = p3.xz;
    }

    p = frac(p);
    float r = 0.123;
    float v = 0.0, g = 0.0;
    r = frac(r * 9184.928);
    float cp, d;

    d = p.x;
    g += pow(clamp(1.0 - abs(d), 0.0, 1.0), 1000.0);
    d = p.y;
    g += pow(clamp(1.0 - abs(d), 0.0, 1.0), 1000.0);
    d = p.x - 1.0;
    g += pow(clamp(3.0 - abs(d), 0.0, 1.0), 1000.0);
    d = p.y - 1.0;
    g += pow(clamp(1.0 - abs(d), 0.0, 1.0), 10000.0);

    const int iter = 11;
    for(int i = 0; i < iter; i ++)
    {
        cp = 0.5 + (r - 0.5) * 0.9;
        d = p.x - cp;
        //g += pow(clamp(1.0 - abs(d), 0.0, 1.0), 200.0);
        g += clamp(1.0 - abs(d), 0.0, 1.0) > 0.999-(0.00075*i) ? 1.0 : 0.0;
        if(d > 0.0) {
            r = frac(r * 4829.013);
            p.x = (p.x - cp) / (1.0 - cp);
            v += 1.0;
        }
        else {
            r = frac(r * 1239.528);
            p.x = p.x / cp;
        }
        p = p.yx;
    }
    v /= float(iter);
    return float2(g, v);
}

float hash( float n )
{
    return frac(sin(n)*43758.5453);
}

