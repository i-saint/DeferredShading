using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;


public class TestMotionTrail : MonoBehaviour
{
	public DSRenderer cam;
	public DSMotionTrail trailObj;
	public bool showGUI;
	public bool rotateByTime = true;
	public int particlesParFrame = 52;
	ParticleSet cspset;



	void Start()
	{
	}

	void Update()
	{
		CameraControl();

		if (Input.GetKeyUp(KeyCode.Space))
		{
			showGUI = !showGUI;
		}
		//{
		//	CSParticle[] additional = new CSParticle[particlesParFrame];
		//	Vector3 center = new Vector3(0.0f, 4.0f, 0.0f);
		//	for (int i = 0; i < additional.Length; ++i)
		//	{
		//		Vector3 r = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
		//		additional[i].position = center + r * 0.5f;
		//		additional[i].velocity = r * 1.5f;
		//	}
		//	cspset.AddParticles(additional);
		//	cspset.csWorldData[0].decelerate = 0.9925f;
		//}
	}

	void CameraControl()
	{
		//Vector3 pos = Quaternion.Euler(0.0f, Time.deltaTime * -15.0f, 0) * cam.transform.position;
		Vector3 pos = cam.transform.position;
		if (Input.GetKeyUp(KeyCode.R)) { rotateByTime = !rotateByTime; }
		if (rotateByTime)
		{
			pos = Quaternion.Euler(0.0f, Time.deltaTime * -10.0f, 0) * pos;
		}
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
		cam.transform.LookAt(new Vector3(0.0f, 2.0f, 0.0f));
	}

	void OnGUI()
	{
		float lineheight = 22.0f;
		float margin = 0.0f;
		float labelWidth = 130.0f;
		float x = 10.0f;
		float y = 10.0f;

		if (!showGUI) { return; }

		GUI.Label(new Rect(x, y, labelWidth, lineheight), "trail delay frame:");
		GUI.TextField(new Rect(x + labelWidth, y, 50, lineheight), trailObj.delayFrame.ToString());
		trailObj.delayFrame = (int)GUI.HorizontalSlider(new Rect(x + labelWidth + 55, y, 100, lineheight), trailObj.delayFrame, 1, 20);
		y += lineheight + margin;

		y += 10.0f;

		GUI.Label(new Rect(x, y, 300, lineheight), "mouse drag & wheel: move camera");
		y += lineheight + margin;

		GUI.Label(new Rect(x, y, 300, lineheight), "space: show / hide GUI");
		y += lineheight + margin;
		GUI.Label(new Rect(x, y, 300, lineheight), "R: camera rotation on / off");
		y += lineheight + margin;
	}
}
