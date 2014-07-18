using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestAnd : MonoBehaviour {

	public GameObject locube;
	public GameObject cube;
	public GameObject cam;
	public GameObject bg;
	public GameObject sphereCore;
	public GameObject sphereAnd;
	public List<GameObject> subVertical;

	void Start ()
	{
		subVertical = new List<GameObject>();
		for (int iy = 0; iy < 10; ++iy)
		{
			for (int iz = 0; iz < 5; ++iz)
			{
				for (int ix = 0; ix < 5; ++ix)
				{
					const float d = 1.0f;
					GameObject c = (GameObject)Instantiate(locube,
						new Vector3(d * ix - d * 2, d * iy - d * 5, d * iz - d * 2), Quaternion.identity);
					float r = 0.75f;
					c.transform.localScale = new Vector4(r, r, r);
					subVertical.Add(c);
				}
			}
		}

		for (int xi = 0; xi < 1000; ++xi)
		{
			Vector3 rv = Vector3.Normalize(new Vector3(Random.Range(-1.0f, 1.0f), 0.0f, Random.Range(-1.0f, 1.0f)));
			rv.y = Random.Range(-2.0f, 1.0f);
			GameObject c = (GameObject)Instantiate(cube, rv * Random.Range(6.0f, 8.0f), Quaternion.identity);
			c.isStatic = true;
			c.transform.parent = bg.transform;
		}
	}
	
	void Update ()
	{
		{
			float t = Time.time * 0.5f;
			float r = 5.0f;
			cam.transform.position = new Vector3(Mathf.Cos(t) * r, 3.5f, Mathf.Sin(t) * r);
			cam.transform.LookAt(new Vector3(0.0f, 1.0f, 0.0f));

			sphereAnd.transform.localScale = Vector3.one * (Mathf.Cos(Time.time*2.0f) + 4.0f);
		}

		foreach(var neg in subVertical) {
			Vector3 pos = neg.transform.position;
			pos.y -= 0.030f;
			if (pos.y < -5.0f) { pos.y += 10.0f; }
			neg.transform.position = pos;
		}
	}
}
