using UnityEngine;
using System.Collections;

public class SmallCube : MonoBehaviour
{
	void Start()
	{
		GetComponent<TSEntity>().cbDestroyed = () => { CBDestroy(); };
	}

	static float R(float r=1.0f)
	{
		return Random.Range(-r, r);
	}

	void CBDestroy()
	{
		TestShooter ts = TestShooter.instance;
		if (!ts) { return; }

		Vector3 pos = transform.position;

		CSParticle[] bullets = new CSParticle[512];
		for (int i = 0; i < bullets.Length; ++i)
		{
			bullets[i].position = new Vector3(pos.x + R(0.3f), pos.y + R(0.3f), R(0.3f));
			bullets[i].velocity = new Vector3(R(), R(), 0.0f) * 8.0f;
		}
		ts.enemyBullets.AddParticles(bullets);
	}
}
