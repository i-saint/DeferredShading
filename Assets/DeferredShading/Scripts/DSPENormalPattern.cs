using UnityEngine;
using System.Collections;

public class DSPENormalPattern : DSEffectBase
{
    public Material matNormalPattern;
    public Material matCopyGBuffer;
    public RenderTexture rtNormalCopy;

    void Awake()
    {
        UpdateDSRenderer();
        dsr.AddCallbackPostGBuffer(() => { Render(); }, 100);
    }

    void Update()
    {
    }

    void Render()
    {
        if (!enabled) { return; }

        Vector2 reso = dsr.GetRenderResolution();
        if (rtNormalCopy == null)
        {
            rtNormalCopy = DSRenderer.CreateRenderTexture((int)reso.x, (int)reso.y, 0, RenderTextureFormat.ARGBHalf);
        }

        Graphics.SetRenderTarget(rtNormalCopy);
        GL.Clear(false, true, Color.black);
        matNormalPattern.SetTexture("_PositionBuffer", dsr.rtPositionBuffer);
        matNormalPattern.SetTexture("_NormalBuffer", dsr.rtNormalBuffer);
        matNormalPattern.SetPass(0);
        DSRenderer.DrawFullscreenQuad();

        Graphics.SetRenderTarget(dsr.rtNormalBuffer);
        matCopyGBuffer.SetTexture("_NormalBuffer", rtNormalCopy);
        matCopyGBuffer.SetPass(2);
        DSRenderer.DrawFullscreenQuad();

        dsr.SetRenderTargetsGBuffer();
    }
}
