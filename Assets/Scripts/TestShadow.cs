using UnityEngine;
using System.Collections;

public class TestShadow : MonoBehaviour {

	public GameObject dslight;
	public GameObject cube;
	public GameObject cam;

	void Start ()
	{
		for (int xi = 0; xi < 15; ++xi)
		{
			for (int zi = 0; zi < 15; ++zi)
			{
				Instantiate(cube, new Vector3(1.1f * xi - 7.7f, Random.Range(-0.75f, 0.0f) - 1.0f, 1.1f * zi - 7.7f), Quaternion.identity);
			}
		}
	}
	
	void Update ()
	{
		{
			float t = Time.time * 0.75f;
			float r = 1.5f;
			dslight.transform.position = new Vector3(0.0f, Mathf.Sin(t) * 0.25f + 1.0f, Mathf.Cos(t * 0.75f) * r);
		}
		{
			float t = Time.time * 0.15f;
			float r = 5.0f;
			cam.transform.position = new Vector3(Mathf.Cos(t) * r, 3.5f, Mathf.Sin(t) * r);
			cam.transform.LookAt(Vector3.zero);
		}
	}
}
