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

	public static void CSDepthPrePassAll(CSParticleWorld world)
	{
		foreach (CSParticleSet i in instances)
		{
			i.CSDepthPrePass(world);
		}
	}

	public static void CSRenderAll(CSParticleWorld world)
	{
		foreach (CSParticleSet i in instances)
		{
			i.CSRender(world);
		}
	}

	public delegate void ParticleHandler(CSParticle[] particles, List<CSParticleCollider> colliders);

	public int maxParticles = 32768;
	public bool processGBufferCollision = false;
	public bool processColliders = true;
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
		cbWorldData = new ComputeBuffer(1, 140);
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
		int kernelProcessGBufferCollision = world.kernelProcessGBufferCollision;
		int kernelProcessColliders = world.kernelProcessColliders;
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

					//Vector4 p4 = new Vector4(particles[pi].position.x, particles[pi].position.y, particles[pi].position.z, 1.0f);
					//p4 = world.viewproj * p4;
					//Vector2 p2 = new Vector2(p4.x, p4.y) / p4.w;
					//p2.x = (p2.x + 1.0f) * 0.5f;
					//p2.y = (p2.y + 1.0f) * 0.5f;
					//Debug.Log ("" + p2.x + ", " + p2.y);
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
		csWorldData[0].rt_size = world.rt_size;
		csWorldData[0].view_proj = world.viewproj;
		cbWorldData.SetData(csWorldData);


		if(processGBufferCollision) {
			csParticle.SetBuffer(kernelProcessGBufferCollision, "world_data", cbWorldData);
			csParticle.SetBuffer(kernelProcessGBufferCollision, "particles", cbParticles);
			csParticle.Dispatch(kernelProcessGBufferCollision, maxParticles / 1024, 1, 1);
		}
		if (processColliders)
		{
			csParticle.SetBuffer(kernelProcessColliders, "world_data", cbWorldData);
			csParticle.SetBuffer(kernelProcessColliders, "particles", cbParticles);
			csParticle.Dispatch(kernelProcessColliders, maxParticles / 1024, 1, 1);
		}
		csParticle.SetBuffer(kernelIntegrate, "world_data", cbWorldData);
		csParticle.SetBuffer(kernelIntegrate, "particles", cbParticles);
		csParticle.Dispatch(kernelIntegrate, maxParticles / 1024, 1, 1);
	}

	void CSDepthPrePass(CSParticleWorld world)
	{
		if (processGBufferCollision) { return; }
		if (matCSParticle == null) { matCSParticle = world.matCSParticle; }

		matCSParticle.SetBuffer("vertices", world.cbCubeVertices);
		matCSParticle.SetBuffer("particles", cbParticles);
		matCSParticle.SetInt("_FlipY", 1);
		matCSParticle.SetPass(0);
		Graphics.DrawProcedural(MeshTopology.Triangles, 36, maxParticles);
	}

	void CSRender(CSParticleWorld world)
	{
		if (matCSParticle == null) { matCSParticle = world.matCSParticle; }

		matCSParticle.SetBuffer("vertices", world.cbCubeVertices);
		matCSParticle.SetBuffer("particles", cbParticles);
		matCSParticle.SetInt("_FlipY", 0);
		matCSParticle.SetPass(1);
		Graphics.DrawProcedural(MeshTopology.Triangles, 36, maxParticles);

		//matCSParticle.SetPass(2);
		//Graphics.DrawProcedural(MeshTopology.Triangles, 36, maxParticles);
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
