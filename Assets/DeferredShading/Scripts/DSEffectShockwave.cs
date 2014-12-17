using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class DSShockwave
{
    public Vector3 pos;
    public float gap = -0.5f;
    public float speed = 20.0f;
    public float fade_speed = 2.0f;
    public float opacity = 1.5f;
    public float scale = 1.0f;
    public float time = 0.0f;

    public Matrix4x4 matrix;
    public Vector4 shockwave_params;

    public void Update()
    {
        time += Time.deltaTime;
        scale += speed * Time.deltaTime;
        opacity -= fade_speed * Time.deltaTime;
        shockwave_params.Set(opacity, gap, gap, gap);
        matrix = Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one * scale);
    }

    public bool IsDead()
    {
        return opacity <= 0.0f;
    }
}

public class DSEffectShockwave : DSEffectBase
{
    public static DSEffectShockwave instance;

    public Material mat;
    public Mesh mesh;
    int i_shockwave_params;
    public List<DSShockwave> entries = new List<DSShockwave>();


    public static DSShockwave AddEntry(Vector3 pos, float gap = -0.5f, float fade_speed = 2.0f, float opacity = 1.5f, float scale = 1.0f)
    {
        DSShockwave e = new DSShockwave
        {
            pos = pos,
            gap = gap,
            fade_speed = fade_speed,
            opacity = opacity,
            scale = scale,
        };
        instance.entries.Add(e);
        return e;
    }


    public override void Awake()
    {
        base.Awake();
        instance = this;
        GetDSRenderer().AddCallbackPostEffect(() => { Render(); }, 5000);
        i_shockwave_params = Shader.PropertyToID("shockwave_params");
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    public override void Update()
    {
        base.Update();
        entries.ForEach((a) => { a.Update(); });
        entries.RemoveAll((a) => { return a.IsDead(); });
    }

    void Render()
    {
        if (!enabled || entries.Count == 0) { return; }
        GetDSRenderer().UpdateShadowFramebuffer();
        entries.ForEach((a) => {
            mat.SetVector(i_shockwave_params, a.shockwave_params);
            mat.SetPass(0);
            Graphics.DrawMeshNow(mesh, a.matrix);
        });
    }
}

