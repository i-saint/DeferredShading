using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public abstract class IParticleSetImpl
{
	public abstract void OnEnable();
	public abstract void OnDisable();
	public abstract void Start();
	public abstract void Update();
	public abstract void DepthPrePass();
	public abstract void GBufferPass();
	public abstract void TransparentPass();
	public abstract void HandleParticleCollision();
}


public class ParticleSetImplCS : IParticleSetImpl
{
	CSParticleSet pset;
	CSParticleWorld world;
	CSParticleWorldImplCS wimpl;
	ComputeBuffer cbWorldData;
	ComputeBuffer cbParticles;

	public ParticleSetImplCS(CSParticleSet p)
	{
		pset = p;
	}

	public override void OnEnable()
	{
	}

	public override void OnDisable()
	{
		cbWorldData.Release();
		cbParticles.Release();
	}

	public override void Start()
	{
		world = CSParticleWorld.instance;
		wimpl = world.impl as CSParticleWorldImplCS;

		//Debug.Log("Marshal.SizeOf(typeof(CSParticle))" + Marshal.SizeOf(typeof(CSParticle)));
		//Debug.Log("Marshal.SizeOf(typeof(CSWordData))" + Marshal.SizeOf(typeof(CSWorldData)));
		cbParticles = new ComputeBuffer(pset.maxParticles, 40);
		cbParticles.SetData(pset.particles);
		cbWorldData = new ComputeBuffer(1, 140);
	}

	public override void Update()
	{
		ComputeShader csParticle = world.csParticle;
		int kernelProcessGBufferCollision = wimpl.kernelProcessGBufferCollision;
		int kernelProcessColliders = wimpl.kernelProcessColliders;
		int kernelIntegrate = wimpl.kernelIntegrate;

		{
			int pi = pset.csWorldData[0].particle_index;
			for (int i = 0; i < pset.particlesToAdd.Count; ++i)
			{
				if (pset.particles[pi].lifetime <= 0.0f)
				{
					pset.particles[pi] = pset.particlesToAdd[i];
					pset.particles[pi].hit_objid = -1;
					pset.particles[pi].lifetime = pset.lifetime;
				}
				pi = ++pi % pset.maxParticles;
			}
			pset.csWorldData[0].particle_index = pi;
			pset.particlesToAdd.Clear();
			cbParticles.SetData(pset.particles);
		}

		cbWorldData.SetData(pset.csWorldData);


		if (pset.processGBufferCollision)
		{
			csParticle.SetTexture(kernelProcessGBufferCollision, "gbuffer_normal", world.rtNormalBufferCopy);
			csParticle.SetTexture(kernelProcessGBufferCollision, "gbuffer_position", world.rtPositionBufferCopy);
			csParticle.SetBuffer(kernelProcessGBufferCollision, "world_data", cbWorldData);
			csParticle.SetBuffer(kernelProcessGBufferCollision, "particles", cbParticles);
			csParticle.Dispatch(kernelProcessGBufferCollision, pset.maxParticles / 1024, 1, 1);
		}
		if (pset.processColliders)
		{
			csParticle.SetBuffer(kernelProcessColliders, "world_data", cbWorldData);
			csParticle.SetBuffer(kernelProcessColliders, "particles", cbParticles);
			csParticle.Dispatch(kernelProcessColliders, pset.maxParticles / 1024, 1, 1);
		}
		csParticle.SetBuffer(kernelIntegrate, "world_data", cbWorldData);
		csParticle.SetBuffer(kernelIntegrate, "particles", cbParticles);
		csParticle.Dispatch(kernelIntegrate, pset.maxParticles / 1024, 1, 1);
	}

	public override void DepthPrePass()
	{
		Material matGBuffer = pset.matParticleGBuffer;
		if (pset.processGBufferCollision) { return; }
		if (matGBuffer == null) { matGBuffer = pset.matParticleGBuffer; }

		matGBuffer.SetBuffer("vertices", wimpl.cbCubeVertices);
		matGBuffer.SetBuffer("particles", cbParticles);
		matGBuffer.SetInt("_FlipY", 1);
		matGBuffer.SetPass(0);
		Graphics.DrawProcedural(MeshTopology.Triangles, 36, pset.maxParticles);
	}

	public override void GBufferPass()
	{
		Material matGBuffer = pset.matParticleGBuffer;
		matGBuffer.SetBuffer("vertices", wimpl.cbCubeVertices);
		matGBuffer.SetBuffer("particles", cbParticles);
		matGBuffer.SetInt("_FlipY", 0);
		matGBuffer.SetPass(1);
		Graphics.DrawProcedural(MeshTopology.Triangles, 36, pset.maxParticles);

		//matCSParticle.SetPass(2);
		//Graphics.DrawProcedural(MeshTopology.Triangles, 36, maxParticles);
	}

