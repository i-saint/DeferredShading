using UnityEngine;
using System.Collections;

[RequireComponent(typeof(DSRenderer))]
public class DSPEReflection : MonoBehaviour
{
	public enum Type
	{
		Light = 0,
		Precise = 1,
	}

	public bool halfResolution = true;
	public Type type = Type.Light;
	public float intensity = 1.0f;
	public float rayAdvance = 1.0f;
	public RenderTexture rtTemp;
	public Material matReflection;
	public Material matCombine;
	DSRenderer dscam;


	void Start()
	{
		dscam = GetComponent<DSRenderer>();
		dscam.AddCallbackPostEffect(() => { Render(); }, 5000);
	}

	void Render()
	{
		if (!enabled) { return; }

		if (rtTemp == null)
		{
			int div = halfResolution ? 2 : 1;
			Camera cam = GetComponent<Camera>();
			rtTemp = DSRenderer.CreateRenderTexture((int)cam.pixelWidth / div, (int)cam.pixelHeight / div, 0, RenderTextureFormat.ARGBHalf);
			rtTemp.filterMode = FilterMode.Bilinear;
		}
		Graphics.SetRenderTarget(rtTemp);
		GL.Clear(false, true, Color.black);
		matReflection.SetFloat("_Intensity", intensity);
		matReflection.SetFloat("_RayAdvance", rayAdvance);
		matReflection.SetTexture("_FrameBuffer", dscam.rtComposite);
		matReflection.SetTexture("_PositionBuffer", dscam.rtPositionBuffer);
		matReflection.SetTexture("_NormalBuffer", dscam.rtNormalBuffer);
		matReflection.SetPass((int)type);
		DSRenderer.DrawFullscreenQuad();

		Graphics.SetRenderTarget(dscam.rtComposite);
		matCombine.SetTexture("_MainTex", rtTemp);
		matCombine.SetPass(0);
		DSRenderer.DrawFullscreenQuad();
	}
}
