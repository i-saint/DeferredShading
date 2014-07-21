using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;


public class TestCSParticle : MonoBehaviour
{

	public GameObject cam;



	void Start ()
	{

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
