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

	public Type type = Type.Light;
	public float intensity = 1.0f;
	public float rayAdvance = 1.0f;
	public RenderTexture rtHalf;
	public Material matReflection;
	public Material matCombine;
	DSRenderer dscam;


	void Start()
	{
		dscam = GetComponent<DSRenderer>();
		dscam.AddCallbackPostEffect(() => { Render(); }, 5000);

		Camera cam = GetComponent<Camera>();
		rtHalf = DSRenderer.CreateRenderTexture((int)cam.pixelWidth / 2, (int)cam.pixelHeight / 2, 0, RenderTextureFormat.ARGBHalf);
		rtHalf.filterMode = FilterMode.Bilinear;
		//matReflection = new Material(Shader.Find("Custom/PostEffect_Reflection"));
		//matCombine = new Material(Shader.Find("Custom/Combine"));
	}

	void Render()
	{
		if (!enabled) { return; }

		Graphics.SetRenderTarget(rtHalf);
		GL.Clear(false, true, Color.black);
		matReflection.SetFloat("_Intensity", intensity);
		matReflection.SetFloat("_RayAdvance", rayAdvance);
		matReflection.SetTexture("_FrameBuffer", dscam.rtComposite);
		matReflection.SetTexture("_PositionBuffer", dscam.rtPositionBuffer);
		matReflection.SetTexture("_NormalBuffer", dscam.rtNormalBuffer);
		matReflection.SetPass((int)type);
		DSRenderer.DrawFullscreenQuad();

		Graphics.SetRenderTarget(dscam.rtComposite);
		matCombine.SetTexture("_MainTex", rtHalf);
		matCombine.SetPass(0);
		DSRenderer.DrawFullscreenQuad();
	}
}
