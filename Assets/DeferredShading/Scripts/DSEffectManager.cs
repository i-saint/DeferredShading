using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public abstract class DSEffectBase : ScriptableObject
{
    public bool enabled = true;

    public abstract void Construct(DSEffectManager renderer);
    public abstract void Destruct();
    public abstract void Update();
}

[RequireComponent(typeof(DSRenderer))]
public class DSEffectManager : MonoBehaviour
{
    public List<DSEffectBase> effects = new List<DSEffectBase>();
    DSRenderer dsr;

    public DSRenderer GetRenderer() { return dsr; }

    void Awake()
    {
        dsr = GetComponent<DSRenderer>();
        effects.ForEach((a) => { a.Construct(this); });
    }

    void OnDestroy()
    {
        effects.ForEach((a) => { a.Destruct(); });
    }

    void Update()
    {
        effects.ForEach((a) => { a.Update(); });
    }
}
