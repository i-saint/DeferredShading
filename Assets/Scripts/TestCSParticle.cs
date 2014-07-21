using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public struct CSParticle
{
	public Vector4 position;
	public Vector4 velocity;
};

public struct CSWorldData
{
	public float timestep;
	public float particle_size;
	public float wall_stiffness;
	public float decelerate;
	public float gravity;
	public int num_particles;
	public int num_sphere_colliders;
	public int num_plane_colliders;
	public int num_box_colliders;
	public int num_forces;

	public void SetDefaultValues()
	{
		timestep = 0.01f;
		particle_size = 0.0f;
		wall_stiffness = 100.0f;
		decelerate = 0.999f;
		gravity = 7.0f;
		num_particles = 0;
		num_sphere_colliders = 0;
		num_plane_colliders = 0;
		num_box_colliders = 0;
		num_forces = 0;
	}
};
public struct CSSphereCollider
{
	public Vector3 center;
	public float radius;
};


public struct CSPlane
{
	public Vector3 normal;
	public float distance;
}

//public unsafe struct CSBoxCollider
//{
//	public fixed CSPlane planes[6];
//}

struct CSPlaneCollider
{
	public Vector3 normal;
	public float distance;
};



public class TestCSParticle : MonoBehaviour
{
	public const int MAX_SPHERE_COLLIDERS = 256;
	public const int MAX_PLANE_COLLIDERS = 256;
	public const int MAX_BOX_COLLIDERS = 256;

	private int kernelUpdateVelocity;
	private int kernelIntegrate;
	private ComputeBuffer cbWorldData;
	private ComputeBuffer cbSphereColliders;
	private ComputeBuffer cbPlaneColliders;
	private ComputeBuffer cbBoxColliders;
	private ComputeBuffer cbParticles;
	private ComputeBuffer cbCubeVertices;
	private ComputeBuffer cbCubeNormals;
	private ComputeBuffer cbCubeIndices;

	public GameObject colSphere;
	public GameObject cam;
	public int numParticles = 65536;
	public Material matCSParticle;
	public ComputeShader csParticle;
	public CSParticle[] particles;
	CSWorldData[] csWorldData = new CSWorldData[1];
	List<CSSphereCollider> csSphereColliders = new List<CSSphereCollider>();
	List<CSPlaneCollider> csPlaneColliders = new List<CSPlaneCollider>();



	void Start ()
	{
		DSRenderer dscam = cam.GetComponent<DSRenderer>();
		dscam.AddCallbackPostGBuffer(() => { RenderCSParticle(); });

		particles = new CSParticle[numParticles];
		{
			const float posMin = -2.0f;
			const float posMax = 2.0f;
			const float velMin = -1.0f;
			const float velMax = 1.0f;
			for (int i = 0; i < particles.Length; ++i )
			{
				particles[i].position = new Vector4(Random.Range(posMin, posMax), Random.Range(posMin, posMax) + 3.0f, Random.Range(posMin, posMax), 0.0f);
				particles[i].velocity = new Vector4(Random.Range(velMin, velMax), Random.Range(velMin, velMax), Random.Range(velMin, velMax), 0.0f);
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

		cbParticles = new ComputeBuffer(numParticles, Marshal.SizeOf(typeof(CSParticle)));
		cbParticles.SetData(particles);

		cbWorldData = new ComputeBuffer(1, Marshal.SizeOf(typeof(CSWorldData)));
		cbSphereColliders = new ComputeBuffer(MAX_SPHERE_COLLIDERS, Marshal.SizeOf(typeof(CSSphereCollider)));
		cbPlaneColliders = new ComputeBuffer(MAX_PLANE_COLLIDERS, Marshal.SizeOf(typeof(CSPlaneCollider)));
		//cbBoxColliders = new ComputeBuffer(MAX_BOX_COLLIDERS, Marshal.SizeOf(typeof(CSBoxCollider)));

		csParticle.SetBuffer(kernelUpdateVelocity, "world_data", cbWorldData);
		csParticle.SetBuffer(kernelUpdateVelocity, "particles", cbParticles);
		csParticle.SetBuffer(kernelUpdateVelocity, "sphere_colliders", cbSphereColliders);
		csParticle.SetBuffer(kernelUpdateVelocity, "plane_colliders", cbPlaneColliders);
		csParticle.SetTexture(kernelUpdateVelocity, "gbuffer_position", dscam.rtPositionBuffer);
		csParticle.SetTexture(kernelUpdateVelocity, "gbuffer_normal", dscam.rtNormalBuffer);

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
		if (csSphereColliders.Count == 0)
		{
			CSSphereCollider col = new CSSphereCollider();
			col.center = new Vector3(0.0f, 0.0f, 0.0f);
			col.radius = 1.5f;
			csSphereColliders.Add(col);
		}
		{
			CSSphereCollider col = new CSSphereCollider();
			col.center = colSphere.transform.position;
			col.radius = colSphere.transform.localScale.x;
			csSphereColliders[0] = col;
		}

		csWorldData[0].num_sphere_colliders = csSphereColliders.Count;
		cbWorldData.SetData(csWorldData);
		cbSphereColliders.SetData(csSphereColliders.ToArray());

		csParticle.Dispatch(kernelUpdateVelocity, numParticles / 1024, 1, 1);
		csParticle.Dispatch(kernelIntegrate, numParticles / 1024, 1, 1);
	}

	protected void OnDisable()
	{
		cbWorldData.Release();
		cbParticles.Release();
		cbSphereColliders.Release();
		cbPlaneColliders.Release();
		//cbBoxColliders.Release();
		cbCubeVertices.Release();
		cbCubeNormals.Release();
		cbCubeIndices.Release();
	}

	void RenderCSParticle()
	{
		//if (!SystemInfo.supportsInstancing)
		//{
		//	return;
		//}

		matCSParticle.SetPass(0);
		Graphics.DrawProcedural(MeshTopology.Triangles, 36, numParticles);
	}
}
