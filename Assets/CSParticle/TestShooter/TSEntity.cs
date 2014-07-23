using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TSEntity : MonoBehaviour
{
	enum EntityType {
		Player,
		Enemy,
		Ground,
		Other,
	}

	public delegate void Callback();
	public static Vector4 damageFlashColor = new Vector4(0.10f, 0.025f, 0.02f, 0.0f);
	Material matBase;
	public Rigidbody rigid;
	public Transform trans;
	public int frame = 0;
	public float life = 100.0f;
	float deltaDamage;
	public float accel = 0.02f;
	public float deccel = 0.99f;
	public float maxSpeed = 5.0f;
	public Callback cbDestroyed;

	void Start()
	{
		rigid = GetComponent<Rigidbody>();
		trans = GetComponent<Transform>();
		MeshRenderer mr = GetComponent<MeshRenderer>();
		matBase = new Material(mr.material.shader);
		mr.material = matBase;
	}
	
	void Update()
	{
		++frame;

		if (rigid)
		{
			Vector3 vel = rigid.velocity;
			vel.x += accel;
			rigid.velocity = vel;

			Vector3 pos = rigid.transform.position;
			pos.z *= 0.98f;
			rigid.transform.position = pos;

			float speed = rigid.velocity.magnitude;
			rigid.velocity = rigid.velocity.normalized * (Mathf.Min(speed, maxSpeed) * deccel);

			rigid.angularVelocity *= 0.95f;
		}
		
		if (deltaDamage > 0.0f && frame % 4 < 2)
		{
			matBase.SetVector("_GlowColor", damageFlashColor);
		}
		else
		{
			matBase.SetVector("_GlowColor", Vector4.zero);
		}

		life -= deltaDamage;
		deltaDamage = 0.0f;
		if (life <= 0.0f)
		{
			Destroy(gameObject);
			if (cbDestroyed != null) { cbDestroyed.Invoke(); }
		}
		if (Mathf.Abs(trans.position.x) > 30.0f || Mathf.Abs(trans.position.z) > 30.0f)
		{
			Destroy(gameObject);
		}
	}

	public void OnDamage(float damage, int damageFrom)
	{
		deltaDamage += damage;
	}

	public void OnHitParticle(ref CSParticle particle)
	{
		OnDamage(0.15f, particle.owner_objid);
	}
}
