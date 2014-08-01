using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestShooter : MonoBehaviour
{
	public static TestShooter instance;

	public ParticleSet enemyBullets;
	public ParticleSet playerBullets;
	public ParticleSet effectParticles;

	public GameObject cam;
	public GameObject bgCube;
	public GameObject enemySmallCube;
	public GameObject enemyMediumCube;
	public GameObject enemyLargeCube;
	public GameObject enemyCore;

	int frame = 0;

	void OnEnable()
	{
		instance = this;
	}

	void OnDisable()
	{
		instance = null;
	}

	void Start()
	{
		enemyBullets.handler = (a, b) => { EnemyBulletHandler(a,b); };
		cam.transform.position = new Vector3(-0.5f, -0.5f, -10.0f);
		cam.transform.LookAt(Vector3.zero);

		for (int yi = 0; yi < 15; ++yi)
		{
			for (int xi = 0; xi < 15; ++xi)
			{
				GameObject c = (GameObject)Instantiate(bgCube,
					new Vector3(2.2f * xi - 15.4f, 2.2f * yi - 15.4f, Random.Range(3.0f, 5.0f) - 1.0f), Quaternion.identity);
				c.transform.localScale = Vector3.one * 2.0f;
			}
		}
	}
	
	void Update()
	{
		++frame;

		if (frame % 500 == 0)
		{
			Vector3 pos = new Vector3(Random.Range(-15.0f, -29.0f), Random.Range(-5.0f, 5.0f), 0.0f);
			Instantiate(enemyLargeCube, pos, Quaternion.identity);
		}
		if (frame % 200 == 0)
		{
			Vector3 pos = new Vector3(Random.Range(-18.0f, -29.0f), Random.Range(-6.0f, 6.0f), 0.0f);
			Instantiate(enemyMediumCube, pos, Quaternion.identity);
		}
		if (frame % 30 == 0)
		{
			Vector3 pos = new Vector3(Random.Range(-18.0f, -29.0f), Random.Range(-6.0f, 6.0f), 0.0f);
			Instantiate(enemySmallCube, pos, Quaternion.identity);
		}
	}

	void EnemyBulletHandler(CSParticle[] particles, List<ParticleCollider> colliders)
	{
		int max_particles = particles.Length;
		for (int i = 0; i < max_particles; ++i )
		{
			int hit = particles[i].hit_objid;
			if (particles[i].lifetime != 0.0f && hit != -1 && hit < colliders.Count)
			{
				ParticleCollider cscol = colliders[hit];
				if (cscol != null && cscol.receiveCollision)
				{
					TSEntity tge = cscol.GetComponent<TSEntity>();
					if (tge)
					{
						tge.OnHitParticle(ref particles[i]);
					}
				}
				particles[i].lifetime = 0.0f;
				particles[i].hit_objid = -1;
			}
		}
	}
}
