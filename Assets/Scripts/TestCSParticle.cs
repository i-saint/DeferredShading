using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;


public class TestCSParticle : MonoBehaviour
{
	public const int MAX_PARTICLES = 65536;
	public const int MAX_SPHERE_COLLIDERS = 256;
	public const int MAX_CAPSULE_COLLIDERS = 256;
	public const int MAX_BOX_COLLIDERS = 256;

	private int kernelUpdateVelocity;
	private int kernelIntegrate;
	private ComputeBuffer cbWorldData;
	private ComputeBuffer cbSphereColliders;
	private ComputeBuffer cbCapsuleColliders;
	private ComputeBuffer cbBoxColliders;
	private ComputeBuffer cbParticles;
	private ComputeBuffer cbCubeVertices;
	private ComputeBuffer cbCubeNormals;
	private ComputeBuffer cbCubeIndices;

	public GameObject colSphere;
	public GameObject cam;
	public Material matCSParticle;
	public ComputeShader csParticle;
	public CSParticle[] particles;
	CSWorldData[] csWorldData = new CSWorldData[1];



	void Start ()
	{
		DSRenderer dscam = cam.GetComponent<DSRenderer>();
		dscam.AddCallbackPostGBuffer(() => { RenderCSParticle(); });

		particles = new CSParticle[MAX_PARTICLES];
		{
			const float posMin = -2.0f;
			const float posMax = 2.0f;
			const float velMin = -1.0f;
			const float velMax = 1.0f;
			for (int i = 0; i < particles.Length; ++i )
			{
				particles[i].position = new Vector3(Random.Range(posMin, posMax), Random.Range(posMin, posMax) + 3.0f, Random.Range(posMin, posMax));
				particles[i].velocity = new Vector3(Random.Range(velMin, velMax), Random.Range(velMin, velMax), Random.Range(velMin, velMax));
				particles[i].owner_objid = 0;
			}
		}

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
		csWorldData[0].SetDefaultValues();

		cbParticles = new ComputeBuffer(MAX_PARTICLES, Marshal.SizeOf(typeof(CSParticle)));
		cbParticles.SetData(particles);

		cbWorldData = new ComputeBuffer(1, Marshal.SizeOf(typeof(CSWorldData)));
		cbSphereColliders = new ComputeBuffer(MAX_SPHERE_COLLIDERS, Marshal.SizeOf(typeof(CSSphereCollider)));
		cbCapsuleColliders = new ComputeBuffer(MAX_CAPSULE_COLLIDERS, Marshal.SizeOf(typeof(CSCapsuleCollider)));
		cbBoxColliders = new ComputeBuffer(MAX_BOX_COLLIDERS, Marshal.SizeOf(typeof(CSBoxCollider)));

		csParticle.SetBuffer(kernelUpdateVelocity, "world_data", cbWorldData);
		csParticle.SetBuffer(kernelUpdateVelocity, "particles", cbParticles);
		csParticle.SetBuffer(kernelUpdateVelocity, "sphere_colliders", cbSphereColliders);
		csParticle.SetBuffer(kernelUpdateVelocity, "capsule_colliders", cbCapsuleColliders);
		csParticle.SetBuffer(kernelUpdateVelocity, "box_colliders", cbBoxColliders);
		//csParticle.SetTexture(kernelUpdateVelocity, "gbuffer_position", dscam.rtPositionBuffer);
		//csParticle.SetTexture(kernelUpdateVelocity, "gbuffer_normal", dscam.rtNormalBuffer);

		csParticle.SetBuffer(kernelIntegrate, "world_data", cbWorldData);
		csParticle.SetBuffer(kernelIntegrate, "particles", cbParticles);

		matCSParticle.SetBuffer("particles", cbParticles);
		matCSParticle.SetBuffer("cubeVertices", cbCubeVertices);
		matCSParticle.SetBuffer("cubeNormals", cbCubeNormals);
		matCSParticle.SetBuffer("cubeIndices", cbCubeIndices);
	}

	void Update()
	{
		{
			float t = Time.time * 0.2f;
			float r = 10.0f;
			cam.transform.position = new Vector3(Mathf.Cos(t) * r, 4.0f, Mathf.Sin(t) * r);
			cam.transform.LookAt(new Vector3(0.0f, 1.0f, 0.0f));
		}

		CSCollider.UpdateCSColliders();
		cbSphereColliders.SetData(CSCollider.csSphereColliders.ToArray());
		cbCapsuleColliders.SetData(CSCollider.csCapsuleColliders.ToArray());
		cbBoxColliders.SetData(CSCollider.csBoxColliders.ToArray());
		csWorldData[0].num_sphere_colliders = CSCollider.csSphereColliders.Count;
		csWorldData[0].num_capsule_colliders = CSCollider.csCapsuleColliders.Count;
		csWorldData[0].num_box_colliders = CSCollider.csBoxColliders.Count;
		csWorldData[0].world_center = transform.position;
		csWorldData[0].world_extent = transform.localScale;
		cbWorldData.SetData(csWorldData);

		csParticle.Dispatch(kernelUpdateVelocity, MAX_PARTICLES / 1024, 1, 1);
		csParticle.Dispatch(kernelIntegrate, MAX_PARTICLES / 1024, 1, 1);
	}

	protected void OnDisable()
	{
		cbWorldData.Release();
		cbParticles.Release();
		cbSphereColliders.Release();
		cbCapsuleColliders.Release();
		cbBoxColliders.Release();
		cbCubeVertices.Release();
		cbCubeNormals.Release();
		cbCubeIndices.Release();
	}

	void RenderCSParticle()
	{
		matCSParticle.SetPass(0);
		Graphics.DrawProcedural(MeshTopology.Triangles, 36, MAX_PARTICLES);
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(transform.position, transform.localScale * 2.0f);
	}
}
