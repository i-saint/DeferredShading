using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class DSCamera : MonoBehaviour
{
	public enum TextureFormat
	{
		Half,
		Float,
	}

	public bool showBuffers = false;
	public bool voronoiGlowline = true;
	public bool normalGlow = true;
	public bool reflection = true;
	public bool bloom = true;
	public TextureFormat textureFormat = TextureFormat.Half;
	public Material matFill;
	public Material matGBufferClear;
	public Material matPointLight;
	public Material matDirectionalLight;
	public Material matGlowLine;
	public Material matGlowNormal;
	public Material matReflection;
	public Material matBloomLuminance;
	public Material matBloomBlur;
	public Material matBloom;
	public Material matCombine;
	public Material matDF;

	public RenderTexture[] mrtTex;
	RenderBuffer[] mrtRB4;
	RenderBuffer[] mrtRB2;
	public RenderTexture[] rtComposite;
	public RenderTexture[] rtBloomH;
	public RenderTexture[] rtBloomQ;
	public RenderTexture rtHalf;
	public RenderTexture rtDepth;
	Camera cam;

	public delegate void Callback();
	public List<Callback> cbPreGBuffer = new List<Callback>();
	public List<Callback> cbPostGBuffer = new List<Callback>();
	public List<Callback> cbPreLighting = new List<Callback>();
	public List<Callback> cbPostLighting = new List<Callback>();


	RenderTexture CreateRenderTexture(int w, int h, int d, RenderTextureFormat f)
	{
		RenderTexture r = new RenderTexture(w, h, d, f);
		r.filterMode = FilterMode.Point;
		r.useMipMap = false;
		r.generateMips = false;
		return r;
	}

	void Start ()
	{
		mrtTex = new RenderTexture[4];
		mrtRB4 = new RenderBuffer[4];
		mrtRB2 = new RenderBuffer[2];
		rtComposite = new RenderTexture[2];
		rtBloomH = new RenderTexture[2];
		rtBloomQ = new RenderTexture[2];
		cam = GetComponent<Camera>();

		RenderTextureFormat format = textureFormat == TextureFormat.Half ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGBFloat;
		for (int i = 0; i < mrtTex.Length; ++i)
		{
			int depthbits = i == 0 ? 32 : 0;
			mrtTex[i] = CreateRenderTexture((int)cam.pixelWidth, (int)cam.pixelHeight, depthbits, format);
			mrtRB4[i] = mrtTex[i].colorBuffer;
		}
		for (int i = 0; i < rtComposite.Length; ++i)
		{
			rtComposite[i] = CreateRenderTexture((int)cam.pixelWidth, (int)cam.pixelHeight, 0, format);
			rtBloomH[i] = CreateRenderTexture(256, 512 / 2, 0, format);
			rtBloomH[i].filterMode = FilterMode.Bilinear;
			rtBloomQ[i] = CreateRenderTexture(128, 256, 0, format);
			rtBloomQ[i].filterMode = FilterMode.Bilinear;
		}
		rtHalf = CreateRenderTexture((int)cam.pixelWidth / 2, (int)cam.pixelHeight / 2, 0, format);
		rtHalf.filterMode = FilterMode.Bilinear;
		rtDepth = CreateRenderTexture((int)cam.pixelWidth, (int)cam.pixelHeight, 32, RenderTextureFormat.RHalf);

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
		matReflection.SetTexture("_FrameBuffer", rtComposite[0]);
		matReflection.SetTexture("_PositionBuffer", mrtTex[1]);
		matReflection.SetTexture("_NormalBuffer", mrtTex[0]);
		matBloom.SetTexture("_FrameBuffer", rtComposite[0]);
		matBloom.SetTexture("_GlowBuffer", mrtTex[3]);
	}
	
	void Update ()
	{
	
	}

	void OnPreRender()
	{
		DSSubtracted.PreRenderAll(this);

		//Graphics.SetRenderTarget(mrtTex[3]);
		//matFill.SetPass(0);
		//DrawFullscreenQuad();
		//mrtRB4[3] = rtComposite[0].colorBuffer;
		//Graphics.SetRenderTarget(mrtRB4, mrtTex[0].depthBuffer);
		//mrtRB4[3] = mrtTex[3].colorBuffer;

		Graphics.SetRenderTarget(mrtRB4, mrtTex[0].depthBuffer);
		matGBufferClear.SetPass(0);
		DrawFullscreenQuad();

		DSSubtracted.RenderAll(this);
		DSSubtractor.RenderAll(this);

		foreach (Callback cb in cbPreGBuffer) { cb.Invoke(); }
	}

	void OnPostRender()
	{
		foreach (Callback cb in cbPostGBuffer) { cb.Invoke(); }
		if (matDF)
		{
			matDF.SetPass(0);
			DrawFullscreenQuad();
		}

		Graphics.SetRenderTarget(rtComposite[0]);
		GL.Clear(true, true, Color.black);
		Graphics.SetRenderTarget(rtComposite[0].colorBuffer, mrtTex[0].depthBuffer);

		foreach (Callback cb in cbPreLighting) { cb.Invoke(); }
		DSLight.matPointLight = matPointLight;
		DSLight.matDirectionalLight = matDirectionalLight;
		DSLight.RenderLights(this);
		foreach (Callback cb in cbPostLighting) { cb.Invoke(); }

		if (voronoiGlowline)
		{
			mrtRB2[0] = rtComposite[0].colorBuffer;
			mrtRB2[1] = mrtTex[3].colorBuffer;
			Graphics.SetRenderTarget(mrtRB2, mrtTex[0].depthBuffer);
			matGlowLine.SetPass(0);
			DrawFullscreenQuad();
			Graphics.SetRenderTarget(rtComposite[0]);
		}
		if(normalGlow) {
			matGlowNormal.SetPass(0);
			DrawFullscreenQuad();
		}

		if (bloom)
		{
			Vector4 hscreen = new Vector4(rtBloomH[0].width, rtBloomH[0].height, 1.0f / rtBloomH[0].width, 1.0f / rtBloomH[0].height);
			Vector4 qscreen = new Vector4(rtBloomQ[0].width, rtBloomQ[0].height, 1.0f / rtBloomQ[0].width, 1.0f / rtBloomQ[0].height);
			matBloomBlur.SetVector("_Screen", hscreen);

			Graphics.SetRenderTarget(rtBloomH[0]);
			matBloomBlur.SetTexture("_GlowBuffer", mrtTex[3]);
			matBloomBlur.SetPass(0);
			DrawFullscreenQuad();
			Graphics.SetRenderTarget(rtBloomH[1]);
			matBloomBlur.SetTexture("_GlowBuffer", rtBloomH[0]);
			matBloomBlur.SetPass(0);
			DrawFullscreenQuad();
			Graphics.SetRenderTarget(rtBloomH[0]);
			matBloomBlur.SetTexture("_GlowBuffer", rtBloomH[1]);
			matBloomBlur.SetPass(0);
			DrawFullscreenQuad();
			Graphics.SetRenderTarget(rtBloomH[1]);
			matBloomBlur.SetTexture("_GlowBuffer", rtBloomH[0]);
			matBloomBlur.SetPass(1);
			DrawFullscreenQuad();

			matBloomBlur.SetVector("_Screen", qscreen);
			Graphics.SetRenderTarget(rtBloomQ[0]);
			matBloomBlur.SetTexture("_GlowBuffer", mrtTex[3]);
			matBloomBlur.SetPass(0);
			DrawFullscreenQuad();
			Graphics.SetRenderTarget(rtBloomQ[1]);
			matBloomBlur.SetTexture("_GlowBuffer", rtBloomQ[0]);
			matBloomBlur.SetPass(0);
			DrawFullscreenQuad();
			Graphics.SetRenderTarget(rtBloomQ[0]);
			matBloomBlur.SetTexture("_GlowBuffer", rtBloomQ[1]);
			matBloomBlur.SetPass(0);
			DrawFullscreenQuad();
			Graphics.SetRenderTarget(rtBloomQ[1]);
			matBloomBlur.SetTexture("_GlowBuffer", rtBloomQ[0]);
			matBloomBlur.SetPass(1);
			DrawFullscreenQuad();

			Graphics.SetRenderTarget(rtComposite[0]);
			matBloom.SetTexture("_GlowBuffer", mrtTex[3]);
			matBloom.SetTexture("_HalfGlowBuffer", rtBloomH[1]);
			matBloom.SetTexture("_QuarterGlowBuffer", rtBloomQ[1]);
			matBloom.SetPass(0);
			DrawFullscreenQuad();
		}
		if(reflection) {
			Graphics.SetRenderTarget(rtHalf);
			GL.Clear(false, true, Color.black);
			matReflection.SetPass(0);
			DrawFullscreenQuad();

			Graphics.SetRenderTarget(rtComposite[0]);
			matCombine.SetTexture("_MainTex", rtHalf);
			matCombine.SetPass(0);
			DrawFullscreenQuad();
		}


		Graphics.SetRenderTarget(null);
		GL.Clear(false, true, Color.black);
		matCombine.SetTexture("_MainTex", rtComposite[0]);
		matCombine.SetPass(1);
		DrawFullscreenQuad();

		Graphics.SetRenderTarget(null);
	}

	void OnGUI()
	{
		if (!showBuffers) { return; }

		Vector2 size = new Vector2(mrtTex[0].width, mrtTex[0].height) / 6.0f;
		float y = 5.0f;
		for (int i = 0; i < 4; ++i )
		{
			GUI.DrawTexture(new Rect(5, y, size.x, size.y), mrtTex[i], ScaleMode.ScaleToFit, false);
			y += size.y + 5.0f;
		}
		GUI.DrawTexture(new Rect(5, y, size.x, size.y), rtComposite[0], ScaleMode.ScaleToFit, false);
		y += size.y + 5.0f;
	}

	static public void DrawFullscreenQuad(float z=1.0f)
	{
		GL.Begin(GL.QUADS);
		GL.Vertex3(-1.0f, -1.0f, z);
		GL.Vertex3(1.0f, -1.0f, z);
		GL.Vertex3(1.0f, 1.0f, z);
		GL.Vertex3(-1.0f, 1.0f, z);

		GL.Vertex3(-1.0f, 1.0f, z);
		GL.Vertex3(1.0f, 1.0f, z);
		GL.Vertex3(1.0f, -1.0f, z);
		GL.Vertex3(-1.0f, -1.0f, z);
		GL.End();
	}
}
