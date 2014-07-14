using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class DSSubtractReceiver : MonoBehaviour
{
	static HashSet<DSSubtractReceiver> _instances;
	public static HashSet<DSSubtractReceiver> instances
	{
		get
		{
			if (_instances == null) { _instances = new HashSet<DSSubtractReceiver>(); }
			return _instances;
		}
	}


	void OnEnable()
	{
		instances.Add(this);
	}

	void OnDisable()
	{
		instances.Remove(this);
	}


	static public void RenderSubtractReceiver(DSCamera cam)
	{
		if (instances.Count == 0) { return; }

		cam.matReverseDepth.SetPass(0);
		Graphics.SetRenderTarget(cam.rtDepth);
		GL.Clear(true, true, Color.black, 0.0f);
		foreach (DSSubtractReceiver l in instances)
		{
			Graphics.DrawMeshNow(l.mesh, l.trans.localToWorldMatrix);
		}
	}


	Transform trans;
	Mesh mesh;

	void Start ()
	{
		trans = GetComponent<Transform>();
		mesh = GetComponent<MeshFilter>().mesh;
	}
}
