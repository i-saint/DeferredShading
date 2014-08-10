using UnityEngine;
using System.Collections;

public class ParticleRenderer : MonoBehaviour
{

	void Start()
	{
	}
	
	void Update()
	{
	}

	void OnPreRender()
	{
		if(ParticleWorld.instance) {
			ParticleWorld.instance.DepthPrePass();
			ParticleWorld.instance.GBufferPass();
		}
	}

	void OnPostRender()
	{
		if (ParticleWorld.instance)
		{
			ParticleWorld.instance.TransparentPass();
		}
	}
}

