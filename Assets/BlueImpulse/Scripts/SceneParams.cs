using UnityEngine;
using System.Collections;

public class SceneParams : MonoBehaviour
{
    public GameObject m_effect_manager;

    public float m_light_intensity = 0.0f;
    public float m_water_brightness = 1.0f;
    public float m_radial_blur_y = -50.0f;
    public float m_radial_blur_intensity = 0.5f;
    public float m_radial_blur_radius = 0.1f;

    void Update ()
    {
        {
            var c = m_effect_manager.GetComponent<DSPECaustics>();
            var w = m_effect_manager.GetComponent<DSPEWater>();
            c.m_intensity = 1.0f * m_water_brightness;
            w.m_reflection_intensity = 0.3f * m_water_brightness;
            w.m_fresnel = 0.15f * m_water_brightness;
        }
        {
            var t = m_effect_manager.GetComponent<DSPERadialBlur>();
            t.m_center.y = m_radial_blur_y;
            t.m_intensity = m_radial_blur_intensity;
            t.m_radius = m_radial_blur_radius;
        }
    }
}
