using UnityEngine;
using System;
using System.Collections;

public class DSPENormalPattern : DSEffectBase
{
    public Material matNormalPattern;
    public Material matCopyGBuffer;
    public RenderTexture rtNormalCopy;
    Action m_render;

    void OnEnable()
    {
        ResetDSRenderer();
        if (m_render == null)
        {
            m_render = Render;
            GetDSRenderer().AddCallbackPostGBuffer(m_render, 1100);
        }
    }

    void UpdateRenderTargets()
    {
        Vector2 reso = GetDSRenderer().GetInternalResolution();
        if (rtNormalCopy != null && rtNormalCopy.width!=(int)reso.x)
        {
            rtNormalCopy.Release();
            rtNormalCopy = null;
        }
        if (rtNormalCopy == null || !rtNormalCopy.IsCreated())
        {
            rtNormalCopy = DSRenderer.CreateRenderTexture((int)reso.x, (int)reso.y, 0, RenderTextureFormat.ARGBHalf);
        }
    }

    void Render()
    {
        if (!enabled) { return; }

        UpdateRenderTargets();

        DSRenderer dsr = GetDSRenderer();
        Graphics.SetRenderTarget(rtNormalCopy);
        GL.Clear(false, true, Color.black);
        matNormalPattern.SetTexture("g_position_buffer", dsr.rtPositionBuffer);
        matNormalPattern.SetTexture("g_normal_buffer", dsr.rtNormalBuffer);
        matNormalPattern.SetPass(0);
        DSRenderer.DrawFullscreenQuad();

        Graphics.SetRenderTarget(dsr.rtNormalBuffer);
        matCopyGBuffer.SetTexture("g_normal_buffer", rtNormalCopy);
        matCopyGBuffer.SetPass(2);
        DSRenderer.DrawFullscreenQuad();

        dsr.SetRenderTargetsGBuffer();
    }
}
