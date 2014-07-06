using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Camera))]
public class DSCamera : MonoBehaviour
{
	public bool showBuffers = false;
	public Material matPointLight;
	public Material matDirectionalLight;
	public Material matGlowLine;
	public Material matGlowNormal;
	public Material matReflection;
	public GameObject sphereMeshObject;

	public RenderTexture[] mrtTex = new RenderTexture[4];
	RenderBuffer[] mrtRB = new RenderBuffer[4];
	public RenderTexture rtComposite;
	Camera cam;


	void Start ()
	{
		cam = GetComponent<Camera>();
		for (int i = 0; i < mrtTex.Length; ++i )
		{
			mrtTex[i] = new RenderTexture((int)cam.pixelWidth, (int)cam.pixelHeight, 32, RenderTextureFormat.ARGBFloat);
			mrtRB[i] = mrtTex[i].colorBuffer;
		}
		rtComposite = new RenderTexture((int)cam.pixelWidth, (int)cam.pixelHeight, 32, RenderTextureFormat.ARGBFloat);
		matPointLight.SetTexture("_NormalBuffer", mrtTex[0]);
		matPointLight.SetTexture("_PositionBuffer", mrtTex[1]);
		matPointLight.SetTexture("_ColorBuffer", mrtTex[2]);
		matPointLight.SetTexture("_GlowBuffer", mrtTex[3]);
		matDirectionalLight.SetTexture("_NormalBuffer", mrtTex[0]);
		matDirectionalLight.SetTexture("_PositionBuffer", mrtTex[1]);
		matDirectionalLight.SetTexture("_ColorBuffer", mrtTex[2]);
		matDirectionalLight.SetTexture("_GlowBuffer", mrtTex[3]);

		matGlowLine.SetTexture("_PositionBuffer", mrtTex[1]);
		matGlowLine.SetTexture("_NormalBuffer", mrtTex[0]);
		matGlowNormal.SetTexture("_PositionBuffer", mrtTex[1]);
		matGlowNormal.SetTexture("_NormalBuffer", mrtTex[0]);
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
		Graphics.SetRenderTarget(null);
		Graphics.SetRenderTarget(rtComposite);
		GL.Clear(true, true, Color.black);

		DSLight.sphereMesh = sphereMeshObject.GetComponent<MeshFilter>().mesh;
		DSLight.matPointLight = matPointLight;
		DSLight.matDirectionalLight = matDirectionalLight;
		DSLight.RenderLights(this);

		matGlowLine.SetPass(0);
		DrawFullscreenQuad();

		matGlowNormal.SetPass(0);
		DrawFullscreenQuad();

		Graphics.SetRenderTarget(null);
		GL.Clear(true, true, Color.black);
		matReflection.SetTexture("_FrameBuffer", rtComposite);
		matReflection.SetTexture("_PositionBuffer", mrtTex[1]);
		matReflection.SetTexture("_NormalBuffer", mrtTex[0]);
		matReflection.SetPass(0);
		DrawFullscreenQuad();

		Graphics.SetRenderTarget(null);
	}

	void OnGUI()
	{
		if (!showBuffers) { return; }

		Vector2 size = new Vector2(mrtTex[0].width, mrtTex[0].height) / 6.0f;
		float y = 5.0f;
		for (int i = 0; i < 3; ++i )
		{
			GUI.DrawTexture(new Rect(5, y, size.x, size.y), mrtTex[i], ScaleMode.ScaleToFit, false);
			y += size.y + 5.0f;
		}
		GUI.DrawTexture(new Rect(5, y, size.x, size.y), rtComposite, ScaleMode.ScaleToFit, false);
		y += size.y + 5.0f;
	}

	static public void DrawFullscreenQuad()
	{
		GL.Begin(GL.QUADS);
		//GL.Vertex3(-1.0f, 1.0f, 1.0f);
		//GL.Vertex3( 1.0f,  1.0f, 1.0f);
		//GL.Vertex3( 1.0f, -1.0f, 1.0f);
		//GL.Vertex3(-1.0f, -1.0f, 1.0f);

		GL.Vertex3(-1.0f, -1.0f, 1.0f);
		GL.Vertex3(1.0f, -1.0f, 1.0f);
		GL.Vertex3(1.0f, 1.0f, 1.0f);
		GL.Vertex3(-1.0f, 1.0f, 1.0f);
		GL.End();
	}
}
