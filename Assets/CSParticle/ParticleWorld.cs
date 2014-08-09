using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public struct CSVertexData
{
	public Vector3 position;
	public Vector3 normal;
}

public abstract class IParticleWorldImpl
{
	public abstract void OnEnable();
	public abstract void OnDisable();
	public abstract void Start();
	public abstract void Update();
}

public class MPParticleWorldImplCPU : IParticleWorldImpl
{
	public override void OnEnable() { }
	public override void OnDisable() { }
	public override void Start() { }
	public override void Update() { }
}


public class MPParticleWorldImplGPU : IParticleWorldImpl
{
	public int kAddParticles;
	public int kPrepare;

	public int kProcessInteraction_Impulse;
	public int kProcessInteraction_SPH_Pass1;
	public int kProcessInteraction_SPH_Pass2;
	public int kProcessColliders;
	public int kProcessGBufferCollision;
	public int kProcessForces;
	public int kIntegrate;
	public int kProcessInteraction_Impulse2D;
	public int kProcessInteraction_SPH_Pass12D;
	public int kProcessInteraction_SPH_Pass22D;

	public ComputeBuffer cbSphereColliders;
	public ComputeBuffer cbCapsuleColliders;
	public ComputeBuffer cbBoxColliders;
	public ComputeBuffer cbForces;
	public ComputeBuffer cbCubeVertices;
	public int capSphereColliders = 256;
	public int capCapsuleColliders = 256;
	public int capBoxColliders = 256;
	public int capForces = 128;
	public int numParticles = 0;

	public override void OnEnable()
	{
	}

	public override void OnDisable()
	{
		cbSphereColliders.Release();
		cbCapsuleColliders.Release();
		cbBoxColliders.Release();
		cbForces.Release();
		cbCubeVertices.Release();
	}

	public override void Start()
	{
		ParticleWorld world = ParticleWorld.instance;
		kAddParticles = world.csParticle.FindKernel("AddParticles");
		kPrepare = world.csParticle.FindKernel("Prepare");
		kProcessInteraction_Impulse = world.csParticle.FindKernel("ProcessInteraction_Impulse");
		kProcessInteraction_SPH_Pass1 = world.csParticle.FindKernel("ProcessInteraction_SPH_Density");
		kProcessInteraction_SPH_Pass2 = world.csParticle.FindKernel("ProcessInteraction_SPH_Force");
		kProcessColliders = world.csParticle.FindKernel("ProcessColliders");
		kProcessGBufferCollision = world.csParticle.FindKernel("ProcessGBufferCollision");
		kProcessForces = world.csParticle.FindKernel("ProcessForces");
		kIntegrate = world.csParticle.FindKernel("Integrate");

		kProcessInteraction_Impulse2D = world.csParticle.FindKernel("ProcessInteraction_Impulse2D");
		kProcessInteraction_SPH_Pass12D = world.csParticle.FindKernel("ProcessInteraction_SPH_Density2D");
		kProcessInteraction_SPH_Pass22D = world.csParticle.FindKernel("ProcessInteraction_SPH_Force2D");

		cbCubeVertices = new ComputeBuffer(36, 24);
		{
			const float s = 0.05f;
			const float p = 1.0f;
			const float n = -1.0f;
			const float z = 0.0f;
			Vector3[] positions = new Vector3[24] {
				new Vector3(-s,-s, s), new Vector3( s,-s, s), new Vector3( s, s, s), new Vector3(-s, s, s),
				new Vector3(-s, s,-s), new Vector3( s, s,-s), new Vector3( s, s, s), new Vector3(-s, s, s),
				new Vector3(-s,-s,-s), new Vector3( s,-s,-s), new Vector3( s,-s, s), new Vector3(-s,-s, s),
				new Vector3(-s,-s, s), new Vector3(-s,-s,-s), new Vector3(-s, s,-s), new Vector3(-s, s, s),
				new Vector3( s,-s, s), new Vector3( s,-s,-s), new Vector3( s, s,-s), new Vector3( s, s, s),
				new Vector3(-s,-s,-s), new Vector3( s,-s,-s), new Vector3( s, s,-s), new Vector3(-s, s,-s),
			};
			Vector3[] normals = new Vector3[24] {
				new Vector3(z, z, p), new Vector3(z, z, p), new Vector3(z, z, p), new Vector3(z, z, p),
				new Vector3(z, p, z), new Vector3(z, p, z), new Vector3(z, p, z), new Vector3(z, p, z),
				new Vector3(z, n, z), new Vector3(z, n, z), new Vector3(z, n, z), new Vector3(z, n, z),
				new Vector3(n, z, z), new Vector3(n, z, z), new Vector3(n, z, z), new Vector3(n, z, z),
				new Vector3(p, z, z), new Vector3(p, z, z), new Vector3(p, z, z), new Vector3(p, z, z),
				new Vector3(z, z, n), new Vector3(z, z, n), new Vector3(z, z, n), new Vector3(z, z, n),
			};
			int[] indices = new int[36] {
				0,1,3, 3,1,2,
				5,4,6, 6,4,7,
				8,9,11, 11,9,10,
				13,12,14, 14,12,15,
				16,17,19, 19,17,18,
				21,20,22, 22,20,23,
			};
			CSVertexData[] vertices = new CSVertexData[36];
			for (int i = 0; i < vertices.Length; ++i)
			{
				vertices[i].position = positions[indices[i]];
				vertices[i].normal = normals[indices[i]];
			}
			cbCubeVertices.SetData(vertices);
		}

		// doesn't work on WebPlayer
		//Debug.Log("Marshal.SizeOf(typeof(CSSphereCollider))" + Marshal.SizeOf(typeof(CSSphereCollider)));
		//Debug.Log("Marshal.SizeOf(typeof(CSCapsuleCollider))" + Marshal.SizeOf(typeof(CSCapsuleCollider)));
		//Debug.Log("Marshal.SizeOf(typeof(CSBoxCollider))" + Marshal.SizeOf(typeof(CSBoxCollider)));
		//Debug.Log("Marshal.SizeOf(typeof(CSForce))" + Marshal.SizeOf(typeof(CSForce)));
		cbSphereColliders = new ComputeBuffer(capSphereColliders, 44);
		cbCapsuleColliders = new ComputeBuffer(capCapsuleColliders, 56);
		cbBoxColliders = new ComputeBuffer(capBoxColliders, 136);
		cbForces = new ComputeBuffer(capForces, 188);

		world.csParticle.SetBuffer(kProcessColliders, "sphere_colliders", cbSphereColliders);
		world.csParticle.SetBuffer(kProcessColliders, "capsule_colliders", cbCapsuleColliders);
		world.csParticle.SetBuffer(kProcessColliders, "box_colliders", cbBoxColliders);
		world.csParticle.SetBuffer(kProcessForces, "forces", cbForces);
	}

