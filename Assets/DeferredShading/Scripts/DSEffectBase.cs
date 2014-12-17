using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class DSEffectBase : MonoBehaviour
{
    protected DSRenderer dsr;
    protected Camera cam;

    protected void UpdateDSRenderer()
    {
        if (dsr == null) dsr = GetComponent<DSRenderer>();
        if (dsr == null) dsr = GetComponentInParent<DSRenderer>();
        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null) cam = GetComponentInParent<Camera>();
    }

    public DSRenderer GetDSRenderer()
    {
        return dsr;
    }

    public Camera GetCamera()
    {
        return cam;
    }
}
