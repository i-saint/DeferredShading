using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DSEffectShockwave : MonoBehaviour
{
    public static List<DSEffectShockwave> instances = new List<DSEffectShockwave>();

    public float gap = 0.5f;
    public float speed = 5.0f;
    public float fade_speed = 0.025f;
    public float time = 0.0f;
    public float opacity = 2.0f;

    public Transform trans;
    public Vector4 shockwave_params;

    void OnEnable()
    {
        instances.Add(this);
    }

    void OnDisable()
    {
        instances.Remove(this);
    }

    void Awake()
    {
        trans = GetComponent<Transform>();
    }

    void Update()
    {
        time += Time.deltaTime;
        opacity -= fade_speed*Time.deltaTime;
        shockwave_params.Set(opacity, gap, gap, gap);
        trans.localScale = trans.localScale + (Vector3.one * speed * Time.deltaTime);

        if (opacity <= 0.0f)
        {
            Destroy(gameObject);
        }
    }
}
