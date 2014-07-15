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
		for (int i = 0; i < 50; ++i )
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
		{
			float t = Time.time * 0.2f;
			float r = 5.0f;
			cam.transform.position = new Vector3(Mathf.Cos(t) * r, 3.5f, Mathf.Sin(t) * r);
			cam.transform.LookAt(new Vector3(0.0f, 1.0f, 0.0f));
		}

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
}