	public override void Update()
	{
		if (ParticleCollider.csSphereColliders.Count >= capSphereColliders)
		{
			while (ParticleCollider.csSphereColliders.Count >= capSphereColliders)
			{
				capSphereColliders *= 2;
			}
			cbSphereColliders.Release();
			cbSphereColliders = new ComputeBuffer(capSphereColliders, 44);
		}
		cbSphereColliders.SetData(ParticleCollider.csSphereColliders.ToArray());

		if (ParticleCollider.csCapsuleColliders.Count >= capCapsuleColliders)
		{
			while (ParticleCollider.csCapsuleColliders.Count >= capCapsuleColliders)
			{
				capCapsuleColliders *= 2;
			}
			cbCapsuleColliders.Release();
			cbCapsuleColliders = new ComputeBuffer(capCapsuleColliders, 56);
		}
		cbCapsuleColliders.SetData(ParticleCollider.csCapsuleColliders.ToArray());

		if (ParticleCollider.csBoxColliders.Count >= capBoxColliders)
		{
			while (ParticleCollider.csBoxColliders.Count >= capBoxColliders)
			{
				capBoxColliders *= 2;
			}
			cbBoxColliders.Release();
			cbBoxColliders = new ComputeBuffer(capBoxColliders, 136);
		}
		cbBoxColliders.SetData(ParticleCollider.csBoxColliders.ToArray());


		if (ParticleForce.forceData.Count >= capForces)
		{
			while (ParticleForce.forceData.Count >= capForces)
			{
				capForces *= 2;
			}
			cbForces.Release();
			cbForces = new ComputeBuffer(capForces, 136);
		}
		cbForces.SetData(ParticleForce.forceData.ToArray());
	}
}




public class ParticleWorld : MonoBehaviour
{
	public enum Implementation
	{
		GPU,
		CPU,
	}
	public static ParticleWorld instance;

	public Implementation implMode;
	public GameObject cam;
	public ComputeShader csParticle;
	public ComputeShader csBitonicSort;
	public ComputeShader csHashGrid;
	public Material matCopyGBuffer;

