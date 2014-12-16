using UnityEngine;
using System.Collections;

public class TestSurfaceLight : MonoBehaviour
{
    public GameObject cam;
    public GameObject cube;
    public bool showGUI = true;
    public bool rotateByTime = true;
    public GameObject[] orbs;
    public Vector3 lookat;
    private ComboBox cbGridPatterns;

    void Start()
    {

        for (int xi = 0; xi < 15; ++xi)
        {
            for (int zi = 0; zi < 15; ++zi)
            {
                Instantiate(cube, new Vector3(1.1f * xi - 7.7f, Random.Range(-2.0f, 0.0f) - 0.7f, 1.1f * zi - 7.7f), Quaternion.identity);
            }
        }
    }

    void Update()
    {
        CameraControl();
        if (Input.GetKeyUp(KeyCode.Space)) { showGUI = !showGUI; }
        if (Input.GetKeyUp(KeyCode.R)) { rotateByTime = !rotateByTime; }

        for (int i = 0; i < orbs.Length; ++i )
        {
            Vector3 pos = orbs[i].transform.position;
            pos = Quaternion.Euler(0.0f, Time.deltaTime * 25.0f, 0) * pos;
            orbs[i].transform.position = pos;
        }
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
        cam.transform.LookAt(lookat);
    }

    void OnGUI()
    {
        float lineheight = 22.0f;
        float margin = 0.0f;
        float labelWidth = 80.0f;
        float x = 10.0f;
        float y = 10.0f;

        if (!showGUI) { return; }

        DSPEGlowline glowline = cam.GetComponent<DSPEGlowline>();
        DSPEReflection reflection = cam.GetComponent<DSPEReflection>();
        DSPEBloom bloom = cam.GetComponent<DSPEBloom>();
        DSPESurfaceLight slight = cam.GetComponent<DSPESurfaceLight>();


        glowline.enabled = GUI.Toggle(new Rect(x, y, 150, lineheight), glowline.enabled, "glowline");
        y += lineheight + margin;

        bloom.enabled = GUI.Toggle(new Rect(x, y, 150, lineheight), bloom.enabled, "bloom");
        y += lineheight + margin;

        reflection.enabled = GUI.Toggle(new Rect(x, y, 150, lineheight), reflection.enabled, "reflection");
        y += lineheight + margin;

        slight.enabled = GUI.Toggle(new Rect(x, y, 150, lineheight), slight.enabled, "surface light");
        y += lineheight + margin;

        GUI.Label(new Rect(x + 20, y, labelWidth, lineheight), "intensity:");
        GUI.TextField(new Rect(x + 20 + labelWidth, y, 50, lineheight), slight.intensity.ToString());
        slight.intensity = (float)GUI.HorizontalSlider(new Rect(x + 20 + labelWidth + 55, y, 100, lineheight), slight.intensity, 0.0f, 1.0f);
        y += lineheight + margin;

        GUI.Label(new Rect(x + 20, y, labelWidth, lineheight), "ray distance:");
        GUI.TextField(new Rect(x + 20 + labelWidth, y, 50, lineheight), slight.rayAdvance.ToString());
        slight.rayAdvance = (float)GUI.HorizontalSlider(new Rect(x + 20 + labelWidth + 55, y, 100, lineheight), slight.rayAdvance, 0.0f, 5.0f);
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
