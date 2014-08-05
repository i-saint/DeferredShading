using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	Transform trans;
	Rigidbody rigid;
	Vector4 glowColor = new Vector4(0.1f, 0.075f, 0.2f, 0.0f);
	public GameObject playerBullet;
	public bool canBlow = true;
	Matrix4x4 blowMatrix;

	void Start()
	{
		trans = GetComponent<Transform>();
		rigid = GetComponent<Rigidbody>();
	}

	void Update()
	{
		TestShooter ts = TestShooter.instance;
		ts.enemyBullets.csWorldData[0].gravity = 0.0f;
		ts.enemyBullets.csWorldData[0].coord_scaler = new Vector3(1.0f, 1.0f, 0.9f);
		if (!canBlow) {
			ts.enemyBullets.csWorldData[0].decelerate = 1.0f;
		}


		MeshRenderer mr = GetComponent<MeshRenderer>();
		mr.material.SetVector("_GlowColor", glowColor);

		if (Input.GetButton("Fire1"))
		{
			Shot();
		}
		if (Input.GetButtonDown("Fire2"))
		{
			Blow();
		}
		{
			Matrix4x4 bt = Matrix4x4.identity;
			bt.SetColumn(3, new Vector4(0.0f, 0.0f, 0.5f, 1.0f));
			bt = Matrix4x4.Scale(new Vector3(5.0f, 5.0f, 10.0f)) * bt;
			blowMatrix = trans.localToWorldMatrix * bt;
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
		if (canBlow)
		{
			Instantiate(playerBullet, trans.position + trans.forward.normalized * 1.0f, trans.rotation);
		}
		else
		{
			TestShooter ts = TestShooter.instance;
			Vector3 pos = transform.position;
			Vector3 dir = transform.forward;
			CSParticle[] additional = new CSParticle[26];
			for (int i = 0; i < additional.Length; ++i)
			{
				additional[i].position = pos + dir * 0.5f;
				additional[i].velocity = (dir + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0.0f)) * 10.0f;
			}
			ts.enemyBullets.AddParticles(additional);
		}
	}

	void Blow()
	{
		//Vector3 pos = trans.position;
		//float strength = 2000.0f;

		//fprops.SetDefaultValues();
		//fprops.shape_type = MPForceShape.Box;
		//fprops.dir_type = MPForceDirection.Radial;
		//fprops.strength_near = strength;
		//fprops.strength_far = strength;
		//fprops.radial_center = pos - (trans.forward * 6.0f);
		//MPAPI.mpAddForce(ref fprops, ref blowMatrix);
	}

	void OnDrawGizmos()
	{
		if (canBlow)
		{
			Color blue = Color.blue;
			blue.a = 0.25f;
			Gizmos.color = blue;
			Gizmos.matrix = blowMatrix * Matrix4x4.Scale(new Vector3(0.0f, 1.0f, 1.0f));
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		}
	}
}
