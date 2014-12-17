using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class DSEffectBase : MonoBehaviour, ISerializationCallbackReceiver
{
    protected DSRenderer dsr;
    protected Camera cam;
    bool reloaded = false;

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

    public virtual void OnBeforeSerialize() {}
    public virtual void OnAfterDeserialize() { reloaded = true; }


    public virtual void Awake()
    {
        UpdateDSRenderer();
        OnReload();
        reloaded = false;
    }


    public virtual void Update()
    {
        if (reloaded)
        {
            OnReload();
            reloaded = false;
        }
    }

    public virtual void OnReload()
    {
    }
}



