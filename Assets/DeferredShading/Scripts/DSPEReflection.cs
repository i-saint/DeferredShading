using UnityEngine;
using System.Collections;

[RequireComponent(typeof(DSCamera))]
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
	Material matReflection;
	Material matCombine;
	DSCamera dscam;


	void Start()
	{
		dscam = GetComponent<DSCamera>();
		dscam.AddCallbackPostLighting(() => { Render(); }, 10000);

		Camera cam = GetComponent<Camera>();
		rtHalf = DSCamera.CreateRenderTexture((int)cam.pixelWidth / 2, (int)cam.pixelHeight / 2, 0, RenderTextureFormat.ARGBHalf);
		rtHalf.filterMode = FilterMode.Bilinear;
		matReflection = new Material(Shader.Find("Custom/PostEffect_Reflection"));
		matCombine = new Material(Shader.Find("Custom/Combine"));
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
		DSCamera.DrawFullscreenQuad();

		Graphics.SetRenderTarget(dscam.rtComposite);
		matCombine.SetTexture("_MainTex", rtHalf);
		matCombine.SetPass(0);
		DSCamera.DrawFullscreenQuad();
	}
}
