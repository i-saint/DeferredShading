using UnityEngine;
using System.Collections;

public class DSBeam : MonoBehaviour
{
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
    Transform trans;
    MeshRenderer mesh_renderer;
    MaterialPropertyBlock property_block;
    Vector4 beam_params;

    void Start()
    {
        trans = GetComponent<Transform>();
        property_block = new MaterialPropertyBlock();
        property_block.AddVector("beam_direction", beam_params);
        property_block.AddVector("base_position", trans.position);
        mesh_renderer = GetComponent<MeshRenderer>();
        mesh_renderer.SetPropertyBlock(property_block);
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

    void OnWillRenderObject()
    {
        property_block.SetVector("beam_direction", beam_params);
        property_block.SetVector("base_position", trans.position);
        mesh_renderer.SetPropertyBlock(property_block);
    }
}
