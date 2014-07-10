using UnityEngine;
using System.Collections;

public class TestSubtraction : MonoBehaviour {

	public GameObject cube;
	public GameObject cam;
	public GameObject negCube;
	public GameObject negSphere;

	void Start ()
	{
		for (int xi = 0; xi < 15; ++xi)
		{
			for (int zi = 0; zi < 15; ++zi)
			{
				Instantiate(cube, new Vector3(1.1f * xi - 7.7f, Random.Range(-0.75f, 0.0f) - 1.0f, 1.1f * zi - 7.7f), Quaternion.identity);
			}
		}
		//negCubes[0].transform.position = new Vector3(0.6f, 1.2f, 2.0f);
		//negCubes[1].transform.position = new Vector3(0.6f, 2.0f, 0.0f);
		//negSpheres[0].transform.position = new Vector3(2.0f, 0.5f, 2.8f);
		//negSpheres[1].transform.position = new Vector3(2.0f, 0.5f, 0.8f);
	}
	
	void Update ()
	{
		{
			float t = Mathf.Sin(Time.time * 0.3f)*0.4f + 30.0f*Mathf.Deg2Rad;
			float r = 5.0f;
			cam.transform.position = new Vector3(Mathf.Cos(t) * r, 3.0f, Mathf.Sin(t) * r);
			cam.transform.LookAt(Vector3.zero);
		}

		{
			Vector3 pos = negCube.transform.position;
			pos.y = Mathf.Cos(Time.time*0.7f) * 0.8f + 1.0f;
			negCube.transform.position = pos;
		}

		{
			Vector3 pos = negSphere.transform.position;
			pos.z = Mathf.Cos(Time.time * 0.5f) * 1.0f - 1.0f;
			negSphere.transform.position = pos;
		}
	}
}
