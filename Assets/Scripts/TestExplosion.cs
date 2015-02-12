using UnityEngine;
using System.Collections;

public class TestExplosion : MonoBehaviour
{
    public GPUParticleWorld m_pset;
    public DSLight m_light;
    public bool m_show_GUI = true;
    public bool m_enable_particles = true;
    public bool m_enable_shockwave = true;
    public bool m_enable_radialblur = true;
    public bool m_enable_light = true;

    static float R(float r = 0.5f)
    {
        return Random.Range(-r, r);
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space)) { m_show_GUI = !m_show_GUI; }

        if(Time.frameCount%90==0) {
            Vector3 pos = new Vector3(R(2.5f), R(1.0f) + 1.0f, R(2.5f));
            PutExplosion(pos);
        }

        if(Input.GetMouseButtonDown(0)) {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                PutExplosion(hit.point - ray.direction*0.1f);
            }
        }
    }

    void PutExplosion(Vector3 pos)
    {

        if (m_enable_shockwave)
        {
            DSShockwaveManager.AddEntity(pos);
        }
        if (m_enable_radialblur)
        {
            DSRadialBlurManager.AddEntity(pos);
        }
        if (m_enable_particles)
        {
            CSParticle[] additional = new CSParticle[1024];
            for (int i = 0; i < additional.Length; ++i)
            {
                additional[i].position = pos + (new Vector3(R(), R(), R())).normalized * R(0.1f);
                additional[i].velocity = (new Vector3(R(), R(), R())).normalized * R(20.0f);
            }
            m_pset.AddParticles(additional);
        }
        if (m_enable_light)
        {
            Instantiate(m_light, pos, Quaternion.identity);
        }
    }


    void OnGUI()
    {
        float lineheight = 22.0f;
        float margin = 0.0f;
        float labelWidth = 130.0f;
        float x = 10.0f;
        float y = 10.0f;

        if (!m_show_GUI) { return; }
        m_enable_shockwave = GUI.Toggle(new Rect(x, y, 150, lineheight), m_enable_shockwave, "shockwave");
        y += lineheight + margin;

        m_enable_radialblur = GUI.Toggle(new Rect(x, y, 150, lineheight), m_enable_radialblur, "radial blur");
        y += lineheight + margin;

        m_enable_particles = GUI.Toggle(new Rect(x, y, 150, lineheight), m_enable_particles, "particle");
        y += lineheight + margin;

        m_enable_light = GUI.Toggle(new Rect(x, y, 150, lineheight), m_enable_light, "light");
        y += lineheight + margin;

    }
}
