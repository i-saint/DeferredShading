using UnityEngine;
using System.Collections;

public class DSCamera : MonoBehaviour {

	public RenderTexture[] mrtTex = new RenderTexture[4];
	RenderBuffer[] mrtRB = new RenderBuffer[4];
	public RenderTexture rtComposite;


	void Start ()
	{
		for (int i = 0; i < mrtTex.Length; ++i )
		{
			mrtTex[i] = new RenderTexture((int)camera.pixelWidth, (int)camera.pixelHeight, 32, RenderTextureFormat.ARGBFloat);
			mrtRB[i] = mrtTex[i].colorBuffer;
		}
		rtComposite = new RenderTexture((int)camera.pixelWidth, (int)camera.pixelHeight, 32, RenderTextureFormat.ARGBFloat);
	}
	
	void Update ()
	{
	
	}

	void OnPreRender()
	{
		for (int i = 0; i < mrtTex.Length; ++i)
		{
			Graphics.SetRenderTarget(mrtTex[i]);
			GL.Clear(true, true, Color.black);
		}
		Graphics.SetRenderTarget(mrtRB, mrtTex[0].depthBuffer);
	}

	void OnPostRender()
	{

		Graphics.SetRenderTarget(rtComposite);
		GL.Clear(true, true, Color.black);

		Graphics.SetRenderTarget(null);
	}
}
