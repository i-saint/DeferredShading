using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	Transform trans;
	Rigidbody rigid;
	Vector4 glowColor = new Vector4(0.1f, 0.075f, 0.2f, 0.0f);

	void Start()
	{
		trans = GetComponent<Transform>();
		rigid = GetComponent<Rigidbody>();
	}

	void Update()
	{
		MeshRenderer mr = GetComponent<MeshRenderer>();
		mr.material.SetVector("_GlowColor", glowColor);

		if (Input.GetButton("Fire1"))
		{
			Shot();
		}
		{
			Vector3 move = Vector3.zero;
			move.x = Input.GetAxis("Horizontal");
			move.y = Input.GetAxis("Vertical");
			rigid.velocity = move*5.0f;
		}
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			Plane plane = new Plane(new Vector3(0.0f,0.0f,1.0f), Vector3.zero);
			float distance = 0;
			if (plane.Raycast(ray, out distance))
			{
				trans.rotation = Quaternion.LookRotation(ray.GetPoint(distance) - trans.position);
			}
		}
	}

	void Shot()
	{
		TestShooter ts = TestShooter.instance;
		Vector3 pos = transform.position;
		Vector3 dir = transform.forward;
		CSParticle[] additional = new CSParticle[26];
		for (int i = 0; i < additional.Length; ++i)
		{
			additional[i].position = pos + dir*0.5f;
			additional[i].velocity = (dir + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0.0f)) * 10.0f;
		}
		ts.enemyBullets.AddParticles(additional);
		ts.enemyBullets.csWorldData[0].gravity = 0.0f;
		ts.enemyBullets.csWorldData[0].decelerate = 1.0f;
	}
}
