using System;
using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DSPEReflection : DSEffectBase
{
    public enum Algorithm
    {
        Simple = 0,
        Temporal = 1,
    }

    public Algorithm m_algorithm = Algorithm.Temporal;
    public float m_resolution_scale = 0.5f;
    public float m_intensity = 0.3f;
    public float m_raymarch_distance = 0.2f;
    public float m_ray_diffusion = 0.01f;
    public float m_falloff_distance = 20.0f;
    public float m_max_accumulation = 25.0f;
    public RenderTexture[] m_rt_temp;
    public Material m_mat_reflection;
    public Material m_mate_combine;
    Action m_render;

#if UNITY_EDITOR
    void Reset()
    {
        m_mat_reflection = AssetDatabase.LoadAssetAtPath("Assets/DeferredShading/Materials/PostEffect_Reflection.mat", typeof(Material)) as Material;
        m_mate_combine = AssetDatabase.LoadAssetAtPath("Assets/DeferredShading/Materials/Combine.mat", typeof(Material)) as Material;
    }
#endif

    void OnEnable()
    {
        ResetDSRenderer();
        if (m_render == null)
        {
            m_render = Render;
            GetDSRenderer().AddCallbackPostEffect(m_render, 5000);
            m_rt_temp = new RenderTexture[2];
        }
    }


    void UpdateRenderTargets()
    {
        Vector2 reso = GetDSRenderer().GetInternalResolution() * m_resolution_scale;
        if (m_rt_temp[0] != null && m_rt_temp[0].width != (int)reso.x)
        {
            Debug.Log("!? " + m_rt_temp[0].width + ", " + reso.x);
            for (int i = 0; i < m_rt_temp.Length; ++i)
            {
                m_rt_temp[i].Release();
                m_rt_temp[i] = null;
            }
        }
        if (m_rt_temp[0] == null || !m_rt_temp[0].IsCreated())
        {
            for (int i = 0; i < m_rt_temp.Length; ++i)
            {
                m_rt_temp[i] = DSRenderer.CreateRenderTexture((int)reso.x, (int)reso.y, 0, RenderTextureFormat.ARGBHalf);
                m_rt_temp[i].filterMode = FilterMode.Point;
                Graphics.SetRenderTarget(m_rt_temp[i]);
                GL.Clear(false,true, Color.black);
            }
        }
    }

    void Render()
    {
        if (!enabled) { return; }
        UpdateRenderTargets();

        DSRenderer dsr = GetDSRenderer();
        Graphics.SetRenderTarget(m_rt_temp[0]);
        //GL.Clear(false, true, Color.black);
        m_mat_reflection.SetFloat("g_intensity", m_intensity);
        m_mat_reflection.SetFloat("_RayMarchDistance", m_raymarch_distance);
        m_mat_reflection.SetFloat("_RayDiffusion", m_ray_diffusion);
        m_mat_reflection.SetFloat("_FalloffDistance", m_falloff_distance);
        m_mat_reflection.SetFloat("_MaxAccumulation", m_max_accumulation);

        m_mat_reflection.SetTexture("g_frame_buffer", dsr.rtComposite);
        m_mat_reflection.SetTexture("g_position_buffer", dsr.rtPositionBuffer);
        m_mat_reflection.SetTexture("g_prev_position_buffer", dsr.rtPrevPositionBuffer);
        m_mat_reflection.SetTexture("g_normal_buffer", dsr.rtNormalBuffer);
        m_mat_reflection.SetTexture("_PrevResult", m_rt_temp[1]);
        m_mat_reflection.SetMatrix("_ViewProjInv", dsr.viewProjInv);
        m_mat_reflection.SetMatrix("_PrevViewProj", dsr.prevViewProj);
        m_mat_reflection.SetMatrix("_PrevViewProjInv", dsr.prevViewProjInv);
        m_mat_reflection.SetPass((int)m_algorithm);
        DSRenderer.DrawFullscreenQuad();

        m_rt_temp[0].filterMode = FilterMode.Trilinear;
        Graphics.SetRenderTarget(dsr.rtComposite);
        m_mate_combine.SetTexture("_MainTex", m_rt_temp[0]);
        m_mate_combine.SetPass(2);
        DSRenderer.DrawFullscreenQuad();
        m_rt_temp[0].filterMode = FilterMode.Point;

        DSRenderer.Swap(ref m_rt_temp[0], ref m_rt_temp[1]);
    }
}
