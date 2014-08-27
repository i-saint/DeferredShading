using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class DSRenderer : MonoBehaviour
{
	public enum TextureFormat
	{
		Half,
		Float,
	}

	public delegate void Callback();
	public struct PriorityCallback
	{
		public int priority;
		public Callback callback;

		public PriorityCallback(Callback cb, int p)
		{
			priority = p;
			callback = cb;
		}
	}

	public class PriorityCallbackComp : IComparer<PriorityCallback>
	{
		public int Compare(PriorityCallback a, PriorityCallback b)
		{
			return a.priority.CompareTo(b.priority);
		}
	}

	public bool showBuffers = false;
	public TextureFormat textureFormat = TextureFormat.Half;
	public Material matFill;
	public Material matGBufferClear;
	public Material matPointLight;
	public Material matDirectionalLight;
	public Material matCombine;

	public RenderTexture[] rtGBuffer;
	public RenderTexture[] rtGBufferB;
	public RenderTexture rtNormalBuffer		{ get { return rtGBuffer[0]; } }
	public RenderTexture rtPositionBuffer	{ get { return rtGBuffer[1]; } }
	public RenderTexture rtColorBuffer		{ get { return rtGBuffer[2]; } }
	public RenderTexture rtGlowBuffer		{ get { return rtGBuffer[3]; } }
	public RenderTexture rtNormalBufferB	{ get { return rtGBufferB[0]; } }
	public RenderTexture rtPositionBufferB	{ get { return rtGBufferB[1]; } }
	public RenderTexture rtColorBufferB		{ get { return rtGBufferB[2]; } }
	public RenderTexture rtGlowBufferB		{ get { return rtGBufferB[3]; } }

	public RenderBuffer[] rbGBuffer;
	public RenderTexture rtComposite;
	public Camera cam;

	List<PriorityCallback> cbPreGBuffer = new List<PriorityCallback>();
	List<PriorityCallback> cbPostGBuffer = new List<PriorityCallback>();
	List<PriorityCallback> cbPreLighting = new List<PriorityCallback>();
	List<PriorityCallback> cbPostLighting = new List<PriorityCallback>();
	List<PriorityCallback> cbTransparent = new List<PriorityCallback>();
	List<PriorityCallback> cbPostEffect = new List<PriorityCallback>();

	public void AddCallbackPreGBuffer(Callback cb, int priority = 1000)
	{
		cbPreGBuffer.Add(new PriorityCallback(cb, priority));
		cbPreGBuffer.Sort(new PriorityCallbackComp());
	}
	public void AddCallbackPostGBuffer(Callback cb, int priority = 1000)
	{
		cbPostGBuffer.Add(new PriorityCallback(cb, priority));
		cbPostGBuffer.Sort(new PriorityCallbackComp());
	}
	public void AddCallbackPreLighting(Callback cb, int priority = 1000)
	{
		cbPreLighting.Add(new PriorityCallback(cb, priority));
		cbPreLighting.Sort(new PriorityCallbackComp());
	}
	public void AddCallbackPostLighting(Callback cb, int priority = 1000)
	{
		cbPostLighting.Add(new PriorityCallback(cb, priority));
		cbPostLighting.Sort(new PriorityCallbackComp());
	}
	public void AddCallbackTransparent(Callback cb, int priority = 1000)
	{
		cbTransparent.Add(new PriorityCallback(cb, priority));
		cbTransparent.Sort(new PriorityCallbackComp());
	}
	public void AddCallbackPostEffect(Callback cb, int priority = 1000)
	{
		cbPostEffect.Add(new PriorityCallback(cb, priority));
		cbPostEffect.Sort(new PriorityCallbackComp());
	}


	public static RenderTexture CreateRenderTexture(int w, int h, int d, RenderTextureFormat f)
	{
		RenderTexture r = new RenderTexture(w, h, d, f);
		r.filterMode = FilterMode.Point;
		r.useMipMap = false;
		r.generateMips = false;
		r.enableRandomWrite = true;
		//r.wrapMode = TextureWrapMode.Repeat;
		return r;
	}

	void Start ()
	{
		rtGBuffer = new RenderTexture[4];
		rtGBufferB = new RenderTexture[4];
		rbGBuffer = new RenderBuffer[4];
		cam = GetComponent<Camera>();

		RenderTextureFormat format = textureFormat == TextureFormat.Half ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGBFloat;
		for (int i = 0; i < rtGBuffer.Length; ++i)
		{
			int depthbits = i == 0 ? 32 : 0;
			rtGBuffer[i] = CreateRenderTexture((int)cam.pixelWidth, (int)cam.pixelHeight, depthbits, format);
			rtGBufferB[i] = CreateRenderTexture((int)cam.pixelWidth, (int)cam.pixelHeight, depthbits, format);
		}
		rtComposite = CreateRenderTexture((int)cam.pixelWidth, (int)cam.pixelHeight, 0, format);
	}


	public void SetRenderTargetsGBuffer()
	{
		Graphics.SetRenderTarget(rbGBuffer, rtNormalBuffer.depthBuffer);
	}

	public void SetRenderTargetsComposite()
	{
		Graphics.SetRenderTarget(rtComposite);
	}

	void OnPreRender()
	{
		for (int i = 0; i < rtGBuffer.Length; ++i)
		{
			Swap(ref rtGBufferB[i], ref rtGBuffer[i]);
		}
		for (int i = 0; i < rtGBuffer.Length; ++i)
		{
			rbGBuffer[i] = rtGBuffer[i].colorBuffer;
		}
		matPointLight.SetTexture("_NormalBuffer", rtNormalBuffer);
		matPointLight.SetTexture("_PositionBuffer", rtPositionBuffer);
		matPointLight.SetTexture("_ColorBuffer", rtColorBuffer);
		matPointLight.SetTexture("_GlowBuffer", rtGlowBuffer);
		matDirectionalLight.SetTexture("_NormalBuffer", rtNormalBuffer);
		matDirectionalLight.SetTexture("_PositionBuffer", rtPositionBuffer);
		matDirectionalLight.SetTexture("_ColorBuffer", rtColorBuffer);
		matDirectionalLight.SetTexture("_GlowBuffer", rtGlowBuffer);

		Graphics.SetRenderTarget(rbGBuffer, rtNormalBuffer.depthBuffer);
		matGBufferClear.SetPass(0);
		DrawFullscreenQuad();

		foreach (PriorityCallback cb in cbPreGBuffer) { cb.callback.Invoke(); }
	}

	void OnPostRender()
	{
		foreach (PriorityCallback cb in cbPostGBuffer) {
			cb.callback.Invoke();
			SetRenderTargetsGBuffer();
		}

		Graphics.SetRenderTarget(rtComposite);
		GL.Clear(true, true, Color.black);
		Graphics.SetRenderTarget(rtComposite.colorBuffer, rtNormalBuffer.depthBuffer);

		foreach (PriorityCallback cb in cbPreLighting) { cb.callback.Invoke(); }
		DSLight.matPointLight = matPointLight;
		DSLight.matDirectionalLight = matDirectionalLight;
		DSLight.RenderLights(this);
		foreach (PriorityCallback cb in cbPostLighting) { cb.callback.Invoke(); }
		foreach (PriorityCallback cb in cbTransparent) { cb.callback.Invoke(); }
		foreach (PriorityCallback cb in cbPostEffect) { cb.callback.Invoke(); }

		Graphics.SetRenderTarget(null);
		matCombine.SetTexture("_MainTex", rtComposite);
		matCombine.SetPass(1);
		DrawFullscreenQuad();
	}

	void OnGUI()
	{
		if (!showBuffers) { return; }

		Vector2 size = new Vector2(rtNormalBuffer.width, rtNormalBuffer.height) / 6.0f;
		float y = 5.0f;
		for (int i = 0; i < 4; ++i )
		{
			GUI.DrawTexture(new Rect(5, y, size.x, size.y), rtGBuffer[i], ScaleMode.ScaleToFit, false);
			y += size.y + 5.0f;
		}
		GUI.DrawTexture(new Rect(5, y, size.x, size.y), rtComposite, ScaleMode.ScaleToFit, false);
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

	public static void Swap<T>(ref T lhs, ref T rhs)
	{
		T temp;
		temp = lhs;
		lhs = rhs;
		rhs = temp;
	}

}
