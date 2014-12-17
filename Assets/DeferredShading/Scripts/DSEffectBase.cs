using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class DSEffectBase : MonoBehaviour
{
    protected DSRenderer dsr;

    protected void UpdateDSRenderer()
    {
        if (dsr == null) dsr = GetComponent<DSRenderer>();
        if (dsr == null) dsr = GetComponentInParent<DSRenderer>();
    }

    public DSRenderer GetDSRenderer()
    {
        return dsr;
    }
}
