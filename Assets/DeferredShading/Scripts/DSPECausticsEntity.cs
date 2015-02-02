using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class DSPECausticsEntity : MonoBehaviour
{
    static List<DSPECausticsEntity> s_instances;

    public static List<DSPECausticsEntity> GetInstances()
    {
        if (s_instances == null) s_instances = new List<DSPECausticsEntity>();
        return s_instances;
    }


    public Matrix4x4 GetMatrix() { return GetComponent<Transform>().localToWorldMatrix; }
    public Mesh GetMesh() { return GetComponent<MeshFilter>().sharedMesh; }

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
        Graphics.DrawMeshNow(GetMesh(), GetMatrix());
    }
}
