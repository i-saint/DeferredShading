using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestSubtraction2 : MonoBehaviour {

	public GameObject cube;
	public GameObject cam;
	public GameObject negCube;
	public GameObject negSphere;
	public List<GameObject> subVertical;
	public List<GameObject> subHorizontal;

	void Start ()
	{
		subVertical = new List<GameObject>();
		subHorizontal = new List<GameObject>();
		for (int i = 0; i < 50; ++i)
		{
			GameObject c = (GameObject)Instantiate(negSphere,
				new Vector3(Random.Range(-3.0f, 3.0f), Random.Range(-3.0f, 3.0f), Random.Range(-3.0f, 3.0f)), Quaternion.identity);
			float r = 1.25f + Random.Range(0.0f, 0.5f);
			c.transform.localScale = new Vector4(r, r, r);
			subHorizontal.Add(c);
		}
		for (int i = 0; i < 10; ++i)
		{
			GameObject c = (GameObject)Instantiate(negCube,
				new Vector3(Random.Range(-4.0f, 4.0f), Random.Range(-4.0f, 4.0f), Random.Range(-4.0f, 4.0f)), Quaternion.identity);
			float r = 1.0f + Random.Range(0.0f, 0.5f);
			c.transform.localScale = new Vector4(r, r, r);
			subVertical.Add(c);
		}

		for (int xi = 0; xi < 1000; ++xi)
		{
			Vector3 rv = Vector3.Normalize(new Vector3(Random.Range(-1.0f, 1.0f), 0.0f, Random.Range(-1.0f, 1.0f)));
			rv.y = Random.Range(-2.0f, 1.0f);
			GameObject c = (GameObject)Instantiate(cube, rv * Random.Range(6.0f, 8.0f), Quaternion.identity);
			c.isStatic = true;
		}
	}
	
	void Update ()
	{
		CameraControl();

		int i = 0;
		foreach(var neg in subVertical) {
			Vector3 pos = neg.transform.position;
			pos.y -= 0.010f + 0.001f*i++;
			if (pos.y < -5.0f) { pos.y += 10.0f; }
			neg.transform.position = pos;
		}
		i = 0;
		foreach (var neg in subHorizontal)
		{
			Vector3 pos = neg.transform.position;
			pos.z += 0.010f + 0.001f * i++;
			if (pos.z > 3.0f) { pos.z -= 7.0f; }
			neg.transform.position = pos;
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
