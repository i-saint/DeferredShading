using UnityEngine;
using System;
using System.Collections;

public class DSPECaustics : DSEffectBase
{
    public Material m_material;
    Action m_render;
    RenderBuffer[] m_rbBuffers;


    void OnEnable()
    {
        ResetDSRenderer();
        if (m_render == null)
        {
            m_render = Render;
            //GetDSRenderer().AddCallbackPostEffect(m_render, 5000);
            GetDSRenderer().AddCallbackPostGBuffer(m_render, 1100);
            m_rbBuffers = new RenderBuffer[2];
        }
    }


    void Render()
    {
        if (!enabled || m_material==null) { return; }

        DSRenderer dsr = GetDSRenderer();
        m_rbBuffers[0] = dsr.rtGlowBuffer.colorBuffer;
        m_rbBuffers[1] = dsr.rtColorBuffer.colorBuffer;

        Graphics.SetRenderTarget(m_rbBuffers, dsr.rtNormalBuffer.depthBuffer);
        m_material.SetTexture("_PositionBuffer", dsr.rtPositionBuffer);
        m_material.SetPass(0);

        DSPECausticsEntity.GetInstances().ForEach((e) => {
            Graphics.DrawMeshNow(e.GetMesh(), e.GetMatrix());
        });
    }
}
