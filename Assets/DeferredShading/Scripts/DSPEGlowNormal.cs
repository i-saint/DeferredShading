using UnityEngine;
using System;
using System.Collections;


public class DSPEGlowNormal : DSEffectBase
{
    public Vector4 baseColor = new Vector4(0.75f, 0.75f, 1.25f, 0.0f);
    public float intensity = 1.0f;
    public float threshold = 0.5f;
    public float edge = 0.2f;
    public Material matGlowNormal;
    Action m_render;

    void OnEnable()
    {
        ResetDSRenderer();
        if (m_render == null)
        {
            m_render = Render;
            GetDSRenderer().AddCallbackPostEffect(m_render);
        }
    }

    void Render()
    {
        if (!enabled) { return; }

        DSRenderer dsr = GetDSRenderer();
        matGlowNormal.SetVector("_BaseColor", baseColor);
        matGlowNormal.SetFloat("g_intensity", intensity);
        matGlowNormal.SetFloat("_Threshold", threshold);
        matGlowNormal.SetFloat("_Edge", edge);
        matGlowNormal.SetTexture("g_position_buffer", dsr.rtPositionBuffer);
        matGlowNormal.SetTexture("g_normal_buffer", dsr.rtNormalBuffer);
        matGlowNormal.SetPass(0);
        DSRenderer.DrawFullscreenQuad();
    }
}
