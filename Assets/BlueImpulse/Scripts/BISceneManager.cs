using UnityEngine;
using System.Collections;

public class BISceneManager : MonoBehaviour
{
    public bool m_show_time = false;
    public float m_time_scale = 1.0f;
    bool m_dirty;

    void Update()
    {
        if(m_dirty)
        {
            Time.timeScale = m_time_scale;
        }
    }

#if UNITY_EDITOR
    void OnGUI()
    {
        if (m_show_time)
        {
            GUI.Label(new Rect(5,5,200,20), "time: "+Time.time);
        }
    }

    void OnValidate()
    {
        m_dirty = true;
    }
#endif // UNITY_EDITOR
}
