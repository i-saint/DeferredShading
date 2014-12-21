using UnityEngine;
using System.Collections;

public class ExplosionFlash : MonoBehaviour
{
    Light m_light;

    void OnEnable()
    {
        m_light = GetComponent<Light>();
    }

    void Update()
    {
        m_light.intensity -= 0.01f;
        if (m_light.intensity<0.0f)
        {
            Destroy(gameObject);
        }
    }
}
