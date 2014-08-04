using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class CubesDrawer
{
	static void Reserve(int num)
	{

	}

	static void DrawCubes(int num, RenderTexture particleData)
	{

	}
}



public abstract class IMPParticleSetImpl
{
	public abstract void OnEnable();
	public abstract void OnDisable();
	public abstract void Start();
	public abstract void Update();
	public abstract void DepthPrePass();
	public abstract void GBufferPass();
	public abstract void TransparentPass();
	public abstract void HandleParticleCollision();

	public abstract void AddParticles(CSParticle[] particles);
}


public class MPParticleSetImplCPU : IMPParticleSetImpl
{
	ParticleSet pset;
	ParticleWorld world;
	MPParticleWorldImplCPU wimpl;

	public MPParticleSetImplCPU(ParticleSet p)
	{
		pset = p;
	}


	public override void OnEnable()
	{
	}

	public override void OnDisable()
	{
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

	public override void AddParticles(CSParticle[] particles)
	{ }
}

public class MPParticleSetImplGPU : IMPParticleSetImpl
{
	ParticleSet pset;
	ParticleWorld world;
	MPParticleWorldImplGPU wimpl;
	List<CSParticle> particlesToAdd = new List<CSParticle>();
	ComputeBuffer cbWorldData;
	ComputeBuffer cbParticles;
	ComputeBuffer cbPIntermediate;
	ComputeBuffer cbCells;

	public MPParticleSetImplGPU(ParticleSet p)
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
		cbPIntermediate.Release();
	}

	public override void Start()
	{
		world = ParticleWorld.instance;
		wimpl = world.impl as MPParticleWorldImplGPU;

		//Debug.Log("Marshal.SizeOf(typeof(CSParticle))" + Marshal.SizeOf(typeof(CSParticle)));
		//Debug.Log("Marshal.SizeOf(typeof(CSWordData))" + Marshal.SizeOf(typeof(CSWorldData)));
		cbParticles = new ComputeBuffer(pset.maxParticles, 40);
		cbParticles.SetData(pset.particles);
		cbPIntermediate = new ComputeBuffer(pset.maxParticles, 12);
		cbWorldData = new ComputeBuffer(1, 188);
	}

	public override void Update()
	{
		ComputeShader csParticle = world.csParticle;

		{
			int pi = pset.csWorldData[0].particle_index;
			for (int i = 0; i < particlesToAdd.Count; ++i)
			{
				if (pset.particles[pi].lifetime <= 0.0f)
				{
					pset.particles[pi] = particlesToAdd[i];
					pset.particles[pi].hit_objid = -1;
					pset.particles[pi].lifetime = pset.lifetime;
				}
				pi = ++pi % pset.maxParticles;
			}
			pset.csWorldData[0].particle_index = pi;
			particlesToAdd.Clear();
			cbParticles.SetData(pset.particles);
		}

		cbWorldData.SetData(pset.csWorldData);

		const int BLOCK_SIZE = 512;
		{
			int kernel = wimpl.kPrepare;
			csParticle.SetBuffer(kernel, "world_data", cbWorldData);
			csParticle.SetBuffer(kernel, "particles", cbParticles);
			csParticle.SetBuffer(kernel, "pidata", cbPIntermediate);
			csParticle.Dispatch(kernel, pset.maxParticles / BLOCK_SIZE, 1, 1);
		}
		if (pset.interactionMode == ParticleSet.Interaction.Impulse)
		{
			int kernel = wimpl.kProcessInteraction_Impulse;
			csParticle.SetBuffer(kernel, "world_data", cbWorldData);
			csParticle.SetBuffer(kernel, "particles", cbParticles);
			csParticle.SetBuffer(kernel, "pidata", cbPIntermediate);
			csParticle.Dispatch(kernel, pset.maxParticles / BLOCK_SIZE, 1, 1);
		}
		else if (pset.interactionMode == ParticleSet.Interaction.SPH)
		{
			int kernel = wimpl.kProcessInteraction_SPH;
			csParticle.SetBuffer(kernel, "world_data", cbWorldData);
			csParticle.SetBuffer(kernel, "particles", cbParticles);
			csParticle.SetBuffer(kernel, "pidata", cbPIntermediate);
			csParticle.Dispatch(kernel, pset.maxParticles / BLOCK_SIZE, 1, 1);
		}

		if (pset.processGBufferCollision)
		{
			int kernel = wimpl.kProcessGBufferCollision;
			csParticle.SetTexture(kernel, "gbuffer_normal", world.rtNormalBufferCopy);
			csParticle.SetTexture(kernel, "gbuffer_position", world.rtPositionBufferCopy);
			csParticle.SetBuffer(kernel, "world_data", cbWorldData);
			csParticle.SetBuffer(kernel, "particles", cbParticles);
			csParticle.SetBuffer(kernel, "pidata", cbPIntermediate);
			csParticle.Dispatch(kernel, pset.maxParticles / BLOCK_SIZE, 1, 1);
		}
		if (pset.processColliders)
		{
			int kernel = wimpl.kProcessColliders;
			csParticle.SetBuffer(kernel, "world_data", cbWorldData);
			csParticle.SetBuffer(kernel, "particles", cbParticles);
			csParticle.SetBuffer(kernel, "pidata", cbPIntermediate);
			csParticle.Dispatch(kernel, pset.maxParticles / BLOCK_SIZE, 1, 1);
		}
		{
			int kernel = wimpl.kProcessForces;
			csParticle.SetBuffer(kernel, "world_data", cbWorldData);
			csParticle.SetBuffer(kernel, "particles", cbParticles);
			csParticle.SetBuffer(kernel, "pidata", cbPIntermediate);
			csParticle.Dispatch(kernel, pset.maxParticles / BLOCK_SIZE, 1, 1);
		}
		{
			int kernel = wimpl.kIntegrate;
			csParticle.SetBuffer(kernel, "world_data", cbWorldData);
			csParticle.SetBuffer(kernel, "particles", cbParticles);
			csParticle.SetBuffer(kernel, "pidata", cbPIntermediate);
			csParticle.Dispatch(kernel, pset.maxParticles / BLOCK_SIZE, 1, 1);
		}
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

	public override void AddParticles(CSParticle[] particles)
	{
		particlesToAdd.AddRange(particles);
	}
}





public class ParticleSet : MonoBehaviour
{
	public static List<ParticleSet> instances = new List<ParticleSet>();

