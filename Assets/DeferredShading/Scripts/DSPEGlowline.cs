using UnityEngine;
using System.Collections;

[RequireComponent(typeof(DSRenderer))]
public class DSPEGlowline : MonoBehaviour
{
	public enum Type
	{
		Radial = 0,
		Voronoi = 1,
	}

	public Type type = Type.Radial;
	public float intensity = 1.0f;
	public Vector4 baseColor = new Vector4(0.45f, 0.4f, 2.0f, 0.0f);
	Material matGlowLine;
	RenderBuffer[] rbBuffers;
	DSRenderer dscam;


	void Start()
	{
		dscam = GetComponent<DSRenderer>();
		dscam.AddCallbackPostLighting(() => { Render(); });

		matGlowLine = new Material(Shader.Find("Custom/PostEffect_Glowline"));
	}

	void Render()
	{
		if (!enabled) { return; }
		if (rbBuffers==null)
		{
			rbBuffers = new RenderBuffer[2] {
				dscam.rtComposite.colorBuffer,
				dscam.rtGlowBuffer.colorBuffer,
			};
		}

		Graphics.SetRenderTarget(rbBuffers, dscam.rtNormalBuffer.depthBuffer);
		matGlowLine.SetTexture("_PositionBuffer", dscam.rtPositionBuffer);
		matGlowLine.SetTexture("_NormalBuffer", dscam.rtNormalBuffer);
		matGlowLine.SetFloat("_Intensity", intensity);
		matGlowLine.SetVector("_BaseColor", baseColor);
		matGlowLine.SetPass((int)type);
		DSRenderer.DrawFullscreenQuad();
		Graphics.SetRenderTarget(dscam.rtComposite);
	}
}
