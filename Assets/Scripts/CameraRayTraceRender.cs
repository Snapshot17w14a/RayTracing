using UnityEngine;

public class CameraRayTraceRender : MonoBehaviour
{
    [SerializeField] private ComputeShader rayTracer;

    private RenderTexture target;

    private int kernelHandle;

    private void Start()
    {
        kernelHandle = rayTracer.FindKernel("CSMain");

        InitRenderTexture();
    }

    private void InitRenderTexture()
    {
        if (target == null || target.width != Screen.width || target.height != Screen.height)
        {
            if (target != null) target.Release();

            target = new RenderTexture(Screen.width, Screen.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);

            target.enableRandomWrite = true;
            target.Create();
        }
    }
}
