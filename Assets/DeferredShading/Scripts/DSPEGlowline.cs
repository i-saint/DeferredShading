using UnityEngine;
using System.Collections;

[RequireComponent(typeof(DSCamera))]
public class DSPEGlowline : MonoBehaviour
{
	public enum Type
	{
		Radial = 0,
		Voronoi = 1,
	}

	public Type type = Type.Radial;
	public float intensity = 1.0f;
	Material matGlowLine;
	RenderBuffer[] rbBuffers;
	DSCamera dscam;


	void Start()
	{
		dscam = GetComponent<DSCamera>();
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
		matGlowLine.SetPass((int)type);
		DSCamera.DrawFullscreenQuad();
		Graphics.SetRenderTarget(dscam.rtComposite);
	}
}
