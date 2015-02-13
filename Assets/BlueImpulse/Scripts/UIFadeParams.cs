using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIFadeParams : MonoBehaviour
{
    public Image m_fader;
    public Text m_code;
    public Text m_sound;
    public RawImage m_unity_logo;

    public float m_fader_alpha;
    public float m_code_alpha;
    public float m_sound_alpha;
    public float m_logo_alpha;

    void Update ()
    {
        m_fader.color = new Color(0, 0, 0, m_fader_alpha);
        m_code.color = new Color(1, 1, 1, m_code_alpha);
        m_sound.color = new Color(1, 1, 1, m_sound_alpha);
        m_unity_logo.color = new Color(1, 1, 1, m_logo_alpha);
    }
}
