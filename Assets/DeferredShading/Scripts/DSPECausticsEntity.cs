using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class DSPECausticsEntity : MonoBehaviour
{
    static List<DSPECausticsEntity> s_instances;

    public static List<DSPECausticsEntity> GetInstances()
    {
        if (s_instances == null) s_instances = new List<DSPECausticsEntity>();
        return s_instances;
    }


    public Mesh m_mesh;

    public Matrix4x4 GetMatrix() { return GetComponent<Transform>().localToWorldMatrix; }
    public Mesh GetMesh() { return m_mesh; }

    void OnEnable()
    {
        GetInstances().Add(this);
    }

    void OnDisable()
    {
        GetInstances().Remove(this);
    }

    void OnDrawGizmos()
    {
        if (!enabled || GetMesh() == null) return;
        Bounds bounds = GetMesh().bounds;
        Gizmos.matrix = GetMatrix();
        Gizmos.DrawWireCube(bounds.center, bounds.extents);
    }
}
