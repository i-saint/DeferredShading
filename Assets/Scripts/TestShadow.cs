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
		CameraControl();
		{
			float t = Time.time * 0.75f;
			float r = 1.5f;
			dslight.transform.position = new Vector3(0.0f, Mathf.Sin(t) * 0.25f + 1.0f, Mathf.Cos(t * 0.75f) * r);
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
		cam.transform.LookAt(new Vector3(0.0f, 0.0f, 0.0f));
	}
}
