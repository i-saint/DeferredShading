using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;


public class CustumDataBatchRenderer<InstanceData> : BatchRendererBase
    where InstanceData : struct
{

    public void AddInstance(ref InstanceData v)
    {
        int i = Interlocked.Increment(ref m_instance_count) - 1;
        if (i < m_max_instances)
        {
            m_instance_data[i] = v;
        }
    }

    public InstanceData[] ReserveInstance(int num, out int reserved_index, out int reserved_num)
    {
        reserved_index = Interlocked.Add(ref m_instance_count, num) - num;
        reserved_num = Mathf.Clamp(m_max_instances - reserved_index, 0, num);
        return m_instance_data;
    }


    [System.Serializable]
    public struct DrawData
    {
        public const int size = 20;

        public int num_max_instances;
        public int num_instances;
        public Vector3 scale;
    }

    [System.Serializable]
    public struct BatchData
    {
        public const int size = 8;

        public int begin;
        public int end;
    }


    public int m_sizeof_instance_data;

    protected DrawData[] m_draw_data = new DrawData[1];
    protected InstanceData[] m_instance_data;
    protected ComputeBuffer m_instance_buffer;
    protected RenderTexture m_instance_texture;

    public void SetInstanceDataSize(int v) { m_sizeof_instance_data = v; }
    public ComputeBuffer GetInstanceBuffer() { return m_instance_buffer; }
    public RenderTexture GetInstanceTexture() { return m_instance_texture; }

    public override Material CloneMaterial(int nth)
    {
        Material m = new Material(m_material);
        m.SetInt("g_batch_begin", nth * m_instances_par_batch);
        if (m_instance_buffer != null) { m.SetBuffer("g_instance_buffer", m_instance_buffer); }
        if (m_instance_texture != null) { m.SetTexture("g_instance_texture", m_instance_texture); }

        // fix rendering order for transparent objects
        if (m.renderQueue >= 3000)
        {
            m.renderQueue = m.renderQueue + (nth + 1);
        }
        return m;
    }


    public virtual void ReleaseGPUData()
    {
        if (m_instance_buffer!=null)
        {
            m_instance_buffer.Release();
            m_instance_buffer = null;
        }
        if (m_instance_texture!=null)
        {
            m_instance_buffer.Release();
            m_instance_buffer = null;
        }
        m_materials.Clear();
    }

    public virtual void ResetGPUData()
    {
        ReleaseGPUData();

        m_instance_data = new InstanceData[m_max_instances];
        m_instance_buffer = new ComputeBuffer(m_max_instances, m_sizeof_instance_data);

        UpdateGPUData();
    }

    public override void UploadInstanceData_Buffer()
    {
        m_instance_buffer.SetData(m_instance_data);
    }

    public override void UploadInstanceData_TextureWithMesh()
    {
    }

    public override void UploadInstanceData_TextureWithPlugin()
    {
    }



    public override void OnEnable()
    {
        base.OnEnable();
        if (m_mesh == null) return;

        ReleaseGPUData();
        ResetGPUData();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        ReleaseGPUData();
    }
}
