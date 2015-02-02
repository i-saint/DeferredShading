using UnityEngine;
using System;
using System.Collections;

public class DSPEWater : DSEffectBase
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
        if (!enabled) { return; }

        DSRenderer dsr = GetDSRenderer();
        m_material.SetTexture("_FrameBuffer", dsr.rtComposite);
        m_material.SetTexture("_PositionBuffer", dsr.rtPositionBuffer);
        m_material.SetTexture("_PrevPositionBuffer", dsr.rtPrevPositionBuffer);
        m_material.SetTexture("_NormalBuffer", dsr.rtNormalBuffer);
        m_material.SetMatrix("_ViewProjInv", dsr.viewProjInv);
        m_material.SetMatrix("_PrevViewProj", dsr.prevViewProj);
        m_material.SetMatrix("_PrevViewProjInv", dsr.prevViewProjInv);
        m_material.SetPass(0);

        DSPEWaterEntity.GetInstances().ForEach((e) =>
        {
            Graphics.DrawMeshNow(e.GetMesh(), e.GetMatrix());
        });
    }
}
