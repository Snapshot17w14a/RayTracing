using UnityEngine;

public class CameraRayTraceRender : MonoBehaviour
{
    [SerializeField] private ComputeShader rayTracer;

    private RenderTexture rayTracedTexture;
    private Camera cam;

    private int kernelHandle;

    private void Start()
    {
        kernelHandle = rayTracer.FindKernel("CSMain");
        cam = Camera.main;

        InitRenderTexture();
    }

    private void InitRenderTexture()
    {
        if (rayTracedTexture == null || rayTracedTexture.width != Screen.width || rayTracedTexture.height != Screen.height)
        {
            if (rayTracedTexture != null) rayTracedTexture.Release();

            rayTracedTexture = new RenderTexture(Screen.width, Screen.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);

            rayTracedTexture.enableRandomWrite = true;
            rayTracedTexture.Create();
        }
    }

    private void Update()
    {
        InitRenderTexture();

        rayTracer.SetTexture(kernelHandle, "Result", rayTracedTexture);

        rayTracer.SetVector("_CameraPosition", cam.transform.position);
        rayTracer.SetMatrix("_CameraToWorld", cam.cameraToWorldMatrix);
        rayTracer.SetMatrix("_CameraInverseProjection", cam.projectionMatrix.inverse);

        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);

        rayTracer.Dispatch(kernelHandle, threadGroupsX, threadGroupsY, 1);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(rayTracedTexture, destination);
    }
}
