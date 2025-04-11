using UnityEngine;
public class CameraRayTraceRender : MonoBehaviour
{
    [SerializeField] private ComputeShader rayTracer;
    [SerializeField] private bool useRayTracer = true;
    [SerializeField] [Range(1, 512)] private int raysPerPixel = 1;

    private RenderTexture rayTracedTexture;
    private Camera cam;
    private ComputeBuffer sphereBuffer;
    private RayTracedSphere[] cachedSpheres;

    private int kernelHandle;
    private int iterationCount = 0;

    public static CameraRayTraceRender Instance => _instance;
    private static CameraRayTraceRender _instance;

    private void Start()
    {
        if (_instance != null) Destroy(this);
        _instance = this;

        kernelHandle = rayTracer.FindKernel("CSMain");
        cam = Camera.main;

        cachedSpheres = CollectSpheresInScene();

        InitRenderTexture();
        GetSphereDataFromObjects();
        CreateAndSetSphereBuffer();
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

        rayTracer.SetVector("_LightPosition", FindAnyObjectByType<Light>().transform.position);
        rayTracer.SetVector("_CameraPosition", cam.transform.position);
        rayTracer.SetMatrix("_CameraToWorld", cam.cameraToWorldMatrix);
        rayTracer.SetMatrix("_CameraInverseProjection", cam.projectionMatrix.inverse);
        rayTracer.SetFloat("_NumRaysPerPixel", raysPerPixel);
        rayTracer.SetInt("_IterationCount", iterationCount);

        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);

        rayTracer.Dispatch(kernelHandle, threadGroupsX, threadGroupsY, 1);

        iterationCount++;
    }

    private void CreateAndSetSphereBuffer()
    {
        Sphere[] spheres = GetSphereDataFromObjects();

        sphereBuffer ??= new(spheres.Length, System.Runtime.InteropServices.Marshal.SizeOf(typeof(Sphere)));
        sphereBuffer.SetData(spheres);

        rayTracer.SetBuffer(kernelHandle, "_Spheres", sphereBuffer);
    }

    private RayTracedSphere[] CollectSpheresInScene()
    {
        return FindObjectsByType<RayTracedSphere>(FindObjectsSortMode.InstanceID);
    }

    private Sphere[] GetSphereDataFromObjects()
    {
        Sphere[] spheres = new Sphere[cachedSpheres.Length];
        for (int i = 0; i < cachedSpheres.Length; i++) spheres[i] = cachedSpheres[i].SphereInformation;

        return spheres;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (useRayTracer) Graphics.Blit(rayTracedTexture, destination);
        else Graphics.Blit(source, destination);
    }

    private void OnDisable()
    {
        sphereBuffer.Release();
    }

    public void ResetBuffers()
    {
        CreateAndSetSphereBuffer();
    }
}
