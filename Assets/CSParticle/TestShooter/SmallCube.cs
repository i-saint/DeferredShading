using UnityEngine;
using System.Collections;

public class SmallCube : MonoBehaviour
{
	void OnDestroy()
	{
		TestShooter ts = TestShooter.instance;
		if (!ts) { return; }

		Vector3 pos = transform.position;

		CSParticle[] bullets = new CSParticle[512];
		for (int i = 0; i < bullets.Length; ++i)
		{
			bullets[i].position = new Vector3(pos.x, pos.y, 0.0f);
			bullets[i].velocity = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0.0f) * 8.0f;
		}
		ts.enemyBullets.AddParticles(bullets);
	}
}
