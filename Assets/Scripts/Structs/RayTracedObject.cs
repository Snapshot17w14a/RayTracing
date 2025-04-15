using UnityEngine;

public class RayTracedObject : MonoBehaviour
{
    protected virtual void OnValidate()
    {
        CameraRayTraceRender.UpdateBuffersNextUpdate = true;
        Debug.Log(GetType() + ": OnValidate");
    }

    protected virtual void OnDestroy()
    {
        CameraRayTraceRender.UpdateBuffersNextUpdate = true;
    }
}
