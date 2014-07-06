using UnityEngine;
using System.Collections;

public class Spin : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		transform.position = new Vector3(Mathf.Cos(Time.time) * 5.0f, 3.0f, Mathf.Sin(Time.time) * 5.0f);
		transform.LookAt(Vector3.zero);
	}
}
