using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public struct CSVertexData
{
	public Vector3 position;
	public Vector3 normal;
}


public class CSParticleWorld : MonoBehaviour
{
	public const int MAX_SPHERE_COLLIDERS = 256;
	public const int MAX_CAPSULE_COLLIDERS = 256;
	public const int MAX_BOX_COLLIDERS = 256;

	public GameObject cam;
	public Material matCSParticle;
	public ComputeShader csParticle;
	public Material matCopyGBuffer;

	public int kernelProcessColliders;
	public int kernelProcessGBufferCollision;
	public int kernelIntegrate;
	public ComputeBuffer cbSphereColliders;
	public ComputeBuffer cbCapsuleColliders;
	public ComputeBuffer cbBoxColliders;
	public ComputeBuffer cbCubeVertices;
	public List<CSParticleCollider> prevColliders = new List<CSParticleCollider>();
	public Vector2 rt_size;
	public Matrix4x4 viewproj;

	public RenderTexture[] rtShadowGBuffer;
	public RenderBuffer[] rbShadowGBuffer;
	public RenderTexture rtShadowNormalBuffer { get { return rtShadowGBuffer[0]; } }
	public RenderTexture rtShadowPositionBuffer { get { return rtShadowGBuffer[1]; } }

	void Start()
	{
		DSRenderer dscam = cam.GetComponent<DSRenderer>();
		dscam.AddCallbackPreGBuffer(() => { PreGBuffer(); }, 800);
		dscam.AddCallbackPostGBuffer(() => { PostGBuffer(); }, 1000);

		kernelProcessColliders = csParticle.FindKernel("ProcessColliders");
		kernelProcessGBufferCollision = csParticle.FindKernel("ProcessGBufferCollision");
		kernelIntegrate = csParticle.FindKernel("Integrate");

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
		cbSphereColliders = new ComputeBuffer(MAX_SPHERE_COLLIDERS, 44);
		cbCapsuleColliders = new ComputeBuffer(MAX_CAPSULE_COLLIDERS, 56);
		cbBoxColliders = new ComputeBuffer(MAX_BOX_COLLIDERS, 136);

		csParticle.SetBuffer(kernelProcessColliders, "sphere_colliders", cbSphereColliders);
		csParticle.SetBuffer(kernelProcessColliders, "capsule_colliders", cbCapsuleColliders);
		csParticle.SetBuffer(kernelProcessColliders, "box_colliders", cbBoxColliders);
	}

	protected void OnDisable()
	{
		cbSphereColliders.Release();
		cbCapsuleColliders.Release();
		cbBoxColliders.Release();
		cbCubeVertices.Release();
	}

	void Update()
	{
		CSParticleSet.HandleParticleCollisionAll(this);

		CSParticleCollider.UpdateCSColliders();
		cbSphereColliders.SetData(CSParticleCollider.csSphereColliders.ToArray());
		cbCapsuleColliders.SetData(CSParticleCollider.csCapsuleColliders.ToArray());
		cbBoxColliders.SetData(CSParticleCollider.csBoxColliders.ToArray());

		prevColliders.Clear();
		prevColliders.AddRange(CSParticleCollider.instances);

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


		CSParticleSet.UpdateParticleSetAll(this);
	}

	void PreGBuffer()
	{
		CSParticleSet.CSDepthPrePassAll(this);
	}

	void PostGBuffer()
	{
		Camera c = cam.GetComponent<Camera>();
		DSRenderer dscam = cam.GetComponent<DSRenderer>();
		bool needs_shadow_gbufferb = false;
		for (int i = 0; i < CSParticleSet.instances.Count; ++i)
		{
			if (CSParticleSet.instances[i].processGBufferCollision)
			{
				needs_shadow_gbufferb = true;
				break;
			}
		}
		if (needs_shadow_gbufferb)
		{
			if (rtShadowGBuffer == null || rbShadowGBuffer == null)
			{
				rtShadowGBuffer = new RenderTexture[2];
				rbShadowGBuffer = new RenderBuffer[2];
				for (int i = 0; i < rtShadowGBuffer.Length; ++i)
				{
					rtShadowGBuffer[i] = DSRenderer.CreateRenderTexture((int)c.pixelWidth, (int)c.pixelHeight, 0, RenderTextureFormat.ARGBHalf);
					rbShadowGBuffer[i] = rtShadowGBuffer[i].colorBuffer;
				}
			}
			Graphics.SetRenderTarget(rbShadowGBuffer, rtShadowGBuffer[0].depthBuffer);
			matCopyGBuffer.SetTexture("_NormalBuffer", dscam.rtNormalBuffer);
			matCopyGBuffer.SetTexture("_PositionBuffer", dscam.rtPositionBuffer);
			matCopyGBuffer.SetPass(0);
			DSRenderer.DrawFullscreenQuad();
			dscam.SetRenderTargetsGBuffer();
			csParticle.SetTexture(kernelProcessGBufferCollision, "gbuffer_normal", rtShadowNormalBuffer);
			csParticle.SetTexture(kernelProcessGBufferCollision, "gbuffer_position", rtShadowPositionBuffer);
		}

		CSParticleSet.CSRenderAll(this);
	}
}
