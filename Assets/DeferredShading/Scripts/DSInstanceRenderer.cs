using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;



public class DSInstanceRenderer : MonoBehaviour
{
    static DSInstanceRenderer s_instance;

    public static DSInstanceRenderer GetInstance() { return s_instance; }

    public static void AddBatch(Batch b)
    {
        s_instance.GetBatches().Add(b);
    }

    public static void RemoveBatch(Batch r)
    {
        s_instance.GetBatches().Remove(r);
    }


    public class Batch
    {
        public Material material;
        public MeshTopology topology;
        public int num_vertices;
        public int num_instances;
    }

    List<Batch> m_batches;
    Action m_depth_prepass;
    Action m_gbuffer;

    List<Batch> GetBatches()
    {
        if (m_batches == null)
        {
            m_batches = new List<Batch>();
        }
        return m_batches;
    }

    void OnEnable()
    {
        s_instance = this;
        DSRenderer dsr = GetComponent<DSRenderer>();
        if (dsr == null) dsr = GetComponentInParent<DSRenderer>();
        if (m_depth_prepass == null)
        {
            m_depth_prepass = DepthPrePass;
            m_gbuffer = GBufferPass;
            dsr.AddCallbackPreGBuffer(m_depth_prepass);
            dsr.AddCallbackPostGBuffer(m_gbuffer, 10);
        }
    }

    void OnDisable()
    {
        if(s_instance==this) s_instance = null;
    }

    void Update()
    {
    }

    public void DepthPrePass()
    {
        if (!enabled || m_batches==null) return;
        foreach (var i in m_batches)
        {
            i.material.SetPass(0);
            Graphics.DrawProcedural(i.topology, i.num_vertices, i.num_instances);
        }
    }

    public void GBufferPass()
    {
        if (!enabled || m_batches == null) return;
        foreach (var i in m_batches)
        {
            i.material.SetPass(1);
            Graphics.DrawProcedural(i.topology, i.num_vertices, i.num_instances);
        }
    }
}
