using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
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


	static public void RenderSubtractors(DSCamera cam)
	{
		foreach (DSSubtractor l in instances)
		{
			Graphics.DrawMeshNow(l.mesh, l.trans.localToWorldMatrix);
		}
	}


	Transform trans;
	Mesh mesh;

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
}
