using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;


public class TestGPUParticle : MonoBehaviour
{
    public GameObject m_camera;
    public GPUParticleWorld m_particle_world;
    public GPUParticleEmitter m_particle_emitter;
    public GPUParticleTrailRenderer m_trail_renderer;
    public GameObject m_capsule;
    public GameObject m_sphere;
    public GameObject m_floor;
    public bool m_show_gui = true;
    public int m_particles_par_frame = 32;



    void Start()
    {
    }

    void Update()
    {
        CameraControl();

        if (Input.GetKeyUp(KeyCode.Space))
        {
            m_show_gui = !m_show_gui;
        }
    }

    void CameraControl()
    {
        //Vector3 pos = Quaternion.Euler(0.0f, Time.deltaTime * -15.0f, 0) * cam.transform.position;
        Vector3 pos = m_camera.transform.position;
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
        m_camera.transform.position = pos;
        m_camera.transform.LookAt(new Vector3(0.0f, 1.0f, 0.0f));
    }

    void OnGUI()
    {
        float lineheight = 22.0f;
        float margin = 0.0f;
        float labelWidth = 130.0f;
        float x = 10.0f;
        float y = 10.0f;
        Vector3 sphpos = m_sphere.transform.position;
        Vector3 cappos = m_capsule.transform.position;
        Vector3 caprot = m_capsule.transform.rotation.eulerAngles;

        if (!m_show_gui) { return; }


        GUI.Label(new Rect(x, y, labelWidth, lineheight), "particles par frame:");
        GUI.TextField(new Rect(x + labelWidth, y, 50, lineheight), m_particles_par_frame.ToString());
        m_particles_par_frame = (int)GUI.HorizontalSlider(new Rect(x + labelWidth + 55, y, 100, lineheight), m_particles_par_frame, 0, 500);
        y += lineheight + margin;
        if (m_particle_emitter != null)
        {
            m_particle_emitter.m_emit_count = m_particles_par_frame;
        }

        m_trail_renderer.enabled = GUI.Toggle(new Rect(x, y, 100, lineheight), m_trail_renderer.enabled, "trail");
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

        m_sphere.transform.position = sphpos;
        m_capsule.transform.position = cappos;
        m_capsule.transform.rotation = Quaternion.Euler(caprot);
    }
}
