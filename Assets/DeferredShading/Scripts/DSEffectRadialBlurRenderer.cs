using UnityEngine;
using System.Collections;

[RequireComponent(typeof(DSRenderer))]
public class DSEffectRadialBlurRenderer : MonoBehaviour
{
    public static DSEffectRadialBlurRenderer instance;

    public Material mat;
    DSRenderer dsr;
    int i_radialblur_params;
    int i_base_position;

    void Awake()
    {
        instance = this;
        dsr = GetComponent<DSRenderer>();
        dsr.AddCallbackPostEffect(() => { Render(); }, 10000);

        i_radialblur_params = Shader.PropertyToID("radialblur_params");
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
        if (!enabled || DSEffectRadialBlur.instances.Count == 0) { return; }
        dsr.UpdateShadowFramebuffer();
        foreach (var b in DSEffectRadialBlur.instances)
        {
            mat.SetVector(i_radialblur_params, b.shockwave_params);
            mat.SetVector(i_base_position, b.trans.position);
            mat.SetPass(0);
            Graphics.DrawMeshNow(dsr.mesh_sphere, b.trans.localToWorldMatrix);
        }
    }
}
