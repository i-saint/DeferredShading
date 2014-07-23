using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;


public class TestCSParticle : MonoBehaviour
{
	public GameObject cam;
	public GameObject particleSet;
	public GameObject capsule;
	public GameObject sphere;
	public GameObject floor;
	public bool showGUI;
	public int particlesParFrame = 52;
	CSParticleSet cspset;



	void Start ()
	{
		cspset = particleSet.GetComponent<CSParticleSet>();
	}

	void Update()
	{
		CameraControl();

		if (Input.GetKeyUp(KeyCode.Space))
		{
			showGUI = !showGUI;
		}
		{
			CSParticle[] additional = new CSParticle[particlesParFrame];
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

	void OnGUI()
	{
		float lineheight = 22.0f;
		float margin = 0.0f;
		float labelWidth = 130.0f;
		float x = 10.0f;
		float y = 10.0f;
		Vector3 sphpos = sphere.transform.position;
		Vector3 cappos = capsule.transform.position;
		Vector3 caprot = capsule.transform.rotation.eulerAngles;

		if (!showGUI) { return; }


		GUI.Label(new Rect(x, y, labelWidth, lineheight), "particles par frame:");
		GUI.TextField(new Rect(x + labelWidth, y, 50, lineheight), particlesParFrame.ToString());
		particlesParFrame = (int)GUI.HorizontalSlider(new Rect(x + labelWidth + 55, y, 100, lineheight), particlesParFrame, 0, 500);
		y += lineheight + margin;

		y += 10.0f;

		GUI.Label(new Rect(x, y, labelWidth, lineheight), "cylinder rotation y:");
		GUI.TextField(new Rect(x + labelWidth, y, 50, lineheight), caprot.y.ToString());
		caprot.y = (float)GUI.HorizontalSlider(new Rect(x + labelWidth + 55, y, 100, lineheight), caprot.y, 0, 360);
		y += lineheight + margin;

		GUI.Label(new Rect(x, y, labelWidth, lineheight), "cylinder position x:");
		GUI.TextField(new Rect(x + labelWidth, y, 50, lineheight), cappos.x.ToString());
		cappos.x = (float)GUI.HorizontalSlider(new Rect(x + labelWidth + 55, y, 100, lineheight), cappos.x, -5, 5);
		y += lineheight + margin;

		GUI.Label(new Rect(x, y, labelWidth, lineheight), "cylinder position y:");
		GUI.TextField(new Rect(x + labelWidth, y, 50, lineheight), cappos.y.ToString());
		cappos.y = (float)GUI.HorizontalSlider(new Rect(x + labelWidth + 55, y, 100, lineheight), cappos.y, -5, 5);
		y += lineheight + margin;

		GUI.Label(new Rect(x, y, labelWidth, lineheight), "cylinder position z:");
		GUI.TextField(new Rect(x + labelWidth, y, 50, lineheight), cappos.z.ToString());
		cappos.z = (float)GUI.HorizontalSlider(new Rect(x + labelWidth + 55, y, 100, lineheight), cappos.z, -5, 5);
		y += lineheight + margin;

		GUI.Label(new Rect(x, y, labelWidth, lineheight), "sphere position x:");
		GUI.TextField(new Rect(x + labelWidth, y, 50, lineheight), sphpos.x.ToString());
		sphpos.x = (float)GUI.HorizontalSlider(new Rect(x + labelWidth + 55, y, 100, lineheight), sphpos.x, -5, 5);
		y += lineheight + margin;

		GUI.Label(new Rect(x, y, labelWidth, lineheight), "sphere position y:");
		GUI.TextField(new Rect(x + labelWidth, y, 50, lineheight), sphpos.y.ToString());
		sphpos.y = (float)GUI.HorizontalSlider(new Rect(x + labelWidth + 55, y, 100, lineheight), sphpos.y, -5, 5);
		y += lineheight + margin;

		GUI.Label(new Rect(x, y, labelWidth, lineheight), "sphere position z:");
		GUI.TextField(new Rect(x + labelWidth, y, 50, lineheight), sphpos.z.ToString());
		sphpos.z = (float)GUI.HorizontalSlider(new Rect(x + labelWidth + 55, y, 100, lineheight), sphpos.z, -5, 5);
		y += lineheight + margin;

		y += 10.0f;

		GUI.Label(new Rect(x, y, 300, lineheight), "mouse drag & wheel: move camera");
		y += lineheight + margin;

		GUI.Label(new Rect(x, y, 300, lineheight), "space: show / hide GUI");
		y += lineheight + margin;

		sphere.transform.position = sphpos;
		capsule.transform.position = cappos;
		capsule.transform.rotation = Quaternion.Euler(caprot);
	}
}
