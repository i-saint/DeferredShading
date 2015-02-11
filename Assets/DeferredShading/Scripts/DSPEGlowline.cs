using UnityEngine;
using System;
using System.Collections;

public class DSPEGlowline : DSEffectBase
{
    public enum SpreadPattern
    {
        Radial = 0,
        Voronoi = 1,
    }
    public enum GridPattern
    {
        Square = 0,
        Hexagon = 1,
        BoxCell = 2,
    }

    public GridPattern gridPattern = GridPattern.BoxCell;
    public SpreadPattern spreadPattern = SpreadPattern.Radial;
    public float intensity = 1.0f;
    public Vector4 baseColor = new Vector4(0.45f, 0.4f, 2.0f, 0.0f);
    public Material matGlowLine;
    RenderBuffer[] rbBuffers;
    Action m_render;

    void OnEnable()
    {
        ResetDSRenderer();
        if (m_render == null)
        {
            m_render = Render;
            GetDSRenderer().AddCallbackPostGBuffer(m_render, 1100);
            rbBuffers = new RenderBuffer[2];
        }
    }

    void Render()
    {
        if (!enabled) { return; }

        DSRenderer dsr = GetDSRenderer();
        rbBuffers[0] = dsr.rtEmissionBuffer.colorBuffer;
        rbBuffers[1] = dsr.rtAlbedoBuffer.colorBuffer;

        Graphics.SetRenderTarget(rbBuffers, dsr.rtNormalBuffer.depthBuffer);
        matGlowLine.SetTexture("g_position_buffer", dsr.rtPositionBuffer);
        matGlowLine.SetTexture("g_normal_buffer", dsr.rtNormalBuffer);
        matGlowLine.SetFloat("g_intensity", intensity);
        matGlowLine.SetVector("_BaseColor", baseColor);
        matGlowLine.SetInt("_GridPattern", (int)gridPattern);
        matGlowLine.SetInt("_SpreadPattern", (int)spreadPattern);
        matGlowLine.SetPass(0);
        DSRenderer.DrawFullscreenQuad();
        Graphics.SetRenderTarget(dsr.rtComposite);
    }
}
