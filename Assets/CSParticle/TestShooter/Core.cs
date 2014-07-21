using UnityEngine;
using System.Collections;

public class Core : MonoBehaviour
{
	Vector4 glowColor = new Vector4(0.2f, 0.0f, 0.0f, 0.0f);

	void Start ()
	{
	
	}
	
	void Update()
	{
		MeshRenderer mr = GetComponent<MeshRenderer>();
		mr.material.SetVector("_GlowColor", glowColor);

		Vector3 pos = transform.position;
		pos.x = 10.0f;
		pos.y = Mathf.Cos(Time.time*0.5f) * 3.0f;
		transform.position = pos;

		TestShooter ts = TestShooter.instance;
		{
			CSParticle[] additional = new CSParticle[52];
			for (int i = 0; i < additional.Length; ++i)
			{
				additional[i].position = new Vector3(pos.x - 1.4f, pos.y, 0.0f);
				additional[i].velocity = new Vector3(Random.Range(-2.0f, -0.5f), Random.Range(-1.0f, 1.0f), 0.0f) * 4.0f;
			}
			ts.enemyBullets.AddParticles(additional);
			ts.enemyBullets.csWorldData[0].gravity = 0.0f;
			ts.enemyBullets.csWorldData[0].decelerate = 1.0f;
		}
	}
}
