using UnityEngine;
using System.Collections;

public class DSBeam : MonoBehaviour
{
    public float speed = 20.0f;
    public float length = 0.0f;
    Transform trans;
    MeshRenderer mesh_renderer;
    MaterialPropertyBlock property_block;
    Vector4 beam_params;

    void Start()
    {
        trans = GetComponent<Transform>();
        property_block = new MaterialPropertyBlock();
        property_block.AddMatrix("prev_Object2World", Matrix4x4.identity);
        mesh_renderer = GetComponent<MeshRenderer>();
        mesh_renderer.SetPropertyBlock(property_block);
    }

    void Update()
    {
        length += speed * Time.deltaTime;
        beam_params.Set(trans.forward.x, trans.forward.y, trans.forward.z, length);
    }

    void OnWillRenderObject()
    {
        property_block.SetVector("beam_direction", beam_params);
        mesh_renderer.SetPropertyBlock(property_block);
    }
}
