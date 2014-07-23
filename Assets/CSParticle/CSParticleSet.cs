using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class CSParticleSet : MonoBehaviour
{
	public static List<CSParticleSet> instances = new List<CSParticleSet>();

	public static void HandleParticleCollisionAll(CSParticleWorld world)
	{
		foreach (CSParticleSet i in instances)
		{
			i.HandleParticleCollision(world);
		}
	}

	public static void UpdateParticleSetAll(CSParticleWorld world)
	{
		foreach (CSParticleSet i in instances)
		{
			i.UpdateParticleSet(world);
		}
	}

	public static void RenderParticleSetAll(CSParticleWorld world)
	{
		foreach (CSParticleSet i in instances)
		{
			i.RenderParticleSet(world);
		}
	}

	public delegate void ParticleHandler(CSParticle[] particles, List<CSParticleCollider> colliders);

	public int maxParticles = 32768;
	public float lifetime = 30.0f;
	public Material matCSParticle;
	public ParticleHandler handler;

	public CSParticle[] particles;
	public CSWorldData[] csWorldData = new CSWorldData[1];
	List<CSParticle> particlesToAdd = new List<CSParticle>();

	ComputeBuffer cbWorldData;
	ComputeBuffer cbParticles;


	void OnEnable()
	{
		instances.Add(this);
	}

	void OnDisable()
	{
		cbWorldData.Release();
		cbParticles.Release();
		instances.Remove(this);
	}


	void Start()
	{
		csWorldData[0].SetDefaultValues();
		csWorldData[0].num_max_particles = maxParticles;

		particles = new CSParticle[maxParticles];
		for (int i = 0; i < particles.Length; ++i )
		{
			particles[i].hit_objid = -1;
			particles[i].lifetime = 0.0f;
		}

		//Debug.Log("Marshal.SizeOf(typeof(CSParticle))" + Marshal.SizeOf(typeof(CSParticle)));
		//Debug.Log("Marshal.SizeOf(typeof(CSWordData))" + Marshal.SizeOf(typeof(CSWorldData)));
		cbParticles = new ComputeBuffer(maxParticles, 40);
		cbParticles.SetData(particles);
		cbWorldData = new ComputeBuffer(1, 68);
	}

	public void HandleParticleCollision(CSParticleWorld world)
	{
		cbParticles.GetData(particles);
		if (handler != null)
		{
			handler(particles, world.prevColliders);
		}
	}

	void UpdateParticleSet(CSParticleWorld world)
	{
		ComputeShader csParticle = world.csParticle;
		int kernelUpdateVelocity = world.kernelUpdateVelocity;
		int kernelIntegrate = world.kernelIntegrate;

		{
			int pi = csWorldData[0].particle_index;
			for (int i = 0; i < particlesToAdd.Count; ++i )
			{
				if (particles[pi].lifetime <= 0.0f)
				{
					particles[pi] = particlesToAdd[i];
					particles[pi].hit_objid = -1;
					particles[pi].lifetime = lifetime;
				}
				pi = ++pi % maxParticles;
			}
			csWorldData[0].particle_index = pi;
			particlesToAdd.Clear();
			cbParticles.SetData(particles);
		}

		csWorldData[0].particle_lifetime = csWorldData[0].particle_lifetime;
		csWorldData[0].num_sphere_colliders = CSParticleCollider.csSphereColliders.Count;
		csWorldData[0].num_capsule_colliders = CSParticleCollider.csCapsuleColliders.Count;
		csWorldData[0].num_box_colliders = CSParticleCollider.csBoxColliders.Count;
		csWorldData[0].world_center = transform.position;
		csWorldData[0].world_extent = transform.localScale;
		cbWorldData.SetData(csWorldData);

		csParticle.SetBuffer(kernelUpdateVelocity, "world_data", cbWorldData);
		csParticle.SetBuffer(kernelUpdateVelocity, "particles", cbParticles);
		csParticle.SetBuffer(kernelIntegrate, "world_data", cbWorldData);
		csParticle.SetBuffer(kernelIntegrate, "particles", cbParticles);

		csParticle.Dispatch(kernelUpdateVelocity, maxParticles / 1024, 1, 1);
		csParticle.Dispatch(kernelIntegrate, maxParticles / 1024, 1, 1);
	}

	void RenderParticleSet(CSParticleWorld world)
	{
		if (matCSParticle == null)
		{
			matCSParticle = world.matCSParticle;
		}
		matCSParticle.SetBuffer("vertices", world.cbCubeVertices);
		matCSParticle.SetBuffer("particles", cbParticles);
		matCSParticle.SetPass(0);
		Graphics.DrawProcedural(MeshTopology.Triangles, 36, maxParticles);
		//Graphics.DrawProcedural(MeshTopology.Points, 24, maxParticles);
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(transform.position, transform.localScale * 2.0f);
	}


	public void AddParticles(CSParticle[] particles)
	{
		particlesToAdd.AddRange(particles);
	}
}
