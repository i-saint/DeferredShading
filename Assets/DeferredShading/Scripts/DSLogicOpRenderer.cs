using UnityEngine;
using System.Collections;

public class DSLogicOpRenderer : MonoBehaviour
{
	public static DSLogicOpRenderer instance;

	public int layerLogicOp = 31;
	public Material matAnd;
	public RenderTexture rtDepth;
	public RenderTexture rtAndDepth;
	public RenderTexture[] rtAndGBuffer;
	public RenderBuffer[] rbAndGBuffer;
	public RenderTexture rtAndNormalBuffer { get { return rtAndGBuffer[0]; } }
	public RenderTexture rtAndPositionBuffer { get { return rtAndGBuffer[1]; } }
	public RenderTexture rtAndColorBuffer { get { return rtAndGBuffer[2]; } }
	public RenderTexture rtAndGlowBuffer { get { return rtAndGBuffer[3]; } }

	DSRenderer dscam;

	DSLogicOpRenderer()
	{
		instance = this;
	}

	void Start()
	{
		dscam = GetComponent<DSRenderer>();
		dscam.AddCallbackPreGBuffer(() => { Render(); }, 900);

		Camera cam = GetComponent<Camera>();
		cam.cullingMask = cam.cullingMask & (~(1 << layerLogicOp));
	}

	void InitializeResources()
	{
		if (rtDepth == null) {
			rtDepth = DSRenderer.CreateRenderTexture((int)dscam.cam.pixelWidth, (int)dscam.cam.pixelHeight, 32, RenderTextureFormat.RHalf);
		}
		if (DSAnd.instances.Count > 0 && rtAndDepth==null)
		{
			Camera cam = dscam.cam;
			rtAndDepth = DSRenderer.CreateRenderTexture((int)dscam.cam.pixelWidth, (int)dscam.cam.pixelHeight, 32, RenderTextureFormat.RHalf);
			rtAndGBuffer = new RenderTexture[4];
			rbAndGBuffer = new RenderBuffer[4];
			for (int i = 0; i < rtAndGBuffer.Length; ++i)
			{
				int depthbits = i == 0 ? 32 : 0;
				rtAndGBuffer[i] = DSRenderer.CreateRenderTexture((int)cam.pixelWidth, (int)cam.pixelHeight, depthbits, RenderTextureFormat.ARGBHalf);
				rbAndGBuffer[i] = rtAndGBuffer[i].colorBuffer;
			}
		}
	}

	void Render()
	{
		if (!enabled) { return; }

		// if there is no subtract or and object, just create g-buffer
		if (DSSubtract.instances.Count == 0 && DSAnd.instances.Count==0)
		{
			foreach (DSLogicOpReceiver l in DSLogicOpReceiver.instances)
			{
				l.matGBuffer.SetPass(0);
				Graphics.DrawMeshNow(l.mesh, l.trans.localToWorldMatrix);
			}
			return;
		}


		InitializeResources();

		// create depth buffer with reversed meshes
		{
			Graphics.SetRenderTarget(rtDepth);
			GL.Clear(true, true, Color.black, 0.0f);
			foreach (DSLogicOpReceiver l in DSLogicOpReceiver.instances)
			{
				l.matReverseDepth.SetPass(0);
				Graphics.DrawMeshNow(l.mesh, l.trans.localToWorldMatrix);
			}
			dscam.SetRenderTargetsGBuffer();
		}

		// create g-buffer 
		foreach (DSLogicOpReceiver l in DSLogicOpReceiver.instances)
		{
			l.matGBuffer.SetPass(0);
			Graphics.DrawMeshNow(l.mesh, l.trans.localToWorldMatrix);
		}

		// subtraction
		foreach (DSSubtract l in DSSubtract.instances)
		{
			l.matStencilWrite.SetPass(0);
			Graphics.DrawMeshNow(l.mesh, l.trans.localToWorldMatrix);
			l.matSubtractor.SetTexture("_Depth", rtDepth);
			l.matSubtractor.SetPass(0);
			Graphics.DrawMeshNow(l.mesh, l.trans.localToWorldMatrix);
			l.matStencilClear.SetPass(0);
			Graphics.DrawMeshNow(l.mesh, l.trans.localToWorldMatrix);
		}

		// and
		if (DSAnd.instances.Count>0)
		{
			Graphics.SetRenderTarget(rtDepth);
			GL.Clear(true, true, Color.black, 0.0f);
			foreach (DSAnd l in DSAnd.instances)
			{
				l.matReverseDepth.SetPass(0);
				Graphics.DrawMeshNow(l.mesh, l.trans.localToWorldMatrix);
			}

			Graphics.SetRenderTarget(rbAndGBuffer, rtAndNormalBuffer.depthBuffer);
			dscam.matGBufferClear.SetPass(0);
			DSRenderer.DrawFullscreenQuad();
			foreach (DSAnd l in DSAnd.instances)
			{
				l.matGBuffer.SetPass(0);
				Graphics.DrawMeshNow(l.mesh, l.trans.localToWorldMatrix);
			}
			dscam.SetRenderTargetsGBuffer();
			matAnd.SetPass(0);
			DSRenderer.DrawFullscreenQuad();
		}
	}
}
