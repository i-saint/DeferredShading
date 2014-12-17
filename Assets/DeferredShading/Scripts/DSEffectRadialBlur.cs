using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class DSRadialBlur
{
    public Vector3 pos;
    public float speed = 5.0f;
    public float opacity = 1.5f;
    public float fade_speed = 0.025f;
    public float scale = 3.0f;
    public float time = 0.0f;

    public Matrix4x4 matrix;
    public Vector4 radialblur_params;

    public void Update()
    {
        time += Time.deltaTime;
        opacity -= fade_speed*Time.deltaTime;
        radialblur_params.Set(opacity, opacity, opacity, opacity);
        matrix = Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one*scale);
    }

    public bool IsDead()
    {
        return opacity <= 0.0f;
    }
}


public class DSEffectRadialBlur : DSEffectBase
{
    public static DSEffectRadialBlur instance;

    public Material mat;
    public Mesh mesh;
    int i_radialblur_params;
    int i_base_position;
    public List<DSRadialBlur> entries = new List<DSRadialBlur>();

    public override void Awake()
    {
        base.Awake();
        instance = this;
        i_radialblur_params = Shader.PropertyToID("radialblur_params");
        i_base_position = Shader.PropertyToID("base_position");
    }

    public override void OnReload()
    {
        base.OnReload();
        dsr.AddCallbackPostEffect(() => { Render(); }, 10000);
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
        dsr.UpdateShadowFramebuffer();
        entries.ForEach((a) =>
        {
            mat.SetVector(i_radialblur_params, a.radialblur_params);
            mat.SetVector(i_base_position, a.pos);
            mat.SetPass(0);
            Graphics.DrawMeshNow(mesh, a.matrix);
        });
    }
}
