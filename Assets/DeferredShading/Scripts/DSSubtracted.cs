using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class DSSubtracted : MonoBehaviour
{
	static HashSet<DSSubtracted> _instances;
	public static HashSet<DSSubtracted> instances
	{
		get
		{
			if (_instances == null) { _instances = new HashSet<DSSubtracted>(); }
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


	static public void PreRenderAll(DSCamera cam)
	{
		if (instances.Count == 0) { return; }

		Graphics.SetRenderTarget(cam.rtDepth);
		GL.Clear(true, true, Color.black, 0.0f);
		foreach (DSSubtracted l in instances)
		{
			l.matReverseDepth.SetPass(0);
			Graphics.DrawMeshNow(l.mesh, l.trans.localToWorldMatrix);
		}
	}

	static public void RenderAll(DSCamera cam)
	{
		if (instances.Count == 0) { return; }

		foreach (DSSubtracted l in instances)
		{
			l.matSubtracted.SetPass(0);
			Graphics.DrawMeshNow(l.mesh, l.trans.localToWorldMatrix);
		}
	}


	Transform trans;
	Mesh mesh;
	public Material matReverseDepth;
	public Material matSubtracted;


	void Start ()
	{
		trans = GetComponent<Transform>();
		mesh = GetComponent<MeshFilter>().mesh;
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(transform.position, transform.localScale * 2.0f);
	}
}
