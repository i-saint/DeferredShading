using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;


public class TestCSParticle : MonoBehaviour
{
	public GameObject cam;
	public GameObject particleSet;
	CSParticleSet cspset;



	void Start ()
	{
		cspset = particleSet.GetComponent<CSParticleSet>();
	}

	void Update()
	{
		CameraControl();
		{
			CSParticle[] additional = new CSParticle[52];
			Vector3 center = new Vector3(0.0f, 4.0f, 0.0f);
			for (int i = 0; i < additional.Length; ++i)
			{
				Vector3 r = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
				additional[i].position = center + r * 0.5f;
				additional[i].velocity = r * 1.5f;
			}
			cspset.AddParticles(additional);
			cspset.csWorldData[0].decelerate = 0.9925f;
		}
	}

	void CameraControl()
	{
		Vector3 pos = Quaternion.Euler(0.0f, Time.deltaTime * -15.0f, 0) * cam.transform.position;
		if (Input.GetMouseButton(0))
		{
			float ry = Input.GetAxis("Mouse X") * 3.0f;
			float rxz = Input.GetAxis("Mouse Y") * 0.25f;
			pos = Quaternion.Euler(0.0f, ry, 0) * pos;
			pos.y += rxz;
		}
		{
			float wheel = Input.GetAxis("Mouse ScrollWheel");
			pos += pos.normalized * wheel * 4.0f;
		}
		cam.transform.position = pos;
		cam.transform.LookAt(new Vector3(0.0f, 1.0f, 0.0f));
	}
}
