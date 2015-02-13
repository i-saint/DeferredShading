using System;
using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DSPEToneMapping : DSEffectBase
{
    public Vector3 m_range_min = Vector3.zero;
    public Vector3 m_range_max = Vector3.one;
    public Vector3 m_pow = Vector3.one;
    public Material m_mat_tonemapping;
    Action m_render;

#if UNITY_EDITOR
    void Reset()
    {
        m_mat_tonemapping = AssetDatabase.LoadAssetAtPath("Assets/DeferredShading/Materials/Posteffect_ToneMapping.mat", typeof(Material)) as Material;
    }
#endif

    void OnEnable()
    {
        ResetDSRenderer();
        if (m_render == null)
        {
            m_render = Render;
            GetDSRenderer().AddCallbackPostEffect(m_render, 5900);
        }
    }

    void Render()
    {
        if (!enabled) { return; }

        DSRenderer dsr = GetDSRenderer();
        dsr.SwapFramebuffer();
        m_mat_tonemapping.SetVector("g_range_min", m_range_min);
        m_mat_tonemapping.SetVector("g_range_max", m_range_max);
        m_mat_tonemapping.SetVector("g_pow", m_pow);
        m_mat_tonemapping.SetPass(0);
        DSRenderer.DrawFullscreenQuad();
    }
}
