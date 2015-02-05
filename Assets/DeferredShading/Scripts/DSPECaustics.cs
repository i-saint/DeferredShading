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
            GetDSRenderer().AddCallbackPostEffect(m_render, 5000);
        }
    }


    void Render()
    {
        if (!enabled || m_material==null) { return; }

        DSRenderer dsr = GetDSRenderer();
        m_material.SetTexture("_PositionBuffer", dsr.rtPositionBuffer);
        m_material.SetPass(0);

        DSPECausticsEntity.GetInstances().ForEach((e) => {
            Graphics.DrawMeshNow(e.GetMesh(), e.GetMatrix());
        });
    }
}
