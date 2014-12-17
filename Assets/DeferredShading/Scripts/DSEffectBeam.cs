using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class DSBeam
{
    public enum State
    {
        Active,
        Fading,
    }

    public Vector3 pos;
    public Vector3 dir;
    public float speed = 20.0f;
    public float length = 0.0f;
    public float fade_speed = 0.025f;
    public float lifetime = 2.0f;
    public float scale = 1.0f;
    public float time = 0.0f;
    public State state = State.Active;

    public Matrix4x4 matrix;
    public Vector4 beam_params;

    public DSBeam(Vector3 pos, Vector3 dir, float fade_speed = 0.025f, float lifetime = 2.0f, float scale = 1.0f)
    {
        this.pos = pos;
        this.dir = dir;
        this.fade_speed = fade_speed;
        this.lifetime = lifetime;
        this.scale = scale;
    }

    public void Update()
    {
        time += Time.deltaTime;
        length = speed * time;
        beam_params.Set(dir.x, dir.y, dir.z, length);

        if (state == State.Active)
        {
            if (time > lifetime)
            {
                state = State.Fading;
            }
        }
        else if (state == State.Fading)
        {
            scale -= fade_speed;
        }
        matrix = Matrix4x4.TRS(pos, Quaternion.LookRotation(dir), Vector3.one * scale);
    }

    public bool IsDead()
    {
        return scale <= 0.0f;
    }
}


public class DSEffectBeam : DSEffectBase
{
    public static DSEffectBeam instance;

    public Material mat;
    int i_beam_direction;
    int i_base_position;
    public List<DSBeam> entries = new List<DSBeam>();

    public static DSBeam AddEntry(Vector3 pos, Vector3 dir, float fade_speed = 0.025f, float lifetime = 2.0f, float scale = 1.0f)
    {
        DSBeam e = new DSBeam(pos, dir, fade_speed, lifetime, scale);
        instance.entries.Add(e);
        return e;
    }

    void Awake()
    {
        instance = this;
        UpdateDSRenderer();
        dsr.AddCallbackPreGBuffer(() => { DepthPrePass(); });
        dsr.AddCallbackPostGBuffer(() => { Render(); });

        i_beam_direction = Shader.PropertyToID("beam_direction");
        i_base_position = Shader.PropertyToID("base_position");
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    void Update()
    {
        entries.ForEach((a) => { a.Update(); });
        entries.RemoveAll((a) => { return a.IsDead(); });
    }

    void DepthPrePass()
    {
        if (!enabled || entries.Count == 0) { return; }
        entries.ForEach((a) =>
        {
            mat.SetVector(i_beam_direction, a.beam_params);
            mat.SetVector(i_base_position, a.pos);
            mat.SetPass(0);
            Graphics.DrawMeshNow(dsr.mesh_sphere, a.matrix);
        });
    }

    void Render()
    {
        if (!enabled || entries.Count==0) { return; }
        entries.ForEach((a) =>
        {
            mat.SetVector(i_beam_direction, a.beam_params);
            mat.SetVector(i_base_position, a.pos);
            mat.SetPass(1);
            Graphics.DrawMeshNow(dsr.mesh_sphere, a.matrix);
        });
    }
}
