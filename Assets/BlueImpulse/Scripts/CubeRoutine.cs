using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BatchCubeRenderer))]
public class CubeRoutine : MonoBehaviour
{
    public BatchCubeRenderer.InstanceData[] m_instances;
    protected BatchCubeRenderer m_renderer;


    public virtual void OnEnable()
    {
        m_renderer = GetComponent<BatchCubeRenderer>();
    }
}
