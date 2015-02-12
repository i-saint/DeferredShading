using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif



[RequireComponent(typeof(GPUParticleWorld))]
public class GPUTrail : MonoBehaviour
{
    public static List<GPUTrail> s_instances = new List<GPUTrail>();

    public int m_trail_max_history = 32;
    public ComputeShader m_cs_trail;
    public Material m_mat_trail;

    ComputeBuffer m_buf_trail_params;
    ComputeBuffer m_buf_trail_entities;
    ComputeBuffer m_buf_trail_history;
    ComputeBuffer m_buf_trail_vertices;

    GPUParticleWorld m_pset;
    CSTrailParams[] m_tmp_params;

    const int BLOCK_SIZE = 512;

#if UNITY_EDITOR
    void Reset()
    {
        m_cs_trail = AssetDatabase.LoadAssetAtPath("Assets/GPUParticle/Shaders/Trail.compute", typeof(ComputeShader)) as ComputeShader;
    }
#endif // UNITY_EDITOR


    void OnEnable()
    {
        s_instances.Add(this);
        m_pset = GetComponent<GPUParticleWorld>();
        m_tmp_params = new CSTrailParams[1];

        m_buf_trail_params = new ComputeBuffer(1, CSTrailParams.size);
        m_buf_trail_entities = new ComputeBuffer(m_pset.m_max_particles, CSTrailEntity.size);
        m_buf_trail_history = new ComputeBuffer(m_pset.m_max_particles * m_trail_max_history, CSTrailHistory.size);
        m_buf_trail_vertices = new ComputeBuffer(m_pset.m_max_particles * m_trail_max_history * 2, CSTrailVertex.size);

        DispatchTrailKernel(0);
    }

    void OnDisable()
    {
        s_instances.Remove(this);

        m_buf_trail_vertices.Release();
        m_buf_trail_history.Release();
        m_buf_trail_entities.Release();
        m_buf_trail_params.Release();
    }


    public void ActualUpdate()
    {
        DispatchTrailKernel(1);

    }

    void DispatchTrailKernel(int i)
    {
        if (!enabled || Time.deltaTime == 0.0f) return;

        m_tmp_params[0].delta_time = Time.deltaTime;
        m_tmp_params[0].max_entities = m_pset.m_max_particles;
        m_tmp_params[0].max_history = m_trail_max_history;
        m_tmp_params[0].camera_position = Camera.current != null ? Camera.current.transform.position : Vector3.zero;
        m_tmp_params[0].width = 0.2f;
        m_buf_trail_params.SetData(m_tmp_params);

        m_cs_trail.SetBuffer(i, "particles", m_pset.GetParticleBuffer());
        m_cs_trail.SetBuffer(i, "params", m_buf_trail_params);
        m_cs_trail.SetBuffer(i, "entities", m_buf_trail_entities);
        m_cs_trail.SetBuffer(i, "history", m_buf_trail_history);
        m_cs_trail.SetBuffer(i, "vertices", m_buf_trail_vertices);
        m_cs_trail.Dispatch(i, m_pset.m_max_particles/BLOCK_SIZE, 1, 1);
    }

    void Render()
    {
        if (!enabled || m_mat_trail==null) return;

        m_mat_trail.SetBuffer("particles", m_pset.GetParticleBuffer());
        m_mat_trail.SetBuffer("params", m_buf_trail_params);
        m_mat_trail.SetBuffer("vertices", m_buf_trail_vertices);
        m_mat_trail.SetPass(0);
        Graphics.DrawProcedural(MeshTopology.Triangles, (m_trail_max_history - 1) * 6, m_pset.GetNumMaxParticles());
    }
}
