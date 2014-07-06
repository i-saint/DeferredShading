using UnityEngine;
using System.Collections;

public class Scene : MonoBehaviour
{

	public GameObject cam;
	public GameObject cube;

	// Use this for initialization
	void Start ()
	{
		for (int xi = 0; xi < 15; ++xi )
		{
			for (int zi = 0; zi < 15; ++zi)
			{
				Instantiate(cube, new Vector3(1.1f*xi-7.7f, Random.Range(-2.0f, 0.0f)-0.7f, 1.1f*zi-7.7f), Quaternion.identity);
			}
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		float t = Time.time * 0.15f;
		float r = 5.5f;
		cam.transform.position = new Vector3(Mathf.Cos(t) * r, 4.0f, Mathf.Sin(t) * r);
		cam.transform.LookAt(Vector3.zero);
	}
}
