using UnityEngine;
using System.Collections;

[RequireComponent(typeof(DSCamera))]
public class DSPEGlowNormal : MonoBehaviour
{
	public Vector4 baseColor = new Vector4(0.75f, 0.75f, 1.25f, 0.0f);
	public float intensity = 1.0f;
	public float threshold = 0.5f;
	public float edge = 0.2f;
	Material matGlowNormal;
	DSCamera dscam;

	void Start()
	{
		dscam = GetComponent<DSCamera>();
		dscam.AddCallbackPostLighting(() => { Render(); });

		matGlowNormal = new Material(Shader.Find("Custom/PostEffect_GlowNormal"));
	}


	void Render()
	{
		if (!enabled) { return; }

		matGlowNormal.SetFloat("_Intensity", intensity);
		matGlowNormal.SetFloat("_Threshold", threshold);
		matGlowNormal.SetFloat("_Edge", edge);
		matGlowNormal.SetTexture("_PositionBuffer", dscam.rtPositionBuffer);
		matGlowNormal.SetTexture("_NormalBuffer", dscam.rtNormalBuffer);
		matGlowNormal.SetPass(0);
		DSCamera.DrawFullscreenQuad();
	}
}
