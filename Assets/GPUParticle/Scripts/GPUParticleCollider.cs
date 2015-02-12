using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GPUParticleColliderBase : MonoBehaviour
{
    static List<GPUParticleColliderBase> s_instances;

    public static List<GPUParticleColliderBase> GetInstances()
    {
        if (s_instances == null) s_instances = new List<GPUParticleColliderBase>();
        return s_instances;
    }

    public static void UpdateAll()
    {
        int i = 0;
        GetInstances().ForEach((v) => {
            v.m_id = i++;
            v.ActualUpdate();
        });
    }


    public GPUParticleWorld[] m_targets;
    public bool m_send_collision = true;
    public bool m_receive_collision = false;
    protected int m_id;
    protected Transform m_trans;

    protected void EachTargets(System.Action<GPUParticleWorld> a)
    {
        if (m_targets.Length == 0) { GPUParticleWorld.GetInstances().ForEach(a); }
        else { foreach (var t in m_targets) { a(t); } }
    }

    void OnEnable()
    {
        GetInstances().Add(this);
        m_trans = GetComponent<Transform>();
    }

    void OnDisable()
    {
        GetInstances().Remove(this);
    }

    public virtual void ActualUpdate() { }
}

