using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public struct CSParticle
{
	public Vector3 position;
	public Vector3 velocity;
	public float speed;
	public float lifetime;
	public int owner_objid;
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
	public Vector3 center;
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

public struct IVector3
{
	public int x;
	public int y;
	public int z;
}

public struct UVector3
{
	public uint x;
	public uint y;
	public uint z;
}

public struct CSWorldData
{
	public float timestep;
	public float particle_size;
	public float particle_lifetime;
	public float wall_stiffness;
	public float pressure_stiffness;
	public float decelerate;
	public float gravity;
	public int num_max_particles;
	public int particle_index;
	public int num_sphere_colliders;
	public int num_capsule_colliders;
	public int num_box_colliders;
	public Vector3 world_center;
	public Vector3 world_extents;
	public IVector3 world_div;
	public IVector3 world_div_bits;
	public UVector3 world_div_shift;
	public Vector3 world_cellsize;
	public Vector3 rcp_world_cellsize;
	public Vector2 rt_size;
	public Matrix4x4 view_proj;
	public float rcp_particle_size2;

	public void SetDefaultValues()
	{
		timestep = 0.01f;
		particle_size = 0.1f;
		wall_stiffness = 1500.0f;
		pressure_stiffness = 500.0f;
		decelerate = 0.99f;
		gravity = 7.0f;
		num_max_particles = 0;
		particle_index = 0;
		num_sphere_colliders = 0;
		num_capsule_colliders = 0;
		num_box_colliders = 0;
		rcp_particle_size2 = 1.0f / (particle_size * 2.0f);
	}

	public static uint MSB(uint x)
	{
		for (int i = 31; i >= 0; --i)
		{
			if ((x & (1 << i)) != 0) { return (uint)i; }
		}
		return 0;
	}

	public void SetWorldSize(Vector3 center, Vector3 extents, UVector3 div)
	{
		world_center = center;
		world_extents = extents;
		div.x = MSB(div.x);
		div.y = MSB(div.y);
		div.z = MSB(div.z);
		world_div_bits.x = (int)div.x;
		world_div_bits.y = (int)div.y;
		world_div_bits.z = (int)div.z;
		world_div.x = (int)(1U << (int)div.x);
		world_div.y = (int)(1U << (int)div.y);
		world_div.z = (int)(1U << (int)div.z);
		world_div_shift.x = 1U;
		world_div_shift.y = 1U << (int)(div.x);
		world_div_shift.z = 1U << (int)(div.x + div.y);
		world_cellsize = new Vector3(
			world_extents.x * 2.0f / world_div.x,
			world_extents.y * 2.0f / world_div.y,
			world_extents.z * 2.0f / world_div.z );
		rcp_world_cellsize = new Vector3(
			1.0f / world_cellsize.x,
			1.0f / world_cellsize.y,
			1.0f / world_cellsize.z );
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
		cscol.shape.radius = col.radius * col.transform.localScale.x;
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

		Matrix4x4 mat = col.gameObject.transform.localToWorldMatrix;
		Vector3 size = col.size * 0.5f;

		Vector3[] vertices = new Vector3[8] {
			new Vector3(size.x, size.y, size.z),
			new Vector3(-size.x, size.y, size.z),
			new Vector3(-size.x, -size.y, size.z),
			new Vector3(size.x, -size.y, size.z),
			new Vector3(size.x, size.y, -size.z),
			new Vector3(-size.x, size.y, -size.z),
			new Vector3(-size.x, -size.y, -size.z),
			new Vector3(size.x, -size.y, -size.z),
		};
		for (int i = 0; i < vertices.Length; ++i) {
			vertices[i] = mat * vertices[i];
		}
		Vector3[] normals = new Vector3[6] {
			Vector3.Cross(vertices[3] - vertices[0], vertices[4] - vertices[0]).normalized,
			Vector3.Cross(vertices[5] - vertices[1], vertices[2] - vertices[1]).normalized,
			Vector3.Cross(vertices[7] - vertices[3], vertices[2] - vertices[3]).normalized,
			Vector3.Cross(vertices[1] - vertices[0], vertices[4] - vertices[0]).normalized,
			Vector3.Cross(vertices[1] - vertices[0], vertices[3] - vertices[0]).normalized,
			Vector3.Cross(vertices[7] - vertices[4], vertices[5] - vertices[4]).normalized,
		};
		float[] distances = new float[6] {
			-Vector3.Dot(vertices[0], normals[0]),
			-Vector3.Dot(vertices[1], normals[1]),
			-Vector3.Dot(vertices[0], normals[2]),
			-Vector3.Dot(vertices[3], normals[3]),
			-Vector3.Dot(vertices[0], normals[4]),
			-Vector3.Dot(vertices[4], normals[5]),
		};
		cscol.shape.center = col.gameObject.transform.position;
		cscol.shape.plane0.normal = normals[0];
		cscol.shape.plane0.distance = distances[0];
		cscol.shape.plane1.normal = normals[1];
		cscol.shape.plane1.distance = distances[1];
		cscol.shape.plane2.normal = normals[2];
		cscol.shape.plane2.distance = distances[2];
		cscol.shape.plane3.normal = normals[3];
		cscol.shape.plane3.distance = distances[3];
		cscol.shape.plane4.normal = normals[4];
		cscol.shape.plane4.distance = distances[4];
		cscol.shape.plane5.normal = normals[5];
		cscol.shape.plane5.distance = distances[5];
	}
}

public class ParticleCollider : MonoBehaviour
{
	public static List<ParticleCollider>	instances = new List<ParticleCollider>();
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
				// todo:
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
