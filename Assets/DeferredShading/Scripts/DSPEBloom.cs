using UnityEngine;
using System;
using System.Collections;

public class DSPEBloom : DSEffectBase
{
    public float m_intensity = 1.5f;
    public Material m_mat_bloom_luminance;
    public Material m_mat_bloom_blur;
    public Material m_mat_bloom_combine;
    public RenderTexture[] m_rt_half;
    public RenderTexture[] m_rt_quarter;
    Action m_render;

    void OnEnable()
    {
        ResetDSRenderer();
        if (m_render == null)
        {
            m_render = Render;
            GetDSRenderer().AddCallbackPostEffect(m_render, 5200);
            m_rt_half = new RenderTexture[2];
            m_rt_quarter = new RenderTexture[2];
        }
    }

    void UpdateRenderTargets()
    {
        if (m_rt_half[0] == null || !m_rt_half[0].IsCreated())
        {
            for (int i = 0; i < 2; ++i)
            {
                m_rt_half[i] = DSRenderer.CreateRenderTexture(256, 256, 0, RenderTextureFormat.ARGBHalf);
                m_rt_half[i].filterMode = FilterMode.Trilinear;
                m_rt_quarter[i] = DSRenderer.CreateRenderTexture(128, 128, 0, RenderTextureFormat.ARGBHalf);
                m_rt_quarter[i].filterMode = FilterMode.Trilinear;
            }
        }
    }
    
    void Render()
    {
        if (!enabled) { return; }
        UpdateRenderTargets();

        DSRenderer dsr = GetDSRenderer();
        Vector4 hscreen = new Vector4(m_rt_half[0].width, m_rt_half[0].height, 1.0f / m_rt_half[0].width, 1.0f / m_rt_half[0].height);
        Vector4 qscreen = new Vector4(m_rt_quarter[0].width, m_rt_quarter[0].height, 1.0f / m_rt_quarter[0].width, 1.0f / m_rt_quarter[0].height);
        m_mat_bloom_blur.SetVector("_Screen", hscreen);

        Graphics.SetRenderTarget(m_rt_half[0]);
        m_mat_bloom_blur.SetTexture("g_glow_buffer", dsr.rtEmissionBuffer);
        m_mat_bloom_blur.SetPass(0);
        DSRenderer.DrawFullscreenQuad();
        Graphics.SetRenderTarget(m_rt_half[1]);
        m_mat_bloom_blur.SetTexture("g_glow_buffer", m_rt_half[0]);
        m_mat_bloom_blur.SetPass(0);
        DSRenderer.DrawFullscreenQuad();
        Graphics.SetRenderTarget(m_rt_half[0]);
        m_mat_bloom_blur.SetTexture("g_glow_buffer", m_rt_half[1]);
        m_mat_bloom_blur.SetPass(0);
        DSRenderer.DrawFullscreenQuad();
        Graphics.SetRenderTarget(m_rt_half[1]);
        m_mat_bloom_blur.SetTexture("g_glow_buffer", m_rt_half[0]);
        m_mat_bloom_blur.SetPass(1);
        DSRenderer.DrawFullscreenQuad();

        m_mat_bloom_blur.SetVector("_Screen", qscreen);
        Graphics.SetRenderTarget(m_rt_quarter[0]);
        m_mat_bloom_blur.SetTexture("g_glow_buffer", dsr.rtEmissionBuffer);
        m_mat_bloom_blur.SetPass(0);
        DSRenderer.DrawFullscreenQuad();
        Graphics.SetRenderTarget(m_rt_quarter[1]);
        m_mat_bloom_blur.SetTexture("g_glow_buffer", m_rt_quarter[0]);
        m_mat_bloom_blur.SetPass(0);
        DSRenderer.DrawFullscreenQuad();
        Graphics.SetRenderTarget(m_rt_quarter[0]);
        m_mat_bloom_blur.SetTexture("g_glow_buffer", m_rt_quarter[1]);
        m_mat_bloom_blur.SetPass(0);
        DSRenderer.DrawFullscreenQuad();
        Graphics.SetRenderTarget(m_rt_quarter[1]);
        m_mat_bloom_blur.SetTexture("g_glow_buffer", m_rt_quarter[0]);
        m_mat_bloom_blur.SetPass(1);
        DSRenderer.DrawFullscreenQuad();

        Graphics.SetRenderTarget(dsr.rtComposite);
        m_mat_bloom_combine.SetTexture("g_glow_buffer", dsr.rtEmissionBuffer);
        m_mat_bloom_combine.SetTexture("g_half_glow_buffer", m_rt_half[1]);
        m_mat_bloom_combine.SetTexture("g_quarter_glow_buffer", m_rt_quarter[1]);
        m_mat_bloom_combine.SetFloat("g_intensity", m_intensity);
        m_mat_bloom_combine.SetPass(0);
        DSRenderer.DrawFullscreenQuad();
    }
}
