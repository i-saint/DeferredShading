using UnityEngine;
using System.Collections;

public class DSLogicOpRenderer : MonoBehaviour
{
	public static DSLogicOpRenderer instance;

	public int layerLogicOp = 31;
	public Material matCopyGB;
	public Material matAnd;
	public RenderTexture rtDepth;
	public RenderTexture rtAndDepth;
	public RenderTexture[] rtShadowGBuffer;
	public RenderTexture[] rtAndGBuffer;
	public RenderBuffer[] rbShadowGBuffer;
	public RenderBuffer[] rbAndGBuffer;
	public RenderTexture rtAndNormalBuffer		{ get { return rtAndGBuffer[0]; } }
	public RenderTexture rtAndPositionBuffer	{ get { return rtAndGBuffer[1]; } }
	public RenderTexture rtAndColorBuffer		{ get { return rtAndGBuffer[2]; } }
	public RenderTexture rtAndGlowBuffer		{ get { return rtAndGBuffer[3]; } }
	public RenderTexture rtShadowNormalBuffer	{ get { return rtShadowGBuffer[0]; } }
	public RenderTexture rtShadowPositionBuffer	{ get { return rtShadowGBuffer[1]; } }
	public RenderTexture rtShadowColorBuffer	{ get { return rtShadowGBuffer[2]; } }
	public RenderTexture rtShadowGlowBuffer		{ get { return rtShadowGBuffer[3]; } }

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
			rtShadowGBuffer = new RenderTexture[4];
			rbShadowGBuffer = new RenderBuffer[4];
			for (int i = 0; i < rtAndGBuffer.Length; ++i)
			{
				int depthbits = i == 0 ? 32 : 0;
				rtAndGBuffer[i] = DSRenderer.CreateRenderTexture((int)cam.pixelWidth, (int)cam.pixelHeight, depthbits, RenderTextureFormat.ARGBHalf);
				rbAndGBuffer[i] = rtAndGBuffer[i].colorBuffer;
				rtShadowGBuffer[i] = DSRenderer.CreateRenderTexture((int)cam.pixelWidth, (int)cam.pixelHeight, 0, RenderTextureFormat.ARGBHalf);
				rbShadowGBuffer[i] = rtShadowGBuffer[i].colorBuffer;
			}
			matCopyGB.SetTexture("_NormalBuffer",	dscam.rtNormalBuffer);
			matCopyGB.SetTexture("_PositionBuffer",dscam.rtPositionBuffer);
			matCopyGB.SetTexture("_ColorBuffer",	dscam.rtColorBuffer);
			matCopyGB.SetTexture("_GlowBuffer",	dscam.rtGlowBuffer);
			matAnd.SetTexture("_DepthBuffer1",		rtDepth);
			matAnd.SetTexture("_NormalBuffer1",		rtShadowNormalBuffer);
			matAnd.SetTexture("_PositionBuffer1",	rtShadowPositionBuffer);
			matAnd.SetTexture("_ColorBuffer1",		rtShadowColorBuffer);
			matAnd.SetTexture("_GlowBuffer1",		rtShadowGlowBuffer);
			matAnd.SetTexture("_DepthBuffer2",		rtAndDepth);
			matAnd.SetTexture("_NormalBuffer2",		rtAndNormalBuffer);
			matAnd.SetTexture("_PositionBuffer2",	rtAndPositionBuffer);
			matAnd.SetTexture("_ColorBuffer2",		rtAndColorBuffer);
			matAnd.SetTexture("_GlowBuffer2",		rtAndGlowBuffer );
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
			Graphics.SetRenderTarget(rtAndDepth);
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

			Graphics.SetRenderTarget(rbShadowGBuffer, rtAndNormalBuffer.depthBuffer);
			matCopyGB.SetPass(0);
			DSRenderer.DrawFullscreenQuad();

			dscam.SetRenderTargetsGBuffer();
			matAnd.SetPass(0);
			DSRenderer.DrawFullscreenQuad();
		}
	}
}
