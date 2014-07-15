using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class DSSubtractor : MonoBehaviour
{
	static HashSet<DSSubtractor> _instances;
	public static HashSet<DSSubtractor> instances
	{
		get
		{
			if (_instances == null) { _instances = new HashSet<DSSubtractor>(); }
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


	static public void RenderAll(DSCamera cam)
	{
		if (instances.Count == 0) { return; }

		foreach (DSSubtractor l in instances)
		{
			l.matStencilWrite.SetPass(0);
			Graphics.DrawMeshNow(l.mesh, l.trans.localToWorldMatrix);
			l.matSubtractor.SetTexture("_Depth", cam.rtDepth);
			l.matSubtractor.SetPass(0);
			Graphics.DrawMeshNow(l.mesh, l.trans.localToWorldMatrix);
			l.matStencilClear.SetPass(0);
			Graphics.DrawMeshNow(l.mesh, l.trans.localToWorldMatrix);
		}
	}


	Transform trans;
	Mesh mesh;
	public Material matStencilWrite;
	public Material matStencilClear;
	public Material matSubtractor;

	//void Reset()
	//{
	//	MeshRenderer mr = GetComponent<MeshRenderer>();
	//	mr.materials = new Material[2] {
	//		Resources.Load<Material>("DSGBufferSubtractStencil"),
	//		Resources.Load<Material>("DSGBufferSubtract")
	//	};
	//}

	void Start ()
	{
		trans = GetComponent<Transform>();
		mesh = GetComponent<MeshFilter>().mesh;
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.black;
		Gizmos.DrawWireCube(transform.position, transform.localScale * 2.0f);
	}
}
