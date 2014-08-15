using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParticleEmitter : MonoBehaviour
{
	public static List<ParticleEmitter> instances = new List<ParticleEmitter>();

	public static void UpdateAll()
	{
		foreach (ParticleEmitter f in instances)
		{
			f._Update();
		}
	}


	public enum Shape
	{
		Sphere,
		Box,
	}

	public ParticleSet pset;
	public Shape shape = Shape.Sphere;
	public Vector3 velosityBase = Vector3.zero;
	public float velosityDiffuse = 0.5f;
	public int emitCount = 8;

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

	void _Update()
	{
		if (pset != null)
		{
			Vector3 pos = transform.position;
			CSParticle[] additional = new CSParticle[emitCount];
			if (shape == Shape.Sphere)
			{
				float s = transform.localScale.x;
				for (int i = 0; i < additional.Length; ++i)
				{
					additional[i].position = pos + (new Vector3(R(), R(), R())).normalized * R(s*0.5f);
					additional[i].velocity = velosityBase + new Vector3(R(), R(), R()) * velosityDiffuse;
				}
			}
			else if (shape == Shape.Sphere)
			{
				Vector3 s = transform.localScale;
				for (int i = 0; i < additional.Length; ++i)
				{
					additional[i].position = pos + new Vector3(R(s.x), R(s.y), R(s.z));
					additional[i].velocity = velosityBase + new Vector3(R(), R(), R()) * velosityDiffuse;
				}
			}
			pset.AddParticles(additional);
		}
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.matrix = transform.localToWorldMatrix;
		switch (shape)
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
