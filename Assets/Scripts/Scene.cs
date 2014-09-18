using UnityEngine;
using System.Collections;

public class Scene : MonoBehaviour
{

	public GameObject[] lights = new GameObject[3];
	public GameObject cam;
	public GameObject cube;
	public bool showGUI = true;
	public bool rotateByTime = true;
	private ComboBox cbGridPatterns;

	void Start ()
	{

		float[] table = new float[4]{0.0f, 90.0f, 180.0f, 270.0f};
		for (int xi = 0; xi < 15; ++xi )
		{
			for (int zi = 0; zi < 15; ++zi)
			{
				GameObject obj = (GameObject)Instantiate(cube, new Vector3(1.1f*xi-7.7f, Random.Range(-2.0f, 0.0f)-0.7f, 1.1f*zi-7.7f), Quaternion.identity);
				obj.transform.Rotate(new Vector3(1.0f, 0.0f, 0.0f), table[Random.Range(0, 4)]);
				obj.transform.Rotate(new Vector3(0.0f, 1.0f, 0.0f), table[Random.Range(0, 4)]);
				obj.transform.Rotate(new Vector3(0.0f, 0.0f, 1.0f), table[Random.Range(0, 4)]);
			}
		}
	}
	
	void Update()
	{
		CameraControl();
		if (Input.GetKeyUp(KeyCode.Space)) { showGUI = !showGUI; }
		if (Input.GetKeyUp(KeyCode.R)) { rotateByTime = !rotateByTime; }
	}

	void CameraControl()
	{
		Vector3 pos = cam.transform.position;
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
		cam.transform.LookAt(new Vector3(0.0f, 0.0f, 0.0f));
	}

	void OnGUI()
	{
		float lineheight = 22.0f;
		float margin = 0.0f;
		float x = 10.0f;
		float y = 10.0f;

		if (!showGUI) { return; }

		DSLight[] dslights = new DSLight[3] {
			lights[0].GetComponent<DSLight>(),
			lights[1].GetComponent<DSLight>(),
			lights[2].GetComponent<DSLight>(),
		};
		DSPEGlowline glowline = cam.GetComponent<DSPEGlowline>();
		DSPEGlowNormal glownormal = cam.GetComponent<DSPEGlowNormal>();
		DSPEReflection reflection = cam.GetComponent<DSPEReflection>();
		DSPEBloom bloom = cam.GetComponent<DSPEBloom>();


		dslights[0].castShadow = GUI.Toggle(new Rect(x, y, 150, lineheight), dslights[0].castShadow, "shadow");
		foreach(var dsl in dslights) {
			dsl.castShadow = dslights[0].castShadow;
		}
		y += lineheight + margin;

		glownormal.enabled = GUI.Toggle(new Rect(x, y, 150, lineheight), glownormal.enabled, "glownormal");
		y += lineheight + margin;

		glowline.enabled = GUI.Toggle(new Rect(x, y, 150, lineheight), glowline.enabled, "glowline");
		y += lineheight + margin;

		//if (cbGridPatterns == null)
		//{
		//	GUIContent[] gridPatterns = new GUIContent[3];
		//	gridPatterns[0] = new GUIContent("hexagon");
		//	gridPatterns[1] = new GUIContent("grid");
		//	gridPatterns[2] = new GUIContent("grid voronoi");

		//	GUIStyle listStyle = new GUIStyle();
		//	listStyle.normal.textColor = Color.white;
		//	listStyle.onHover.background =
		//	listStyle.hover.background = new Texture2D(2, 2);
		//	listStyle.padding.left =
		//	listStyle.padding.right =
		//	listStyle.padding.top =
		//	listStyle.padding.bottom = 4;

		//	cbGridPatterns = new ComboBox(new Rect(x+20, y, 150, lineheight), gridPatterns[0], gridPatterns, "button", "box", listStyle);
		//}
		//y += lineheight + margin;

		bloom.enabled = GUI.Toggle(new Rect(x, y, 150, lineheight), bloom.enabled, "bloom");
		y += lineheight + margin;

		reflection.enabled = GUI.Toggle(new Rect(x, y, 150, lineheight), reflection.enabled, "reflection");
		y += lineheight + margin;

		y += 10.0f;

		GUI.Label(new Rect(x, y, 300, lineheight), "mouse drag & wheel: move camera");
		y += lineheight + margin;

		GUI.Label(new Rect(x, y, 300, lineheight), "space: show / hide GUI");
		y += lineheight + margin;

		GUI.Label(new Rect(x, y, 300, lineheight), "R: rotation on / off");
		y += lineheight + margin;

		//cbGridPatterns.Show();
	}
}
