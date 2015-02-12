using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GPUParticleEmitter : MonoBehaviour
{
    public static List<GPUParticleEmitter> instances = new List<GPUParticleEmitter>();

    public static void UpdateAll()
    {
        foreach (GPUParticleEmitter f in instances)
        {
            f.ActualUpdate();
        }
    }


    public enum Shape
    {
        Sphere,
        Box,
    }

    public GPUParticleWorld[] m_targets;
    public int m_emit_count = 16;
    public Shape m_shape = Shape.Sphere;
    public Vector3 m_velosity_base = Vector3.zero;
    public float m_velosity_diffuse = 0.5f;
    CSParticle[] m_tmp_to_add;

    void OnEnable()
    {
        instances.Add(this);
    }

    void OnDisable()
    {
        instances.Remove(this);
    }

    void Start()
    {
    }
    

    static float R(float r=0.5f)
    {
        return Random.Range(-r, r);
    }

    void EachTargets(System.Action<GPUParticleWorld> a)
    {
        if (m_targets.Length == 0) { GPUParticleWorld.GetInstances().ForEach(a); }
        else { foreach (var t in m_targets) { a(t); } }
    }

    void ActualUpdate()
    {
        if(m_tmp_to_add==null || m_tmp_to_add.Length!=m_emit_count)
        {
            m_tmp_to_add = new CSParticle[m_emit_count];
        }

        Vector3 pos = transform.position;
        if (m_shape == Shape.Sphere)
        {
            float s = transform.localScale.x;
            for (int i = 0; i < m_tmp_to_add.Length; ++i)
            {
                m_tmp_to_add[i].position = pos + (new Vector3(R(), R(), R())).normalized * R(s * 0.5f);
                m_tmp_to_add[i].velocity = m_velosity_base + new Vector3(R(), R(), R()) * m_velosity_diffuse;
            }
        }
        else if (m_shape == Shape.Sphere)
        {
            Vector3 s = transform.localScale;
            for (int i = 0; i < m_tmp_to_add.Length; ++i)
            {
                m_tmp_to_add[i].position = pos + new Vector3(R(s.x), R(s.y), R(s.z));
                m_tmp_to_add[i].velocity = m_velosity_base + new Vector3(R(), R(), R()) * m_velosity_diffuse;
            }
        }
        EachTargets((t) => { t.AddParticles(m_tmp_to_add); });
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.matrix = transform.localToWorldMatrix;
        switch (m_shape)
        {
            case Shape.Sphere:
                Gizmos.DrawWireSphere(Vector3.zero, 0.5f);
                break;

            case Shape.Box:
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                break;
        }
        Gizmos.matrix = Matrix4x4.identity;
    }
}
