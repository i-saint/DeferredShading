using UnityEngine;
using System.Collections;

[RequireComponent(typeof(DSRenderer))]
public class DSEffectShockwaveRenderer : MonoBehaviour
{
    public static DSEffectShockwaveRenderer instance;

    public Material mat;
    DSRenderer dsr;
    int i_shockwave_params;
    int i_base_position;

    void Awake()
    {
        instance = this;
        dsr = GetComponent<DSRenderer>();
        dsr.AddCallbackPostEffect(() => { Render(); }, 5000);

        i_shockwave_params = Shader.PropertyToID("shockwave_params");
        i_base_position = Shader.PropertyToID("base_position");
    }
    
    void OnDestroy()
    {
        if(instance==this) {
            instance = null;
        }
    }

    void Render()
    {
        if (!enabled || DSEffectShockwave.instances.Count==0) { return; }
        dsr.UpdateShadowFramebuffer();
        foreach (var b in DSEffectShockwave.instances)
        {
            mat.SetVector(i_shockwave_params, b.shockwave_params);
            mat.SetVector(i_base_position, b.trans.position);
            mat.SetPass(0);
            Graphics.DrawMeshNow(dsr.mesh_sphere, b.trans.localToWorldMatrix);
        }
        //UpdateShadowFramebuffer()
    }
}
