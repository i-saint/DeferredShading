using System;
using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DSPEVignette : DSEffectBase
{
    public float m_darkness = 0.7f;
    public float m_monochrome = 0.0f;
    public float m_scanline = 0.0f;
    public float m_scanline_scale = 1.0f;
    public Vector3 m_color_shearing = Vector3.zero;
    public Material m_material;
    Action m_render;

#if UNITY_EDITOR
    void Reset()
    {
        m_material = AssetDatabase.LoadAssetAtPath("Assets/DeferredShading/Materials/Posteffect_Vignette.mat", typeof(Material)) as Material;
    }
#endif

    void OnEnable()
    {
        ResetDSRenderer();
        if (m_render == null)
        {
            m_render = Render;
            GetDSRenderer().AddCallbackPostEffect(m_render, 6000);
        }
    }

    void Render()
    {
        if (!enabled) { return; }

        DSRenderer dsr = GetDSRenderer();
        dsr.SwapFramebuffer();
        m_material.SetFloat("g_darkness", m_darkness);
        m_material.SetFloat("g_monochrome", m_monochrome);
        m_material.SetFloat("g_scanline", m_scanline);
        m_material.SetFloat("g_scanline_scale", m_scanline_scale);
        m_material.SetVector("g_color_shearing", m_color_shearing);
        m_material.SetPass(0);
        DSRenderer.DrawFullscreenQuad();
    }
}
