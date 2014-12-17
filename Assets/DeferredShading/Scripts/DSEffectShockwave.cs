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

    public DSShockwave(Vector3 pos, float gap = -0.5f, float fade_speed = 2.0f, float opacity = 1.5f, float scale = 1.0f)
    {
        this.pos = pos;
        this.gap = gap;
        this.fade_speed = fade_speed;
        this.opacity = opacity;
        this.scale = scale;
    }

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
    int i_shockwave_params;
    public List<DSShockwave> entries = new List<DSShockwave>();


    public static DSShockwave AddEntry(Vector3 pos, float gap = -0.5f, float fade_speed = 2.0f, float opacity = 1.5f, float scale = 1.0f)
    {
        DSShockwave e = new DSShockwave(pos, gap, fade_speed, opacity, scale);
        instance.entries.Add(e);
        return e;
    }


    void Awake()
    {
        instance = this;
        UpdateDSRenderer();
        dsr.AddCallbackPostEffect(() => { Render(); }, 5000);

        i_shockwave_params = Shader.PropertyToID("shockwave_params");
    }

    void OnDestroy()
    {
        entries.Clear();
        instance = null;
    }

    void Update()
    {
        entries.ForEach((a) => { a.Update(); });
        entries.RemoveAll((a) => { return a.IsDead(); });
    }

    void Render()
    {
        if (!enabled || entries.Count == 0) { return; }
        dsr.UpdateShadowFramebuffer();
        entries.ForEach((a) => {
            mat.SetVector(i_shockwave_params, a.shockwave_params);
            mat.SetPass(0);
            Graphics.DrawMeshNow(dsr.mesh_sphere, a.matrix);
        });
    }
}