	public List<ParticleCollider> prevColliders = new List<ParticleCollider>();
	public Vector2 rt_size;
	public Matrix4x4 viewproj;

	public RenderTexture[] rtGBufferCopy;
	public RenderBuffer[] rbGBufferCopy;
	public RenderTexture rtNormalBufferCopy { get { InitializeGBufferCopy();  return rtGBufferCopy[0]; } }
	public RenderTexture rtPositionBufferCopy { get { InitializeGBufferCopy(); return rtGBufferCopy[1]; } }

	public IParticleWorldImpl impl;


	void InitializeGBufferCopy()
	{
		if (rtGBufferCopy == null || rbGBufferCopy == null)
		{
			rtGBufferCopy = new RenderTexture[2];
			rbGBufferCopy = new RenderBuffer[2];
			Camera c = cam.GetComponent<Camera>();
			for (int i = 0; i < rtGBufferCopy.Length; ++i)
			{
				rtGBufferCopy[i] = DSRenderer.CreateRenderTexture((int)c.pixelWidth, (int)c.pixelHeight, 0, RenderTextureFormat.ARGBHalf);
				rbGBufferCopy[i] = rtGBufferCopy[i].colorBuffer;
			}
		}
	}

	void OnEnable()
	{
		instance = this;
		switch (implMode)
		{
			case Implementation.GPU: impl = new MPParticleWorldImplGPU(); break;
			case Implementation.CPU: impl = new MPParticleWorldImplCPU(); break;
		}
		impl.OnEnable();
	}

	void OnDisable()
	{
		impl.OnDisable();
		impl = null;
		if (instance == this)
		{
			instance = null;
		}
	}

	void Start()
	{
		DSRenderer dscam = cam.GetComponent<DSRenderer>();
		dscam.AddCallbackPreGBuffer(() => { DepthPrePass(); }, 800);
		dscam.AddCallbackPostGBuffer(() => { GBufferPass(); }, 1000);
		dscam.AddCallbackTransparent(() => { TransparentPass(); }, 1000);

		impl.Start();
	}

	void Update()
	{
		ParticleSet.HandleParticleCollisionAll();

		ParticleCollider.UpdateAll();
		ParticleForce.UpdateAll();
		impl.Update();

		prevColliders.Clear();
		prevColliders.AddRange(ParticleCollider.instances);

		Camera c = cam.GetComponent<Camera>();
		DSRenderer dscam = cam.GetComponent<DSRenderer>();
		Matrix4x4 view = c.worldToCameraMatrix;
		Matrix4x4 proj = c.projectionMatrix;
		proj[2, 0] = proj[2, 0] * 0.5f + proj[3, 0] * 0.5f;
		proj[2, 1] = proj[2, 1] * 0.5f + proj[3, 1] * 0.5f;
		proj[2, 2] = proj[2, 2] * 0.5f + proj[3, 2] * 0.5f;
		proj[2, 3] = proj[2, 3] * 0.5f + proj[3, 3] * 0.5f;
		viewproj = proj * view;
		rt_size = new Vector2(dscam.rtNormalBuffer.width, dscam.rtNormalBuffer.height);

		ParticleSet.UpdateAll();
		ParticleForce.forceData.Clear();
	}

	void DepthPrePass()
	{
		ParticleSet.DepthPrePassAll();
	}

	void GBufferPass()
	{
		DSRenderer dscam = cam.GetComponent<DSRenderer>();
		bool needs_gbuffer_copy = false;
		for (int i = 0; i < ParticleSet.instances.Count; ++i)
		{
			if (ParticleSet.instances[i].processGBufferCollision)
			{
				needs_gbuffer_copy = true;
				break;
			}
		}
		if (needs_gbuffer_copy)
		{
			Graphics.SetRenderTarget(rbGBufferCopy, rtGBufferCopy[0].depthBuffer);
			matCopyGBuffer.SetTexture("_NormalBuffer", dscam.rtNormalBuffer);
			matCopyGBuffer.SetTexture("_PositionBuffer", dscam.rtPositionBuffer);
			matCopyGBuffer.SetPass(0);
			DSRenderer.DrawFullscreenQuad();
			dscam.SetRenderTargetsGBuffer();
		}

		ParticleSet.GBufferPassAll();
	}

	void TransparentPass()
	{
		ParticleSet.TransparentPassAll();
	}
}
