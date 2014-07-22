using UnityEngine;
using System.Collections;

public class Scene : MonoBehaviour
{

	public GameObject cam;
	public GameObject cube;

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
	
	void Update()
	{
		CameraControl();
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