	public override void TransparentPass()
	{
		Material matTransparent = pset.matParticleTransparent;
		matTransparent.SetBuffer("vertices", wimpl.cbCubeVertices);
		matTransparent.SetBuffer("particles", cbParticles);
		matTransparent.SetPass(0);
		Graphics.DrawProcedural(MeshTopology.Triangles, 36, pset.maxParticles);
	}

	public override void HandleParticleCollision()
	{
		cbParticles.GetData(pset.particles);
		if (pset.handler != null)
		{
			pset.handler(pset.particles, world.prevColliders);
		}
	}
}


public class ParticleSetImplPS : IParticleSetImpl
{
	CSParticleSet pset;
	RenderTexture[] rtParticlePosition = new RenderTexture[2];
	RenderTexture[] rtParticleVelocity = new RenderTexture[2];
	RenderTexture[] rtParticleParams = new RenderTexture[2];

	public ParticleSetImplPS(CSParticleSet p)
	{
		pset = p;
	}

	public override void OnEnable()
	{
	}

	public override void OnDisable()
	{
		for (int i = 0; i < rtParticlePosition.Length; ++i )
		{
			rtParticlePosition[i].Release();
			rtParticleVelocity[i].Release();
			rtParticleParams[i].Release();
		}
	}

	public override void Start()
	{
	}

	public override void Update()
	{
	}

	public override void DepthPrePass()
	{
	}

	public override void GBufferPass()
	{
	}

	public override void TransparentPass()
	{
	}

	public override void HandleParticleCollision()
	{
	}
}





public class CSParticleSet : MonoBehaviour
{
	public static List<CSParticleSet> instances = new List<CSParticleSet>();

	public static void HandleParticleCollisionAll()
	{
		foreach (CSParticleSet i in instances) { i.HandleParticleCollision(); }
	}

	public static void UpdateAll()
	{
		foreach (CSParticleSet i in instances) { i._Update(); }
	}

	public static void DepthPrePassAll()
	{
		foreach (CSParticleSet i in instances) { i.DepthPrePass(); }
	}

	public static void GBufferPassAll()
	{
		foreach (CSParticleSet i in instances) { i.GBufferPass(); }
	}

	public static void TransparentPassAll()
	{
		foreach (CSParticleSet i in instances) { i.TransparentPass(); }
	}


	public delegate void ParticleHandler(CSParticle[] particles, List<CSParticleCollider> colliders);

	public int maxParticles = 32768;
	public bool processGBufferCollision = false;
	public bool processColliders = true;
	public float lifetime = 30.0f;
	public Material matParticleGBuffer;
	public Material matParticleTransparent;
	public ParticleHandler handler;

	public CSParticle[] particles;
	public CSWorldData[] csWorldData = new CSWorldData[1];
	public List<CSParticle> particlesToAdd = new List<CSParticle>();

	IParticleSetImpl impl;


	void OnEnable()
	{
		instances.Add(this);
	}

	void OnDisable()
	{
		impl.OnDisable();
		impl = null;
		instances.Remove(this);
	}


	void Start()
	{
		switch(CSParticleWorld.instance.implMode) {
			case CSParticleWorld.Implementation.ComputeShader: impl = new ParticleSetImplCS(this); break;
			case CSParticleWorld.Implementation.PixelShader: impl = new ParticleSetImplPS(this); break;
		}
		csWorldData[0].SetDefaultValues();
		csWorldData[0].num_max_particles = maxParticles;

		particles = new CSParticle[maxParticles];
		for (int i = 0; i < particles.Length; ++i )
		{
			particles[i].hit_objid = -1;
			particles[i].lifetime = 0.0f;
		}
		impl.Start();
	}

	public void HandleParticleCollision()
	{
		impl.HandleParticleCollision();
	}

	void _Update()
	{
		CSParticleWorld world = CSParticleWorld.instance;
		csWorldData[0].particle_lifetime = csWorldData[0].particle_lifetime;
		csWorldData[0].num_sphere_colliders = CSParticleCollider.csSphereColliders.Count;
		csWorldData[0].num_capsule_colliders = CSParticleCollider.csCapsuleColliders.Count;
		csWorldData[0].num_box_colliders = CSParticleCollider.csBoxColliders.Count;
		csWorldData[0].world_center = transform.position;
		csWorldData[0].world_extent = transform.localScale;
		csWorldData[0].rt_size = world.rt_size;
		csWorldData[0].view_proj = world.viewproj;
		impl.Update();

	}

	void DepthPrePass()
	{
		if (matParticleGBuffer==null || processGBufferCollision) { return; }
		impl.DepthPrePass();
	}

	void GBufferPass()
	{
		if (matParticleGBuffer==null) { return; }
		impl.GBufferPass();
	}

	void TransparentPass()
	{
		if (matParticleTransparent == null) { return; }
		impl.TransparentPass();
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
