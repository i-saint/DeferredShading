using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DSBeam : MonoBehaviour
{
    public static List<DSBeam> instances = new List<DSBeam>();

    public enum State
    {
        Active,
        Fading,
    }

    public float speed = 20.0f;
    public float length = 0.0f;
    public float fade_speed = 0.025f;
    public float lifetime = 2.0f;
    public float time = 0.0f;
    public State state = State.Active;

    public Transform trans;
    public Vector4 beam_params;

    void OnEnable()
    {
        instances.Add(this);
        Debug.Log("DSBeam.OnEnable(): " + instances.Count + " instances");
    }

    void OnDisable()
    {
        instances.Remove(this);
        Debug.Log("DSBeam.OnDisable(): " + instances.Count + " instances");
    }

    void Start()
    {
        trans = GetComponent<Transform>();
    }

    void Update()
    {
        time += Time.deltaTime;
        length = speed * time;
        beam_params.Set(trans.forward.x, trans.forward.y, trans.forward.z, length);

        if (state==State.Active)
        {
            if (time > lifetime)
            {
                state = State.Fading;
            }
        }
        else if (state == State.Fading)
        {
            Vector3 scale = trans.localScale;
            scale.x -= fade_speed;
            scale.y -= fade_speed;
            scale.z -= fade_speed;
            trans.localScale = scale;
            if (scale.x <= 0.0f)
            {
                Destroy(gameObject);
            }
        }
    }
}
