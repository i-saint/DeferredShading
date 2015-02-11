using UnityEngine;
using System;
using System.Collections;

public class DSPECaustics : DSEffectBase
{
    public Material m_material;
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


    void Render()
    {
        if (!enabled || m_material==null) { return; }

        DSRenderer dsr = GetDSRenderer();
        Graphics.SetRenderTarget(dsr.rtEmissionBuffer.colorBuffer, dsr.rtNormalBuffer.depthBuffer);
        m_material.SetTexture("g_position_buffer", dsr.rtPositionBuffer);
        m_material.SetTexture("g_normal_buffer", dsr.rtNormalBuffer);
        m_material.SetPass(0);

        DSPECausticsEntity.GetInstances().ForEach((e) => {
            Graphics.DrawMeshNow(e.GetMesh(), e.GetMatrix());
        });
        Graphics.SetRenderTarget(dsr.rtComposite);
    }
}
