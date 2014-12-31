using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class DSRadialBlurEntity
{
    public Vector3 position;
    public float size = 15.0f;
    public float strength = 0.2f;
    public float fade_speed = 0.1f;
    public float pow = 0.7f;
    public float time = 0.0f;
    public Vector4 stretch;
    public Vector4 color_bias;

    public Matrix4x4 matrix;
    public Vector4 radialblur_params;

    public void Update()
    {
        time += Time.deltaTime;
        strength -= fade_speed*Time.deltaTime;
        float s = size * 0.49f;
        radialblur_params.Set(s, 1.0f / s, strength, pow);
        matrix = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one*size);
    }

    public bool IsDead()
    {
        return strength <= 0.0f;
    }
}


public class DSRadialBlurManager : DSEffectBase
{
    public static DSRadialBlurManager s_instance;

    public Material m_material;
    public Mesh m_mesh;
    public List<DSRadialBlurEntity> m_entities = new List<DSRadialBlurEntity>();
    int m_i_radialblur_params;
    int m_i_stretch_params;
    int m_i_color_bias;
    int m_i_base_position;
    Action m_render;


    public static void AddEntity(DSRadialBlurEntity e)
    {
        if (!s_instance.enabled) return;
        s_instance.m_entities.Add(e);
    }

    public static DSRadialBlurEntity AddEntity(
        Vector3 pos, float size = 20.0f, float strength = 0.5f, float fade_speed = 0.5f, float pow = 0.7f)
    {
        if (!s_instance.enabled) return null;
        DSRadialBlurEntity e = new DSRadialBlurEntity
        {
            position = pos,
            size = size,
            strength = strength,
            fade_speed = fade_speed,
            pow = pow,
        };
        s_instance.m_entities.Add(e);
        return e;
    }


    void OnEnable()
    {
        ResetDSRenderer();
        s_instance = this;
        if (m_render == null)
        {
            m_render = Render;
            GetDSRenderer().AddCallbackPostEffect(m_render, 10000);
            m_i_radialblur_params = Shader.PropertyToID("radialblur_params");
            m_i_stretch_params = Shader.PropertyToID("stretch_params");
            m_i_color_bias = Shader.PropertyToID("color_bias");
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

    void Render()
    {
        if (!enabled || m_entities.Count == 0) { return; }
        GetDSRenderer().UpdateShadowFramebuffer();
        m_entities.ForEach((a) =>
        {
            m_material.SetVector(m_i_radialblur_params, a.radialblur_params);
            m_material.SetVector(m_i_stretch_params, a.stretch);
            m_material.SetVector(m_i_color_bias, a.color_bias);
            m_material.SetVector(m_i_base_position, a.position);
            m_material.SetPass(0);
            Graphics.DrawMeshNow(m_mesh, a.matrix);
        });
    }
}
