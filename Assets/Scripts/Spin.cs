using UnityEngine;
using System.Collections;

public class Spin : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		transform.Rotate(new Vector3(0.0f, 1.0f, 0.0f), Time.deltaTime*10.0f);
	}
}
