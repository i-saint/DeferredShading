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

public class MPUtil
{
	public static void Swap<T>(ref T lhs, ref T rhs)
	{
		T temp;
		temp = lhs;
		lhs = rhs;
		rhs = temp;
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
	public struct CellData
	{
		public uint begin;
		public uint end;
	}

	ParticleSet pset;
	ParticleWorld world;
	MPParticleWorldImplGPU wimpl;
	List<CSParticle> particlesToAdd = new List<CSParticle>();
	ComputeBuffer cbWorldData;
	ComputeBuffer cbCells;
	ComputeBuffer[] cbParticles = new ComputeBuffer[2];
	ComputeBuffer cbParticlesToAdd;
	ComputeBuffer cbPIntermediate;
	ComputeBuffer[] cbSortData = new ComputeBuffer[2];
	GPUSort gpusort;

	CellData[] dbgCellData;
	GPUSort.KIP[] dbgSortData;


	public MPParticleSetImplGPU(ParticleSet p)
	{
		pset = p;
	}

	public override void OnEnable()
	{
	}

	public override void OnDisable()
	{
		gpusort.OnDisable();

		cbParticlesToAdd.Release();
		cbPIntermediate.Release();
		cbSortData[0].Release();
		cbSortData[1].Release();
		cbParticles[0].Release();
		cbParticles[1].Release();
		cbCells.Release();
		cbWorldData.Release();
	}

	public override void Start()
	{
		world = ParticleWorld.instance;
		wimpl = world.impl as MPParticleWorldImplGPU;

		//Debug.Log("Marshal.SizeOf(typeof(CSParticle))" + Marshal.SizeOf(typeof(CSParticle)));
		//Debug.Log("Marshal.SizeOf(typeof(CSWordData))" + Marshal.SizeOf(typeof(CSWorldData)));
		IVector3 world_div = pset.csWorldData[0].world_div;
		int sizeof_WorldData = 208;
		int sizeof_CellData = 8;
		int sizeof_ParticleData = 40;
		int sizeof_IMData = 12;
		int sizeof_SortData = 8;
		int num_cells = world_div.x * world_div.y * world_div.z;

		cbWorldData = new ComputeBuffer(1, sizeof_WorldData);
		cbCells = new ComputeBuffer(num_cells, sizeof_CellData);
		cbParticles[0] = new ComputeBuffer(pset.maxParticles, sizeof_ParticleData);
		cbParticles[1] = new ComputeBuffer(pset.maxParticles, sizeof_ParticleData);
		cbParticles[0].SetData(pset.particles);
		cbParticlesToAdd = new ComputeBuffer(pset.maxParticles, sizeof_ParticleData);
		cbPIntermediate = new ComputeBuffer(pset.maxParticles, sizeof_IMData);
		cbSortData[0] = new ComputeBuffer(pset.maxParticles, sizeof_SortData);
		cbSortData[1] = new ComputeBuffer(pset.maxParticles, sizeof_SortData);

		gpusort = new GPUSort();
		gpusort.Start();
	}

	public override void Update()
	{
		ComputeShader csParticle = world.csParticle;
		ComputeShader csHashGrid = world.csHashGrid;
		ComputeShader csSort = world.csBitonicSort;
		IVector3 world_div = pset.csWorldData[0].world_div;
		int num_cells = world_div.x * world_div.y * world_div.z;

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
			cbParticles[0].SetData(pset.particles);
		}
		cbWorldData.SetData(pset.csWorldData);
		CSWorldData csWorldData = pset.csWorldData[0];


		const int BLOCK_SIZE = 512;

		//// add new particles
		//{
		//	ComputeShader cs = csParticle;
		//	int kernel = wimpl.kAddParticles;
		//	cs.SetBuffer(kernel, "world_data", cbWorldData);
		//	cs.SetBuffer(kernel, "particles", cbParticles[0]);
		//	cs.SetBuffer(kernel, "pimd", cbPIntermediate);
		//	cs.Dispatch(kernel, pset.maxParticles / BLOCK_SIZE, 1, 1);
		//}

		// clear cells
		{
			ComputeShader cs = csHashGrid;
			int kernel = 0;
			cs.SetBuffer(kernel, "cells_rw", cbCells);
			cs.Dispatch(kernel, num_cells / BLOCK_SIZE, 1, 1);
		}
		// generate hashes
		{
			ComputeShader cs = csHashGrid;
			int kernel = 1;
			cs.SetBuffer(kernel, "world_data", cbWorldData);
			cs.SetBuffer(kernel, "particles", cbParticles[0]);
			cs.SetBuffer(kernel, "sort_keys_rw", cbSortData[0]);
			cs.Dispatch(kernel, pset.maxParticles / BLOCK_SIZE, 1, 1);
		}
		// sort keys
		{
			gpusort.BitonicSort(csSort, cbSortData[0], cbSortData[1], (uint)csWorldData.num_max_particles);
		}
		// reorder particles
		{
			ComputeShader cs = csHashGrid;
			int kernel = 2;
			cs.SetBuffer(kernel, "world_data", cbWorldData);
			cs.SetBuffer(kernel, "particles", cbParticles[0]);
			cs.SetBuffer(kernel, "particles_rw", cbParticles[1]);
			cs.SetBuffer(kernel, "sort_keys", cbSortData[0]);
			cs.SetBuffer(kernel, "cells_rw", cbCells);
			cs.Dispatch(kernel, pset.maxParticles / BLOCK_SIZE, 1, 1);
			MPUtil.Swap(ref cbParticles[0], ref cbParticles[1]);
		}

		{
			dbgSortData = new GPUSort.KIP[csWorldData.num_max_particles];
			cbSortData[0].GetData(dbgSortData);
			uint prev = 0;
			for (int i = 0; i < dbgSortData.Length; ++i)
			{
				if (prev > dbgSortData[i].key)
				{
					Debug.Log("sort bug: "+i);
					break;
				}
				prev = dbgSortData[i].key;
			}
		}

		//dbgCellData = new CellData[num_cells];
		//cbCells.GetData(dbgCellData);
		//for (int i = 0; i < num_cells; ++i )
		//{
		//	if (dbgCellData[i].begin!=0)
		//	{
		//		Debug.Log("dbgCellData:" + dbgCellData[i].begin + "," + dbgCellData[i].end);
		//		break;
		//	}
		//}


		// initialize intermediate data
		{
			ComputeShader cs = csParticle;
			int kernel = wimpl.kPrepare;
			cs.SetBuffer(kernel, "world_data", cbWorldData);
			cs.SetBuffer(kernel, "particles", cbParticles[0]);
			cs.SetBuffer(kernel, "pimd", cbPIntermediate);
			cs.Dispatch(kernel, pset.maxParticles / BLOCK_SIZE, 1, 1);
		}

		// particle interaction
		if (pset.interactionMode == ParticleSet.Interaction.Impulse)
		{
			ComputeShader cs = csParticle;
			int kernel = wimpl.kProcessInteraction_Impulse;
			cs.SetBuffer(kernel, "world_data", cbWorldData);
			cs.SetBuffer(kernel, "particles", cbParticles[0]);
			cs.SetBuffer(kernel, "pimd", cbPIntermediate);
			cs.SetBuffer(kernel, "cells", cbCells);
			cs.Dispatch(kernel, num_cells / BLOCK_SIZE, 1, 1);
		}
		else if (pset.interactionMode == ParticleSet.Interaction.SPH)
		{
			ComputeShader cs = csParticle;
			int kernel = wimpl.kProcessInteraction_SPH_Pass1;
			cs.SetBuffer(kernel, "world_data", cbWorldData);
			cs.SetBuffer(kernel, "particles", cbParticles[0]);
			cs.SetBuffer(kernel, "pimd", cbPIntermediate);
			cs.SetBuffer(kernel, "cells", cbCells);
			cs.Dispatch(kernel, num_cells / BLOCK_SIZE, 1, 1);

			kernel = wimpl.kProcessInteraction_SPH_Pass2;
			cs.SetBuffer(kernel, "world_data", cbWorldData);
			cs.SetBuffer(kernel, "particles", cbParticles[0]);
			cs.SetBuffer(kernel, "pimd", cbPIntermediate);
			cs.SetBuffer(kernel, "cells", cbCells);
			cs.Dispatch(kernel, num_cells / BLOCK_SIZE, 1, 1);
		}
		else if (pset.interactionMode == ParticleSet.Interaction.None)
		{
		}

		// gbuffer collision
		if (pset.processGBufferCollision)
		{
			ComputeShader cs = csParticle;
			int kernel = wimpl.kProcessGBufferCollision;
			cs.SetTexture(kernel, "gbuffer_normal", world.rtNormalBufferCopy);
			cs.SetTexture(kernel, "gbuffer_position", world.rtPositionBufferCopy);
			cs.SetBuffer(kernel, "world_data", cbWorldData);
			cs.SetBuffer(kernel, "particles", cbParticles[0]);
			cs.SetBuffer(kernel, "pimd", cbPIntermediate);
			cs.Dispatch(kernel, pset.maxParticles / BLOCK_SIZE, 1, 1);
		}

		// colliders
		if (pset.processColliders)
		{
			ComputeShader cs = csParticle;
			int kernel = wimpl.kProcessColliders;
			cs.SetBuffer(kernel, "world_data", cbWorldData);
			cs.SetBuffer(kernel, "particles", cbParticles[0]);
			cs.SetBuffer(kernel, "pimd", cbPIntermediate);
			cs.Dispatch(kernel, num_cells / BLOCK_SIZE, 1, 1);
		}

		// forces
		if (pset.processForces)
		{
			ComputeShader cs = csParticle;
			int kernel = wimpl.kProcessForces;
			cs.SetBuffer(kernel, "world_data", cbWorldData);
			cs.SetBuffer(kernel, "particles", cbParticles[0]);
			cs.SetBuffer(kernel, "pimd", cbPIntermediate);
			cs.Dispatch(kernel, pset.maxParticles / BLOCK_SIZE, 1, 1);
		}

		// integrate
		{
			ComputeShader cs = csParticle;
			int kernel = wimpl.kIntegrate;
			cs.SetBuffer(kernel, "world_data", cbWorldData);
			cs.SetBuffer(kernel, "particles", cbParticles[0]);
			cs.SetBuffer(kernel, "pimd", cbPIntermediate);
			cs.Dispatch(kernel, pset.maxParticles / BLOCK_SIZE, 1, 1);
		}
	}

