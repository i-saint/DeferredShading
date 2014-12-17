using UnityEngine;
using System.Collections;


public class DSPFog : DSEffectBase
{
    public Material matFog;
    public Vector4 color = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
    public float near = 5.0f;
    public float far = 10.0f;

    void Awake()
    {
        UpdateDSRenderer();
        dsr.AddCallbackPostEffect(() => { Render(); }, 1100);
    }

    void Update()
    {
    }

    void Render()
    {
        if (!enabled || matFog == null) { return; }

        matFog.SetTexture("_PositionBuffer", dsr.rtPositionBuffer);
        matFog.SetVector("_Color", color);
        matFog.SetFloat("_Near", near);
        matFog.SetFloat("_Far", far);
        matFog.SetPass(0);
        DSRenderer.DrawFullscreenQuad();
    }
}
