using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;


public class TestBeam : MonoBehaviour
{
    public DSRenderer cam;
    public bool showGUI;
    public bool rotateByTime = false;
    public GameObject beam_prefab;
    public GameObject shockwave_prefab;



    void Start()
    {
    }

    void Update()
    {
        CameraControl();

        if (Time.frameCount % 30==0)
        {
            Vector3 pos = new Vector3(Random.Range(-5.0f, 5.0f), Random.Range(-1.0f, 2.0f) + 1.0f, Random.Range(-4.0f, 4.0f) - 10.0f);
            Vector3 dir = new Vector3(0.0f, 0.0f, 1.0f);
            DSEffectBeam.AddEntry(pos, dir);
            DSEffectShockwave.AddEntry(pos);
            DSEffectRadialBlur.AddEntry(pos);
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            showGUI = !showGUI;
        }
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
        float x = 10.0f;
        float y = 10.0f;

        if (!showGUI) { return; }


        GUI.Label(new Rect(x, y, 300, lineheight), "mouse drag & wheel: move camera");
        y += lineheight + margin;

        GUI.Label(new Rect(x, y, 300, lineheight), "space: show / hide GUI");
        y += lineheight + margin;
        GUI.Label(new Rect(x, y, 300, lineheight), "R: camera rotation on / off");
        y += lineheight + margin;
    }
}
