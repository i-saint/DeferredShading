using UnityEngine;
using System.Collections;

[RequireComponent(typeof(DSRenderer))]
public class DSPESurfaceLight : MonoBehaviour
{
    public bool halfResolution = false;
    public float intensity = 1.0f;
    public float rayAdvance = 1.0f;
    public Material matSurfaceLight;
    public Material matCombine;
    public Material matFill;
    public RenderTexture[] rtTemp;
    DSRenderer dsr;


    void Start()
    {
        rtTemp = new RenderTexture[2];
        dsr = GetComponent<DSRenderer>();
        dsr.AddCallbackPostLighting(() => { Render(); }, 100);
    }

    void OnDisable()
    {
    }

    void Render()
    {
        if (!enabled) { return; }

        Vector2 reso = dsr.GetRenderResolution();
        if (rtTemp[0] == null)
        {
            int div = halfResolution ? 2 : 1;
            for (int i = 0; i < rtTemp.Length; ++i )
            {
                rtTemp[i] = DSRenderer.CreateRenderTexture((int)reso.x / div, (int)reso.y / div, 0, RenderTextureFormat.ARGBHalf);
                rtTemp[i].filterMode = FilterMode.Bilinear;
            }
        }
        Graphics.SetRenderTarget(rtTemp[1]);
        matFill.SetVector("_Color", new Vector4(0.0f, 0.0f, 0.0f, 0.02f));
        matFill.SetTexture("_PositionBuffer1", dsr.rtPositionBuffer);
        matFill.SetTexture("_PositionBuffer2", dsr.rtPrevPositionBuffer);
        matFill.SetPass(1);
        DSRenderer.DrawFullscreenQuad();

        Graphics.SetRenderTarget(rtTemp[0]);
        matSurfaceLight.SetFloat("_Intensity", intensity);
        matSurfaceLight.SetFloat("_RayAdvance", rayAdvance);
        matSurfaceLight.SetTexture("_NormalBuffer", dsr.rtNormalBuffer);
        matSurfaceLight.SetTexture("_PositionBuffer", dsr.rtPositionBuffer);
        matSurfaceLight.SetTexture("_ColorBuffer", dsr.rtColorBuffer);
        matSurfaceLight.SetTexture("_GlowBuffer", dsr.rtGlowBuffer);
        matSurfaceLight.SetTexture("_PrevResult", rtTemp[1]);
        matSurfaceLight.SetPass(0);
        DSRenderer.DrawFullscreenQuad();

        Graphics.SetRenderTarget(dsr.rtComposite);
        matCombine.SetTexture("_MainTex", rtTemp[0]);
        matCombine.SetVector("_PixelSize", new Vector4(1.0f / rtTemp[0].width, 1.0f / rtTemp[0].height, 0.0f, 0.0f));
        matCombine.SetPass(2);
        DSRenderer.DrawFullscreenQuad();

        Swap(ref rtTemp[0], ref rtTemp[1]);
    }

    public static void Swap<T>(ref T lhs, ref T rhs)
    {
        T temp;
        temp = lhs;
        lhs = rhs;
        rhs = temp;
    }
}
