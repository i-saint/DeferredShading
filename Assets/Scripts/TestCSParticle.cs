using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public struct Particle
{
	public Vector4 position;
	public Vector4 velocity;
}

public class TestCSParticle : MonoBehaviour
{
	private int kernelUpdateVelocity;
	private int kernelIntegrate;
	private ComputeBuffer cbParticles;
	private ComputeBuffer cbCubeVertices;
	private ComputeBuffer cbCubeNormals;
	private ComputeBuffer cbCubeIndices;

	public GameObject cam;
	public int numParticles = 1024;
	public Material matCSParticle;
	public ComputeShader csParticle;
	public Particle[] particles;



	void Start ()
	{
		DSRenderer dscam = cam.GetComponent<DSRenderer>();
		dscam.AddCallbackPostGBuffer(() => { UpdateCSParticle(); });
		dscam.AddCallbackPostGBuffer(() => { RenderCSParticle(); });

		particles = new Particle[numParticles];
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

		cbParticles = new ComputeBuffer(numParticles, 32);
		cbParticles.SetData(particles);
		csParticle.SetBuffer(kernelUpdateVelocity, "particles", cbParticles);
		csParticle.SetTexture(kernelUpdateVelocity, "positionBuffer", dscam.rtPositionBuffer);
		csParticle.SetTexture(kernelUpdateVelocity, "normalBuffer", dscam.rtNormalBuffer);
		csParticle.SetBuffer(kernelIntegrate, "particles", cbParticles);
		matCSParticle.SetBuffer("particles", cbParticles);
		matCSParticle.SetBuffer("cubeVertices", cbCubeVertices);
		matCSParticle.SetBuffer("cubeNormals", cbCubeNormals);
		matCSParticle.SetBuffer("cubeIndices", cbCubeIndices);
	}

	protected void OnDisable()
	{
		cbParticles.Release();
		cbCubeVertices.Release();
		cbCubeNormals.Release();
		cbCubeIndices.Release();
	}

	void UpdateCSParticle()
	{
		csParticle.Dispatch(kernelUpdateVelocity, numParticles / 1024, 1, 1);
		csParticle.Dispatch(kernelIntegrate, numParticles / 1024, 1, 1);
	}

	void RenderCSParticle()
	{
		if (!SystemInfo.supportsInstancing)
		{
			return;
		}

		matCSParticle.SetPass(0);
		Graphics.DrawProcedural(MeshTopology.Triangles, 36, numParticles);
	}


	void Update()
	{
		{
			float t = Time.time * 0.2f;
			float r = 10.0f;
			cam.transform.position = new Vector3(Mathf.Cos(t) * r, 4.0f, Mathf.Sin(t) * r);
			cam.transform.LookAt(new Vector3(0.0f, 1.0f, 0.0f));
		}
	}
}
