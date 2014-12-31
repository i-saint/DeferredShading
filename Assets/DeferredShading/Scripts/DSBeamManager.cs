using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



[Serializable]
public class DSBeamEntity
{
    public Vector3 position;
    public Vector3 direction;
    public float lifetime;
    public float speed;
    public float fade_speed;
    public float length;
    public float scale;

    public Matrix4x4 matrix;
    public Vector4 beam_params;

    public void Update()
    {
        lifetime -= Time.deltaTime;
        length += speed * Time.deltaTime;
        beam_params.Set(direction.x, direction.y, direction.z, length);
        if (lifetime<=0.0f)
        {
            scale -= fade_speed;
        }
        matrix = Matrix4x4.TRS(position, Quaternion.LookRotation(direction), Vector3.one * scale);
    }

    public bool IsDead()
    {
        return scale <= 0.0f;
    }

    public void Fade()
    {
        lifetime = 0.0f;
    }

    public void Kill()
    {
        scale = 0.0f;
    }
}


public class DSBeamManager : DSEffectBase
{
    public static DSBeamManager s_instance;

    public Material m_material;
    public Mesh m_mesh;
    public List<DSBeamEntity> m_entities = new List<DSBeamEntity>();
    int m_i_beam_direction;
    int m_i_base_position;
    Action m_depth_prepass;
    Action m_render;

    public static void AddEntity(DSBeamEntity e)
    {
        if (!s_instance.enabled) return ;
        s_instance.m_entities.Add(e);
    }

    public static DSBeamEntity AddEntity(Vector3 pos, Vector3 dir, float speed = 20.0f, float fade_speed = 0.025f, float lifetime = 2.0f, float scale = 1.0f)
    {
        if (!s_instance.enabled) return null;
        DSBeamEntity e = new DSBeamEntity {
            position = pos,
            direction = dir,
            speed = 20.0f,
            fade_speed = fade_speed,
            lifetime = lifetime,
            scale = scale,
            length = 0.0f,
        };
        s_instance.m_entities.Add(e);
        return e;
    }


    void OnEnable()
    {
        ResetDSRenderer();
        s_instance = this;
        if (m_depth_prepass==null)
        {
            m_depth_prepass = DepthPrePass;
            m_render = Render;
            GetDSRenderer().AddCallbackPreGBuffer(m_depth_prepass);
            GetDSRenderer().AddCallbackPostGBuffer(m_render);

            m_i_beam_direction = Shader.PropertyToID("beam_direction");
            m_i_base_position = Shader.PropertyToID("base_position");
        }
    }

    void OnDisable()
    {
        if (s_instance == this) s_instance = null;
    }

    void Update()
    {
        m_entities.ForEach((a) => { a.Update(); });
        m_entities.RemoveAll((a) => { return a.IsDead(); });
    }

    void DepthPrePass()
    {
        if (!enabled || m_entities.Count == 0) { return; }
        m_entities.ForEach((a) =>
        {
            m_material.SetVector(m_i_beam_direction, a.beam_params);
            m_material.SetVector(m_i_base_position, a.position);
            m_material.SetPass(0);
            Graphics.DrawMeshNow(m_mesh, a.matrix);
        });
    }

    void Render()
    {
        if (!enabled || m_entities.Count==0) { return; }
        m_entities.ForEach((a) =>
        {
            m_material.SetVector(m_i_beam_direction, a.beam_params);
            m_material.SetVector(m_i_base_position, a.position);
            m_material.SetPass(1);
            Graphics.DrawMeshNow(m_mesh, a.matrix);
        });
    }
}
