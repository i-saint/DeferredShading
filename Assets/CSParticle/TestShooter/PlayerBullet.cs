using UnityEngine;
using System.Collections;

public class PlayerBullet : MonoBehaviour
{
	Vector4 glowColor = new Vector4(0.2f, 0.2f, 0.4f, 0.0f);

	public float speed = 1.0f;
	public float power = 30.0f;

	void Start()
	{
	}

	void Update()
	{
		MeshRenderer mr = GetComponent<MeshRenderer>();
		mr.material.SetVector("_GlowColor", glowColor);
		if (Mathf.Abs(transform.position.x) > 20.0f ||
		   Mathf.Abs(transform.position.z) > 20.0f)
		{
			Destroy(gameObject);
		}
		transform.position += transform.forward.normalized * speed;
	}
}
