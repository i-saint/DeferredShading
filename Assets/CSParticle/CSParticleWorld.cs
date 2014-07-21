using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class CSParticleWorld : MonoBehaviour
{
	public const int MAX_SPHERE_COLLIDERS = 256;
	public const int MAX_CAPSULE_COLLIDERS = 256;
	public const int MAX_BOX_COLLIDERS = 256;

	public GameObject cam;
	public Material matCSParticle;
	public ComputeShader csParticle;

	public int kernelUpdateVelocity;
	public int kernelIntegrate;
	public ComputeBuffer cbSphereColliders;
	public ComputeBuffer cbCapsuleColliders;
	public ComputeBuffer cbBoxColliders;
	public ComputeBuffer cbCubeVertices;
	public ComputeBuffer cbCubeNormals;
	public ComputeBuffer cbCubeIndices;


	void Start()
	{
		DSRenderer dscam = cam.GetComponent<DSRenderer>();
		dscam.AddCallbackPostGBuffer(() => { RenderCSParticle(); });

		kernelUpdateVelocity = csParticle.FindKernel("UpdateVelocity");
		kernelIntegrate = csParticle.FindKernel("Integrate");

		cbCubeVertices = new ComputeBuffer(24, 12);
		cbCubeNormals = new ComputeBuffer(24, 12);
		cbCubeIndices = new ComputeBuffer(36, 4);
		{
			const float s = 0.05f;
			const float p = 1.0f;
			const float n = -1.0f;
			const float z = 0.0f;
			cbCubeVertices.SetData(new Vector3[24] {
				new Vector3(-s,-s, s), new Vector3( s,-s, s), new Vector3( s, s, s), new Vector3(-s, s, s),
				new Vector3(-s, s,-s), new Vector3( s, s,-s), new Vector3( s, s, s), new Vector3(-s, s, s),
				new Vector3(-s,-s,-s), new Vector3( s,-s,-s), new Vector3( s,-s, s), new Vector3(-s,-s, s),
				new Vector3(-s,-s, s), new Vector3(-s,-s,-s), new Vector3(-s, s,-s), new Vector3(-s, s, s),
				new Vector3( s,-s, s), new Vector3( s,-s,-s), new Vector3( s, s,-s), new Vector3( s, s, s),
				new Vector3(-s,-s,-s), new Vector3( s,-s,-s), new Vector3( s, s,-s), new Vector3(-s, s,-s),
			});
			cbCubeNormals.SetData(new Vector3[24] {
				new Vector3(z, z, p), new Vector3(z, z, p), new Vector3(z, z, p), new Vector3(z, z, p),
				new Vector3(z, p, z), new Vector3(z, p, z), new Vector3(z, p, z), new Vector3(z, p, z),
				new Vector3(z, n, z), new Vector3(z, n, z), new Vector3(z, n, z), new Vector3(z, n, z),
				new Vector3(n, z, z), new Vector3(n, z, z), new Vector3(n, z, z), new Vector3(n, z, z),
				new Vector3(p, z, z), new Vector3(p, z, z), new Vector3(p, z, z), new Vector3(p, z, z),
				new Vector3(z, z, n), new Vector3(z, z, n), new Vector3(z, z, n), new Vector3(z, z, n),
			});
			cbCubeIndices.SetData(new int[36] {
				0,1,3, 3,1,2,
				5,4,6, 6,4,7,
				8,9,11, 11,9,10,
				13,12,14, 14,12,15,
				16,17,19, 19,17,18,
				21,20,22, 22,20,23,
			});
		}

		cbSphereColliders = new ComputeBuffer(MAX_SPHERE_COLLIDERS, Marshal.SizeOf(typeof(CSSphereCollider)));
		cbCapsuleColliders = new ComputeBuffer(MAX_CAPSULE_COLLIDERS, Marshal.SizeOf(typeof(CSCapsuleCollider)));
		cbBoxColliders = new ComputeBuffer(MAX_BOX_COLLIDERS, Marshal.SizeOf(typeof(CSBoxCollider)));

		csParticle.SetBuffer(kernelUpdateVelocity, "sphere_colliders", cbSphereColliders);
		csParticle.SetBuffer(kernelUpdateVelocity, "capsule_colliders", cbCapsuleColliders);
		csParticle.SetBuffer(kernelUpdateVelocity, "box_colliders", cbBoxColliders);

		matCSParticle.SetBuffer("cubeVertices", cbCubeVertices);
		matCSParticle.SetBuffer("cubeNormals", cbCubeNormals);
		matCSParticle.SetBuffer("cubeIndices", cbCubeIndices);
	}

	protected void OnDisable()
	{
		cbSphereColliders.Release();
		cbCapsuleColliders.Release();
		cbBoxColliders.Release();
		cbCubeVertices.Release();
		cbCubeNormals.Release();
		cbCubeIndices.Release();
	}

	void Update()
	{
		CSParticleCollider.UpdateCSColliders();
		cbSphereColliders.SetData(CSParticleCollider.csSphereColliders.ToArray());
		cbCapsuleColliders.SetData(CSParticleCollider.csCapsuleColliders.ToArray());
		cbBoxColliders.SetData(CSParticleCollider.csBoxColliders.ToArray());

		CSParticleSet.UpdateParticleSetAll(this);
	}

	void RenderCSParticle()
	{
		CSParticleSet.RenderParticleSetAll(this);
	}
}
