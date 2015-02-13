using UnityEngine;
using System;
using System.Collections;

public class DSPEWater : DSEffectBase
{
    public float m_speed = 1.00f;
    public float m_refraction = 0.05f;
    public float m_reflection_intensity = 0.3f;
    public float m_fresnel = 0.25f;
    public float m_raymarch_step = 0.2f;
    public float m_attenuation_by_distance = 0.02f;
    public Material m_material;
    Action m_render;


    void OnEnable()
    {
        ResetDSRenderer();
        if (m_render == null)
        {
            m_render = Render;
            GetDSRenderer().AddCallbackPostEffect(m_render, 5500);
        }
    }


    void Render()
    {
        if (!enabled || m_material == null) { return; }

        DSRenderer dsr = GetDSRenderer();
        dsr.CopyFramebuffer();
        Graphics.SetRenderTarget(dsr.rtComposite.colorBuffer, dsr.rtNormalBuffer.depthBuffer);
        m_material.SetTexture("g_position_buffer", dsr.rtPositionBuffer);
        m_material.SetTexture("g_normal_buffer", dsr.rtNormalBuffer);
        m_material.SetFloat("g_speed", m_speed);
        m_material.SetFloat("g_refraction", m_refraction);
        m_material.SetFloat("g_reflection_intensity", m_reflection_intensity);
        m_material.SetFloat("g_fresnel", m_fresnel);
        m_material.SetFloat("g_raymarch_step", m_raymarch_step);
        m_material.SetFloat("g_attenuation_by_distance", m_attenuation_by_distance);
        m_material.SetPass(0);

        DSPEWaterEntity.GetInstances().ForEach((e) =>
        {
            Graphics.DrawMeshNow(e.GetMesh(), e.GetMatrix());
        });
        Graphics.SetRenderTarget(dsr.rtComposite);
    }
}