	public override void DepthPrePass()
	{
		Material matGBuffer = pset.matParticleGBuffer;
		if (pset.processGBufferCollision || !pset.depthPrePass) { return; }

		matGBuffer.SetBuffer("vertices", wimpl.cbCubeVertices);
		matGBuffer.SetBuffer("particles", cbParticles[0]);
		matGBuffer.SetInt("_FlipY", 1);
		matGBuffer.SetPass(1);
		Graphics.DrawProcedural(MeshTopology.Triangles, 36, pset.maxParticles);
	}

	public override void GBufferPass()
	{
		Material matGBuffer = pset.matParticleGBuffer;
		matGBuffer.SetBuffer("vertices", wimpl.cbCubeVertices);
		matGBuffer.SetBuffer("particles", cbParticles[0]);
		matGBuffer.SetInt("_FlipY", 0);
		matGBuffer.SetPass(pset.depthPrePass ? 2 : 0);
		Graphics.DrawProcedural(MeshTopology.Triangles, 36, pset.maxParticles);
	}

	public override void TransparentPass()
	{
		Material matTransparent = pset.matParticleTransparent;
		matTransparent.SetBuffer("vertices", wimpl.cbCubeVertices);
		matTransparent.SetBuffer("particles", cbParticles[0]);
		matTransparent.SetPass(0);
		Graphics.DrawProcedural(MeshTopology.Triangles, 36, pset.maxParticles);
	}

	public override void HandleParticleCollision()
	{
		cbParticles[0].GetData(pset.particles);
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
	public int worldDivX = 256;
	public int worldDivY = 1;
	public int worldDivZ = 256;
	public bool depthPrePass = true;
	public bool processGBufferCollision = false;
	public bool processColliders = true;
	public bool processForces = true;
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
		csWorldData[0].SetWorldSize(transform.position, transform.localScale,
			new UVector3 { x = (uint)worldDivX, y = (uint)worldDivY, z = (uint)worldDivZ });

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
		impl.Update();
	}

	void DepthPrePass()
	{
		if (matParticleGBuffer == null) { return; }
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
