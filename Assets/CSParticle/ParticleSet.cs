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
	ComputeBuffer cbWorldIData;
	ComputeBuffer cbSPHParams;
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
		cbSPHParams.Release();
		cbWorldIData.Release();
		cbWorldData.Release();
	}

	public override void Start()
	{
		world = ParticleWorld.instance;
		wimpl = world.impl as MPParticleWorldImplGPU;

		//Debug.Log("Marshal.SizeOf(typeof(CSParticle))" + Marshal.SizeOf(typeof(CSParticle)));
		//Debug.Log("Marshal.SizeOf(typeof(CSWordData))" + Marshal.SizeOf(typeof(CSWorldData)));
		IVector3 world_div = pset.csWorldData[0].world_div;
		int num_cells = world_div.x * world_div.y * world_div.z;

		cbWorldData = new ComputeBuffer(1, CSWorldData.size);
		cbWorldIData = new ComputeBuffer(1, CSWorldIData.size);
		cbWorldIData.SetData(new CSWorldIData[1]);
		cbSPHParams = new ComputeBuffer(1, CSSPHParams.size);
		cbCells = new ComputeBuffer(num_cells, CSCell.size);
		cbParticles[0] = new ComputeBuffer(pset.maxParticles, CSParticle.size);
		cbParticles[1] = new ComputeBuffer(pset.maxParticles, CSParticle.size);
		cbParticles[0].SetData(pset.particles);
		cbParticlesToAdd = new ComputeBuffer(pset.maxParticles, CSParticle.size);
		cbPIntermediate = new ComputeBuffer(pset.maxParticles, CSParticleIData.size);
		cbSortData[0] = new ComputeBuffer(pset.maxParticles, CSSortData.size);
		cbSortData[1] = new ComputeBuffer(pset.maxParticles, CSSortData.size);

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

		pset.csWorldData[0].num_additional_particles = particlesToAdd.Count;
		cbWorldData.SetData(pset.csWorldData);
		CSWorldData csWorldData = pset.csWorldData[0];
		cbSPHParams.SetData(pset.csSPHParams);


		const int BLOCK_SIZE = 512;

		{
			CSWorldIData[] wid = new CSWorldIData[1];
			cbWorldIData.GetData(wid);
			pset.csWorldIData[0].num_active_particles = wid[0].num_active_particles;
		}

		// add new particles
		pset.csWorldIData[0].num_active_particles += particlesToAdd.Count;
		if (particlesToAdd.Count>0)
		{
			ComputeShader cs = csParticle;
			int kernel = wimpl.kAddParticles;
			cbParticlesToAdd.SetData(particlesToAdd.ToArray());
			cs.SetBuffer(kernel, "world_data", cbWorldData);
			cs.SetBuffer(kernel, "world_idata", cbWorldIData);
			cs.SetBuffer(kernel, "particles", cbParticles[0]);
			cs.SetBuffer(kernel, "particles_to_add", cbParticlesToAdd);
			cs.Dispatch(kernel, particlesToAdd.Count / BLOCK_SIZE + 1, 1, 1);
			particlesToAdd.Clear();
		}

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
			cs.SetBuffer(kernel, "world_idata", cbWorldIData);
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
			cs.SetBuffer(kernel, "world_idata", cbWorldIData);
			cs.SetBuffer(kernel, "particles", cbParticles[0]);
			cs.SetBuffer(kernel, "particles_rw", cbParticles[1]);
			cs.SetBuffer(kernel, "sort_keys", cbSortData[0]);
			cs.SetBuffer(kernel, "cells_rw", cbCells);
			cs.Dispatch(kernel, pset.maxParticles / BLOCK_SIZE, 1, 1);
			MPUtil.Swap(ref cbParticles[0], ref cbParticles[1]);
		}

		//{
		//	dbgSortData = new GPUSort.KIP[csWorldData.num_max_particles];
		//	cbSortData[0].GetData(dbgSortData);
		//	uint prev = 0;
		//	for (int i = 0; i < dbgSortData.Length; ++i)
		//	{
		//		if (prev > dbgSortData[i].key)
		//		{
		//			Debug.Log("sort bug: "+i);
		//			break;
		//		}
		//		prev = dbgSortData[i].key;
		//	}
		//}

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
			cs.SetBuffer(kernel, "cells", cbCells);
			cs.Dispatch(kernel, pset.maxParticles / BLOCK_SIZE, 1, 1);
		}

		// particle interaction
		if (pset.interactionMode == ParticleSet.Interaction.Impulse)
		{
			ComputeShader cs = csParticle;
			int kernel = pset.dimension == ParticleSet.Dimension.Dimendion3D ?
				wimpl.kProcessInteraction_Impulse : wimpl.kProcessInteraction_Impulse2D;
			cs.SetBuffer(kernel, "world_data", cbWorldData);
			cs.SetBuffer(kernel, "particles", cbParticles[0]);
			cs.SetBuffer(kernel, "pimd", cbPIntermediate);
			cs.SetBuffer(kernel, "cells", cbCells);
			cs.Dispatch(kernel, pset.maxParticles / BLOCK_SIZE, 1, 1);
		}
		else if (pset.interactionMode == ParticleSet.Interaction.SPH)
		{
			ComputeShader cs = csParticle;
			int kernel = pset.dimension == ParticleSet.Dimension.Dimendion3D ?
				wimpl.kProcessInteraction_SPH_Pass1 : wimpl.kProcessInteraction_SPH_Pass12D;
			cs.SetBuffer(kernel, "world_data", cbWorldData);
			cs.SetBuffer(kernel, "sph_params", cbSPHParams);
			cs.SetBuffer(kernel, "particles", cbParticles[0]);
			cs.SetBuffer(kernel, "pimd", cbPIntermediate);
			cs.SetBuffer(kernel, "cells", cbCells);
			cs.Dispatch(kernel, pset.maxParticles / BLOCK_SIZE, 1, 1);

			kernel = pset.dimension == ParticleSet.Dimension.Dimendion3D ?
				wimpl.kProcessInteraction_SPH_Pass2 : wimpl.kProcessInteraction_SPH_Pass22D;
			cs.SetBuffer(kernel, "world_data", cbWorldData);
			cs.SetBuffer(kernel, "sph_params", cbSPHParams);
			cs.SetBuffer(kernel, "particles", cbParticles[0]);
			cs.SetBuffer(kernel, "pimd", cbPIntermediate);
			cs.SetBuffer(kernel, "cells", cbCells);
			cs.Dispatch(kernel, pset.maxParticles / BLOCK_SIZE, 1, 1);
		}
		else if (pset.interactionMode == ParticleSet.Interaction.None)
		{
			// do nothing
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
			cs.SetBuffer(kernel, "cells", cbCells);
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
			cs.SetBuffer(kernel, "cells", cbCells);
			cs.Dispatch(kernel, num_cells / BLOCK_SIZE, 1, 1);
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

	MeshTopology GetTopologyType()
	{
		switch (pset.renderMode)
		{
			case ParticleSet.RenderMode.Point: return MeshTopology.Points;
			case ParticleSet.RenderMode.Billboard: return MeshTopology.Triangles;
			case ParticleSet.RenderMode.Cube: return MeshTopology.Triangles;
		}
		return MeshTopology.Points;
	}
	int GetNumVertices()
	{
		switch (pset.renderMode)
		{
			case ParticleSet.RenderMode.Point: return 1;
			case ParticleSet.RenderMode.Billboard: return 6;
			case ParticleSet.RenderMode.Cube: return 36;
		}
		return 1;
	}

	public override void DepthPrePass()
	{
		Material matGBuffer = pset.matParticleGBuffer;
		if (pset.processGBufferCollision || !pset.depthPrePass) { return; }

		matGBuffer.SetBuffer("vertices", wimpl.cbCubeVertices);
		matGBuffer.SetBuffer("particles", cbParticles[0]);
		matGBuffer.SetInt("_FlipY", 1);
		matGBuffer.SetPass(1);
		Graphics.DrawProcedural(GetTopologyType(), GetNumVertices(), pset.GetNumParticles());
	}

	public override void GBufferPass()
	{
		Material matGBuffer = pset.matParticleGBuffer;
		matGBuffer.SetBuffer("vertices", wimpl.cbCubeVertices);
		matGBuffer.SetBuffer("particles", cbParticles[0]);
		matGBuffer.SetInt("_FlipY", 0);
		matGBuffer.SetPass(pset.depthPrePass && !pset.processGBufferCollision  ? 2 : 0);
		Graphics.DrawProcedural(GetTopologyType(), GetNumVertices(), pset.GetNumParticles());
	}

	public override void TransparentPass()
	{
		Material matTransparent = pset.matParticleTransparent;
		matTransparent.SetBuffer("vertices", wimpl.cbCubeVertices);
		matTransparent.SetBuffer("particles", cbParticles[0]);
		matTransparent.SetPass(0);
		Graphics.DrawProcedural(GetTopologyType(), GetNumVertices(), pset.GetNumParticles());
	}

	public override void HandleParticleCollision()
	{
		if (pset.handler != null)
		{
			CSWorldIData[] wid = new CSWorldIData[1];
			cbWorldIData.GetData(wid);
			pset.csWorldIData[0].num_active_particles = wid[0].num_active_particles;

			cbParticles[0].GetData(pset.particles);
			pset.handler(pset.particles, wid[0].num_active_particles, world.prevColliders);
			cbParticles[0].SetData(pset.particles);
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


	public delegate void ParticleHandler(CSParticle[] particles, int num_particles, List<ParticleCollider> colliders);

	public enum Dimension
	{
		Dimendion3D,
		Dimendion2D,
	}

	public enum Interaction
	{
		Impulse,
		SPH,
		None,
	}

	public enum RenderMode
	{
		Point,
		Billboard,
		Cube,
	}

	public ParticleWorld.Implementation implMode;
	public Dimension dimension = Dimension.Dimendion3D;
	public Interaction interactionMode = Interaction.Impulse;
	public int maxParticles = 32768;
	public float particleRadius = 0.05f;
	public int worldDivX = 256;
	public int worldDivY = 1;
	public int worldDivZ = 256;
	public float deccelerate = 0.99f;
	public float pressureStiffness = 500.0f;
	public float wallStiffness = 1000.0f;
	public Vector3 coordScaler = Vector3.one;

	public float SPH_smoothlen = 0.2f;
	public float SPH_particleMass = 0.0002f;
	public float SPH_pressureStiffness = 200.0f;
	public float SPH_restDensity = 1000.0f;
	public float SPH_viscosity = 0.1f;

	public RenderMode renderMode = RenderMode.Cube;
	public bool depthPrePass = true;
	public bool processGBufferCollision = false;
	public bool processColliders = true;
	public bool processForces = true;
	public float lifetime = 20.0f;
	public Material matParticleGBuffer;
	public Material matParticleTransparent;
	public ParticleHandler handler;

	public CSParticle[] particles;
	public CSWorldData[] csWorldData = new CSWorldData[1];
	public CSWorldIData[] csWorldIData = new CSWorldIData[1];
	public CSSPHParams[] csSPHParams = new CSSPHParams[1];

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
		csWorldData[0].SetWorldSize(transform.position, transform.localScale*0.5f,
			new UVector3 { x = (uint)worldDivX, y = (uint)worldDivY, z = (uint)worldDivZ });
		csSPHParams[0].SetDefaultValues(csWorldData[0].particle_size);

		particles = new CSParticle[maxParticles];
		for (int i = 0; i < particles.Length; ++i )
		{
			particles[i].hit_objid = -1;
			//particles[i].owner_objid = -1;
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
		csWorldData[0].particle_size = particleRadius;
		csWorldData[0].particle_lifetime = lifetime;
		csWorldData[0].num_sphere_colliders = ParticleCollider.csSphereColliders.Count;
		csWorldData[0].num_capsule_colliders = ParticleCollider.csCapsuleColliders.Count;
		csWorldData[0].num_box_colliders = ParticleCollider.csBoxColliders.Count;
		csWorldData[0].num_forces = ParticleForce.forceData.Count;
		csWorldData[0].decelerate = deccelerate;
		csWorldData[0].coord_scaler = coordScaler;
		csWorldData[0].pressure_stiffness = pressureStiffness;
		csWorldData[0].wall_stiffness = wallStiffness;
		csWorldData[0].view_proj = world.viewproj;
		csWorldData[0].rt_size = world.rt_size;

		csSPHParams[0].smooth_len = SPH_smoothlen;
		csSPHParams[0].particle_mass = SPH_particleMass;
		csSPHParams[0].pressure_stiffness = SPH_pressureStiffness;
		csSPHParams[0].rest_density = SPH_restDensity;
		csSPHParams[0].viscosity = SPH_viscosity;
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
		Gizmos.DrawWireCube(transform.position, transform.localScale);
	}


	public void AddParticles(CSParticle[] particles)
	{
		impl.AddParticles(particles);
	}

	public int GetNumParticles()
	{
		return csWorldIData[0].num_active_particles;
	}
}
