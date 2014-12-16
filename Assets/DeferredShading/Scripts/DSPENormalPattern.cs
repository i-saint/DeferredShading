using UnityEngine;
using System.Collections;

[RequireComponent(typeof(DSRenderer))]
public class DSPENormalPattern : MonoBehaviour
{
    public Material matNormalPattern;
    public Material matCopyGBuffer;
    public RenderTexture rtNormalCopy;
    DSRenderer dsr;

    void Start()
    {
        dsr = GetComponent<DSRenderer>();
        dsr.AddCallbackPostGBuffer(() => { Render(); }, 100);
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
