using UnityEngine;
using System.Collections;

[RequireComponent(typeof(DSRenderer))]
public class DSBeamRenderer : MonoBehaviour
{
    public static DSBeamRenderer instance;

    public Material mat;
    DSRenderer dsr;
    int i_beam_direction;
    int i_base_position;

    void Awake()
    {
        instance = this;
        dsr = GetComponent<DSRenderer>();
        dsr.AddCallbackPreGBuffer(() => { DepthPrePass(); });
        dsr.AddCallbackPostGBuffer(() => { Render(); });

        i_beam_direction = Shader.PropertyToID("beam_direction");
        i_base_position = Shader.PropertyToID("base_position");
    }
    
    void OnDestroy()
    {
        if(instance==this) {
            instance = null;
        }
    }

    void DepthPrePass()
    {
        if (!enabled) { return; }
        foreach (var b in DSBeam.instances)
        {
            mat.SetVector(i_beam_direction, b.beam_params);
            mat.SetVector(i_base_position, b.trans.position);
            mat.SetPass(0);
            Graphics.DrawMeshNow(dsr.mesh_sphere, b.trans.localToWorldMatrix);
        }
    }

    void Render()
    {
        if (!enabled) { return; }
        foreach (var b in DSBeam.instances)
        {
            mat.SetVector(i_beam_direction, b.beam_params);
            mat.SetVector(i_base_position, b.trans.position);
            mat.SetPass(1);
            Graphics.DrawMeshNow(dsr.mesh_sphere, b.trans.localToWorldMatrix);
        }
    }
}
