using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GPUParticleCollider : MonoBehaviour
{
    static List<GPUParticleCollider> s_instances;
    static List<CSSphereCollider> s_sphere_colliders;
    static List<CSCapsuleCollider> s_capsule_colliders;
    static List<CSBoxCollider> s_box_colliders;

    public static List<GPUParticleCollider> GetInstances()
    {
        if (s_instances == null) s_instances = new List<GPUParticleCollider>();
        return s_instances;
    }

    public static List<CSSphereCollider> GetSphereColliderData()
    {
        if (s_sphere_colliders == null) s_sphere_colliders = new List<CSSphereCollider>();
        return s_sphere_colliders;
    }

    public static List<CSCapsuleCollider> GetCapsuleColliderData()
    {
        if (s_capsule_colliders == null) s_capsule_colliders = new List<CSCapsuleCollider>();
        return s_capsule_colliders;
    }

    public static List<CSBoxCollider> GetBoxColliderData()
    {
        if (s_box_colliders == null) s_box_colliders = new List<CSBoxCollider>();
        return s_box_colliders;
    }


    public static void UpdateAll()
    {
        GetSphereColliderData().Clear();
        GetCapsuleColliderData().Clear();
        GetBoxColliderData().Clear();

        for (int i = 0; i < s_instances.Count; ++i )
        {
            if (s_instances[i].m_col3d != null)
            {
                if (!s_instances[i].m_send_collision) { continue; }

                Collider col = s_instances[i].m_col3d;
                SphereCollider sphere = col as SphereCollider;
                CapsuleCollider capsule = col as CapsuleCollider;
                BoxCollider box = col as BoxCollider;
                if (sphere)
                {
                    CSSphereCollider cscol = new CSSphereCollider();
                    CSImpl.BuildSphereCollider(ref cscol, sphere, i);
                    GetSphereColliderData().Add(cscol);
                }
                else if (capsule)
                {
                    CSCapsuleCollider cscol = new CSCapsuleCollider();
                    CSImpl.BuildCapsuleCollider(ref cscol, capsule, i);
                    GetCapsuleColliderData().Add(cscol);
                }
                else if (box)
                {
                    CSBoxCollider cscol = new CSBoxCollider();
                    CSImpl.BuildBoxCollider(ref cscol, box, i);
                    GetBoxColliderData().Add(cscol);
                }
            }
            if (s_instances[i].m_col2d != null)
            {
                // todo:
            }
        }
    }


    public bool m_send_collision = true;
    public bool m_receive_collision = false;
    Collider m_col3d;
    Collider2D m_col2d;

    void OnEnable()
    {
        GetInstances().Add(this);
        m_col3d = GetComponent<Collider>();
        m_col2d = GetComponent<Collider2D>();
    }

    void OnDisable()
    {
        GetInstances().Remove(this);
    }
}
