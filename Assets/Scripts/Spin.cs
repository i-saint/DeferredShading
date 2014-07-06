using UnityEngine;
using System.Collections;

public class Spin : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		float t = Time.time * 0.15f;
		float r = 6.5f;
		transform.position = new Vector3(Mathf.Cos(t) * r, 3.0f, Mathf.Sin(t) * r);
		transform.LookAt(Vector3.zero);
	}
}