	public static void HandleParticleCollisionAll()
	{
		foreach (ParticleSet i in instances) { i.HandleParticleCollision(); }
	}

	public static void UpdateAll()
	{
		foreach (ParticleSet i in instances) { i._Update(); }
	}

	public static void DepthPrePassAll()
	{
		foreach (ParticleSet i in instances) { i.DepthPrePass(); }
	}

	public static void GBufferPassAll()
	{
		foreach (ParticleSet i in instances) { i.GBufferPass(); }
	}

	public static void TransparentPassAll()
	{
		foreach (ParticleSet i in instances) { i.TransparentPass(); }
	}


	public delegate void ParticleHandler(CSParticle[] particles, List<ParticleCollider> colliders);

	public enum Interaction {
		Impulse,
		SPH,
		None,
	}

	public ParticleWorld.Implementation implMode;
	public Interaction interactionMode = Interaction.Impulse;
	public int maxParticles = 32768;
	public uint worldDivX = 256;
	public uint worldDivY = 1;
	public uint worldDivZ = 256;
	public bool processGBufferCollision = false;
	public bool processColliders = true;
	public float lifetime = 30.0f;
	public Material matParticleGBuffer;
	public Material matParticleTransparent;
	public ParticleHandler handler;

	public CSParticle[] particles;
	public CSWorldData[] csWorldData = new CSWorldData[1];

	IMPParticleSetImpl impl;


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
		switch(ParticleWorld.instance.implMode) {
			case ParticleWorld.Implementation.GPU: impl = new MPParticleSetImplGPU(this); break;
			case ParticleWorld.Implementation.CPU: impl = new MPParticleSetImplCPU(this); break;
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
		ParticleWorld world = ParticleWorld.instance;
		csWorldData[0].particle_lifetime = csWorldData[0].particle_lifetime;
		csWorldData[0].num_sphere_colliders = ParticleCollider.csSphereColliders.Count;
		csWorldData[0].num_capsule_colliders = ParticleCollider.csCapsuleColliders.Count;
		csWorldData[0].num_box_colliders = ParticleCollider.csBoxColliders.Count;
		csWorldData[0].rt_size = world.rt_size;
		csWorldData[0].view_proj = world.viewproj;
		csWorldData[0].SetWorldSize(transform.position, transform.localScale, new UVector3 { x = worldDivX, y = worldDivY, z = worldDivZ });
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
		impl.AddParticles(particles);
	}
}
