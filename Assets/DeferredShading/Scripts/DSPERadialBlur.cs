using System;
using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DSPERadialBlur : DSEffectBase
{
    public float m_intensity = 1.0f;
    public float m_radius = 0.0f;
    public Vector3 m_center = Vector3.zero;
    public Vector3 m_threshold = Vector3.zero;
    public Material m_material;
    Action m_render;

#if UNITY_EDITOR
    void Reset()
    {
        m_material = AssetDatabase.LoadAssetAtPath("Assets/DeferredShading/Materials/Posteffect_RadialBlur.mat", typeof(Material)) as Material;
    }
#endif

    void OnEnable()
    {
        ResetDSRenderer();
        if (m_render == null)
        {
            m_render = Render;
            GetDSRenderer().AddCallbackPostEffect(m_render, 5950);
        }
    }

    void Render()
    {
        if (!enabled) { return; }

        DSRenderer dsr = GetDSRenderer();
        dsr.SwapFramebuffer();
        m_material.SetFloat("g_intensity", m_intensity);
        m_material.SetFloat("g_radius", m_radius);
        m_material.SetVector("g_center", m_center);
        m_material.SetVector("g_threshold", m_threshold);
        m_material.SetPass(0);
        DSRenderer.DrawFullscreenQuad();
    }
}
