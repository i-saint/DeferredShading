using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestShooter : MonoBehaviour
{
	public CSParticleSet enemyBullets;
	public CSParticleSet playerBullets;
	public CSParticleSet effectParticles;


	void Start()
	{
		enemyBullets.handler = (a, b) => { EnemyBulletHandler(a,b); };
	}
	
	void Update()
	{

		{
			const float posMin = -2.0f;
			const float posMax = 2.0f;
			const float velMin = -1.0f;
			const float velMax = 1.0f;
			CSParticle[] additional = new CSParticle[16];
			for (int i = 0; i < additional.Length; ++i)
			{
				additional[i].position = new Vector3(Random.Range(posMin, posMax), Random.Range(posMin, posMax) + 3.0f, Random.Range(posMin, posMax));
				additional[i].velocity = new Vector3(Random.Range(velMin, velMax), Random.Range(velMin, velMax), Random.Range(velMin, velMax));
				additional[i].owner_objid = 0;
				//particles[i].owner_objid = -1;
			}
			enemyBullets.AddParticles(additional);
		}
	}

	void EnemyBulletHandler(CSParticle[] particles, List<CSParticleCollider> colliders)
	{
		int max_particles = particles.Length;
		for (int i = 0; i < max_particles; ++i )
		{
			int hit = particles[i].hit_objid;
			if (particles[i].lifetime != 0.0f && hit != -1 && hit < colliders.Count)
			{
				CSParticleCollider cscol = colliders[hit];
				if (cscol != null && cscol.receiveCollision)
				{
					TSEntity tge = cscol.GetComponent<TSEntity>();
					if (tge)
					{
						tge.OnHitParticle(ref particles[i]);
					}
				}
				particles[i].lifetime = 0.0f;
			}
		}
	}
}
