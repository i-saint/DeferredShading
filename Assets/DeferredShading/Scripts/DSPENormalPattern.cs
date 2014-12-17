using UnityEngine;
using System.Collections;

public class DSPENormalPattern : DSEffectBase
{
    public Material matNormalPattern;
    public Material matCopyGBuffer;
    public RenderTexture rtNormalCopy;

    public override void OnReload()
    {
        base.OnReload();
        dsr.AddCallbackPostGBuffer(() => { Render(); }, 100);
    }

    void UpdateRenderTargets()
    {
        Vector2 reso = dsr.GetInternalResolution();
        if (rtNormalCopy != null && rtNormalCopy.width!=reso.x)
        {
            rtNormalCopy.Release();
            rtNormalCopy = null;
        }
        if (rtNormalCopy == null || !rtNormalCopy.IsCreated())
        {
            rtNormalCopy = DSRenderer.CreateRenderTexture((int)reso.x, (int)reso.y, 0, RenderTextureFormat.ARGBHalf);
        }
    }

    void Render()
    {
        if (!enabled) { return; }

        UpdateRenderTargets();

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
