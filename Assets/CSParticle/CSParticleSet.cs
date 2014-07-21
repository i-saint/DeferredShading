using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class CSParticleSet : MonoBehaviour
{
	public static List<CSParticleSet> instances = new List<CSParticleSet>();

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


	public int maxParticles = 32768;

	public CSParticle[] particles;
	public CSWorldData[] csWorldData = new CSWorldData[1];

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
		{
			const float posMin = -2.0f;
			const float posMax = 2.0f;
			const float velMin = -1.0f;
			const float velMax = 1.0f;
			for (int i = 0; i < particles.Length; ++i)
			{
				particles[i].position = new Vector3(Random.Range(posMin, posMax), Random.Range(posMin, posMax) + 3.0f, Random.Range(posMin, posMax));
				particles[i].velocity = new Vector3(Random.Range(velMin, velMax), Random.Range(velMin, velMax), Random.Range(velMin, velMax));
				particles[i].owner_objid = 0;
				//particles[i].owner_objid = -1;
			}
		}
		cbParticles = new ComputeBuffer(maxParticles, Marshal.SizeOf(typeof(CSParticle)));
		cbParticles.SetData(particles);

		cbWorldData = new ComputeBuffer(1, Marshal.SizeOf(typeof(CSWorldData)));
	}

	void UpdateParticleSet(CSParticleWorld world)
	{
		ComputeShader csParticle = world.csParticle;
		int kernelUpdateVelocity = world.kernelUpdateVelocity;
		int kernelIntegrate = world.kernelIntegrate;

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
		Material matCSParticle = world.matCSParticle;
		matCSParticle.SetBuffer("particles", cbParticles);
		matCSParticle.SetPass(0);
		Graphics.DrawProcedural(MeshTopology.Triangles, 36, maxParticles);
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(transform.position, transform.localScale * 2.0f);
	}
}
