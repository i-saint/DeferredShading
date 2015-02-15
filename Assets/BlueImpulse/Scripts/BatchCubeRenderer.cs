using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;


public class BatchCubeRenderer : CustumDataBatchRenderer<BatchCubeRenderer.InstanceData>
{

    public struct InstanceData
    {
        public const int size = 48;

        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        float local_time;
        float id;
    }



    public override void OnEnable()
    {
        SetInstanceDataSize(InstanceData.size);
        base.OnEnable();
    }
}
