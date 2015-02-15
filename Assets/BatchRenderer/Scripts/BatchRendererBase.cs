using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;


public abstract class BatchRendererBase : MonoBehaviour
{
    public enum DataTransferMode
    {
        Buffer,
        TextureWithPlugin,
        TextureWithMesh,
    }

    public int m_max_instances = 1024 * 4;
    public Mesh m_mesh;
    public Material m_material;
    public LayerMask m_layer_selector = 1;
    public bool m_cast_shadow = false;
    public bool m_receive_shadow = false;
    public Vector3 m_scale = Vector3.one;
    public Camera m_camera;
    public bool m_flush_on_LateUpdate = true;
    public DataTransferMode m_data_transfer_mode;

    protected int m_instances_par_batch;
    protected int m_instance_count;
    protected int m_batch_count;
    protected int m_layer;
    protected Transform m_trans;
    protected Mesh m_expanded_mesh;
    protected List<Material> m_materials;

    public int GetMaxInstanceCount() { return m_max_instances; }
    public int GetInstanceCount() { return m_instance_count; }
    public void SetInstanceCount(int v) { m_instance_count = v; }



    public virtual Material CloneMaterial(int nth)
    {
        Material m = new Material(m_material);
        m.SetInt("g_batch_begin", nth * m_instances_par_batch);
        return m;
    }

    public virtual void UploadInstanceData_Buffer() { Debug.Log("not implemented"); }
    public virtual void UploadInstanceData_TextureWithMesh() { Debug.Log("not implemented"); }
    public virtual void UploadInstanceData_TextureWithPlugin() { Debug.Log("not implemented"); }

    public virtual void UpdateGPUData()
    {
        switch (m_data_transfer_mode)
        {
            case DataTransferMode.Buffer:
                UploadInstanceData_Buffer();
                break;
            case DataTransferMode.TextureWithMesh:
                UploadInstanceData_TextureWithMesh();
                break;
            case DataTransferMode.TextureWithPlugin:
                UploadInstanceData_TextureWithPlugin();
                break;
        }
        m_materials.ForEach((v) =>
        {
            v.SetInt("g_num_instances", m_instance_count);
            v.SetVector("g_scale", m_scale);
        });
    }


    public virtual void Flush()
    {
        if (m_mesh == null || m_instance_count==0)
        {
            m_instance_count = 0;
            return;
        }

        m_expanded_mesh.bounds = new Bounds(m_trans.position, m_trans.localScale);
        m_instance_count = Mathf.Min(m_instance_count, m_max_instances);
        m_batch_count = BatchRendererUtil.ceildiv(m_max_instances, m_instances_par_batch);

        while (m_materials.Count < m_batch_count)
        {
            Material m = CloneMaterial(m_materials.Count);
            m_materials.Add(m);
        }
        UpdateGPUData();

        Matrix4x4 matrix = Matrix4x4.identity;
        for (int i = 0; i < m_batch_count; ++i)
        {
            Graphics.DrawMesh(m_expanded_mesh, matrix, m_materials[i], m_layer, m_camera, 0, null, m_cast_shadow, m_receive_shadow);
        }
        m_instance_count = m_batch_count = 0;
    }




    public virtual void OnEnable()
    {
        if (m_mesh == null) return;

        m_trans = GetComponent<Transform>();
        m_materials = new List<Material>();

        m_expanded_mesh = BatchRendererUtil.CreateExpandedMesh(m_mesh, out m_instances_par_batch);
        m_expanded_mesh.UploadMeshData(true);

        int layer_mask = m_layer_selector.value;
        for (int i = 0; i < 32; ++i )
        {
            if ((layer_mask & (1<<i)) != 0)
            {
                m_layer = i;
                m_layer_selector.value = 1 << i;
                break;
            }
        }

        if (m_data_transfer_mode == DataTransferMode.Buffer && !SystemInfo.supportsComputeShaders)
        {
            Debug.Log("BatchRenderer: ComputeBuffer is not available. fallback to TextureWithMesh data transfer mode.");
            m_data_transfer_mode = DataTransferMode.TextureWithMesh;
        }
        if (m_data_transfer_mode == DataTransferMode.TextureWithPlugin && !BatchRendererUtil.IsCopyToTextureAvailable())
        {
            Debug.Log("BatchRenderer: CopyToTexture plugin is not available. fallback to TextureWithMesh data transfer mode.");
            m_data_transfer_mode = DataTransferMode.TextureWithMesh;
        }
    }

    public virtual void OnDisable()
    {
    }

    public virtual void LateUpdate()
    {
        if (m_flush_on_LateUpdate)
        {
            Flush();
        }
    }

    public virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
