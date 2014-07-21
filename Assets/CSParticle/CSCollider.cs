using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public struct CSParticle
{
	public Vector3 position;
	public Vector3 velocity;
	public float speed;
	public int owner_objid; // 0: invalid & dead
	public int hit_objid;
};

public struct CSAABB
{
	public Vector3 center;
	public Vector3 extents;
}

public struct CSColliderInfo
{
	public int owner_objid;
	public CSAABB aabb;
}

public struct CSSphere
{
	public Vector3 center;
	public float radius;
}

public struct CSCapsule
{
	public Vector3 pos1;
	public Vector3 pos2;
	public float radius;
}

public struct CSPlane
{
	public Vector3 normal;
	public float distance;
}

public struct CSBox
{
	public CSPlane plane0;
	public CSPlane plane1;
	public CSPlane plane2;
	public CSPlane plane3;
	public CSPlane plane4;
	public CSPlane plane5;
}


public struct CSSphereCollider
{
	public CSColliderInfo info;
	public CSSphere shape;
}

public struct CSCapsuleCollider
{
	public CSColliderInfo info;
	public CSCapsule shape;
}

public struct CSBoxCollider
{
	public CSColliderInfo info;
	public CSBox shape;
}


public struct CSWorldData
{
	public float timestep;
	public float particle_size;
	public float wall_stiffness;
	public float decelerate;
	public float gravity;
	public int num_max_particles;
	public int num_particles;
	public int num_sphere_colliders;
	public int num_capsule_colliders;
	public int num_box_colliders;
	public Vector3 world_center;
	public Vector3 world_extent;

	public void SetDefaultValues()
	{
		timestep = 0.01f;
		particle_size = 0.0f;
		wall_stiffness = 100.0f;
		decelerate = 0.995f;
		gravity = 7.0f;
		num_max_particles = TestCSParticle.MAX_PARTICLES;
		num_particles = 0;
		num_sphere_colliders = 0;
		num_capsule_colliders = 0;
		num_box_colliders = 0;
	}
};


public class CSImpl
{
	static void ConstructColliderInfo<T>(ref CSColliderInfo info, T col, int id) where T : Collider
	{
		info.owner_objid = id;
		info.aabb.center = col.bounds.center;
		info.aabb.extents = col.bounds.extents;
	}

	static public void ConstructSphereCollider(ref CSSphereCollider cscol, SphereCollider col, int id)
	{
		ConstructColliderInfo(ref cscol.info, col, id);
		cscol.shape.center = col.gameObject.transform.position;
		cscol.shape.radius = col.radius * col.transform.localScale.magnitude * 0.5f;
	}

	static public void ConstructCapsuleCollider(ref CSCapsuleCollider cscol, CapsuleCollider col, int id)
	{
		ConstructColliderInfo(ref cscol.info, col, id);
		Vector3 e = Vector3.zero;
		float h = Mathf.Max(0.0f, col.height - col.radius * 2.0f);
		float r = col.radius * col.transform.localScale.x;
		switch (col.direction)
		{
			case 0: e.Set(h * 0.5f, 0.0f, 0.0f); break;
			case 1: e.Set(0.0f, h * 0.5f, 0.0f); break;
			case 2: e.Set(0.0f, 0.0f, h * 0.5f); break;
		}
		Vector4 pos1 = new Vector4(e.x, e.y, e.z, 1.0f);
		Vector4 pos2 = new Vector4(-e.x, -e.y, -e.z, 1.0f);
		pos1 = col.transform.localToWorldMatrix * pos1;
		pos2 = col.transform.localToWorldMatrix * pos2;
		cscol.shape.radius = r;
		cscol.shape.pos1 = pos1;
		cscol.shape.pos2 = pos2;
	}

	static public void ConstructBoxCollider(ref CSBoxCollider cscol, BoxCollider col, int id)
	{
		ConstructColliderInfo(ref cscol.info, col, id);
	}
}

public class CSCollider : MonoBehaviour
{
	public static List<CSCollider>			instances = new List<CSCollider>();
	public static List<CSSphereCollider>	csSphereColliders = new List<CSSphereCollider>();
	public static List<CSCapsuleCollider>	csCapsuleColliders = new List<CSCapsuleCollider>();
	public static List<CSBoxCollider>		csBoxColliders = new List<CSBoxCollider>();

	public static void UpdateCSColliders()
	{
		csSphereColliders.Clear();
		csCapsuleColliders.Clear();
		csBoxColliders.Clear();

		for (int i = 0; i < instances.Count; ++i )
		{
			if (instances[i].col3d != null)
			{
				if (!instances[i].sendCollision) { continue; }

				Collider col = instances[i].col3d;
				SphereCollider sphere = col as SphereCollider;
				CapsuleCollider capsule = col as CapsuleCollider;
				BoxCollider box = col as BoxCollider;
				if (sphere)
				{
					CSSphereCollider cscol = new CSSphereCollider();
					CSImpl.ConstructSphereCollider(ref cscol, sphere, i);
					csSphereColliders.Add(cscol);
				}
				else if (capsule)
				{
					CSCapsuleCollider cscol = new CSCapsuleCollider();
					CSImpl.ConstructCapsuleCollider(ref cscol, capsule, i);
					csCapsuleColliders.Add(cscol);
				}
				else if (box)
				{
					CSBoxCollider cscol = new CSBoxCollider();
					CSImpl.ConstructBoxCollider(ref cscol, box, i);
					csBoxColliders.Add(cscol);
				}
			}
			if (instances[i].col2d != null)
			{

			}
		}
	}


	public bool sendCollision = true;
	public bool receiveCollision = false;
	Collider col3d;
	Collider2D col2d;

	void OnEnable()
	{
		instances.Add(this);
		col3d = GetComponent<Collider>();
		col2d = GetComponent<Collider2D>();
	}

	void OnDisable()
	{
		instances.Remove(this);
	}
}
