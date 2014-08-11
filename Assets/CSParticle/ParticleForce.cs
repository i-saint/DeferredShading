using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CSForceShape
{
	All,
	Sphere,
	Capsule,
	Box
}

public enum CSForceDirection
{
	Directional,
	Radial,
	VectorField,
}

public struct CSForceInfo
{
	public CSForceShape shape_type;
	public CSForceDirection dir_type;
	public float strength;
	public float random_seed;
	public float random_diffuse;
	public Vector3 direction;
	public Vector3 center;
	public Vector3 rcp_cellsize;
}

public struct CSForce
{
	public const int size = 208;

	public CSForceInfo info;
	public CSSphere sphere;
	public CSCapsule capsule;
	public CSBox box;
}


public class ParticleForce : MonoBehaviour
{
	public static List<ParticleForce> instances = new List<ParticleForce>();
	public static List<CSForce> forceData = new List<CSForce>();

	public static void UpdateAll()
	{
		foreach(ParticleForce f in instances) {
			f._Update();
			forceData.Add(f.force);
		}
	}

	public static void AddForce(ref CSForce f)
	{
		forceData.Add(f);
	}


	public CSForceShape shapeType;
	public CSForceDirection directionType;
	public float strengthNear = 10.0f;
	public float strengthFar = 0.0f;
	public float rangeInner = 0.0f;
	public float rangeOuter = 100.0f;
	public float attenuationExp = 0.5f;
	public float randomDiffuse = 0.0f;
	public float randomSeed = 1.0f;
	public Vector3 direction = new Vector3(0.0f, -1.0f, 0.0f);
	public Vector3 VF_cellsize = new Vector3(0.5f, 0.5f, 0.5f);
	public CSForce force;

	
	void OnEnable()
	{
		instances.Add(this);
	}

	void OnDisable()
	{
		instances.Remove(this);
	}

	void _Update()
	{
		force.info.shape_type = shapeType;
		force.info.dir_type = directionType;
		force.info.strength = strengthNear;
		force.info.random_diffuse = randomDiffuse;
		force.info.random_seed = randomSeed;
		force.info.direction = direction;
		force.info.center = transform.position;
		force.info.rcp_cellsize = new Vector3(1.0f / VF_cellsize.x, 1.0f / VF_cellsize.y, 1.0f / VF_cellsize.z);
		if (shapeType == CSForceShape.Sphere)
		{
			force.sphere.center = transform.position;
			force.sphere.radius = transform.localScale.x;
		}
		else if (shapeType == CSForceShape.Box)
		{
			CSImpl.BuildBox(ref force.box, transform.localToWorldMatrix, Vector3.one);
		}
	}

	void OnDrawGizmos()
	{
		{
			float arrowHeadAngle = 30.0f;
			float arrowHeadLength = 0.5f;
			Vector3 pos = transform.position;
			Vector3 dir = direction * strengthNear * 0.5f;

			Gizmos.matrix = Matrix4x4.identity;
			Gizmos.color = Color.yellow;
			Gizmos.DrawRay(pos, dir);

			Vector3 right = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
			Vector3 left = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
			Gizmos.DrawRay(pos + dir, right * arrowHeadLength);
			Gizmos.DrawRay(pos + dir, left * arrowHeadLength);
		}
		{
			Gizmos.color = Color.yellow;
			Gizmos.matrix = transform.localToWorldMatrix;
			switch (shapeType)
			{
				case CSForceShape.Sphere:
					Gizmos.DrawWireSphere(Vector3.zero, 0.5f);
					break;

				case CSForceShape.Box:
					Gizmos.color = Color.yellow;
					Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
					break;
			}
			Gizmos.matrix = Matrix4x4.identity;
		}
	}
}
