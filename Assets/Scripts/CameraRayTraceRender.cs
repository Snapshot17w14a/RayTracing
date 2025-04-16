using UnityEngine;

[ExecuteAlways, ImageEffectAllowedInSceneView]
public class CameraRayTraceRender : MonoBehaviour
{
    [Header("Render Settings")]
    [SerializeField] private ComputeShader rayTracer;
    [SerializeField] private ComputeShader accumulativeShader;
    [SerializeField] private bool useRayTracer = true;
    [SerializeField] private bool useShaderInSceneView = false;
    [SerializeField] [Range(1, 512)] private int raysPerPixel = 1;
    [SerializeField] private int maxBouces = 1;
    [SerializeField] private bool resetBuffersOnUpdate = false;
    [SerializeField] private bool accumulateFrames = true;

    [Header("Environment Settings")]
    [SerializeField] private Color HorizonColor;
    [SerializeField] private Color SkyColor;
    [SerializeField] [Range(0, 1)] private float skyEmissionStrength = 1f;
    [SerializeField] private Color sunColor;
    [SerializeField] private float skyEyeStrength = 1f;
    [SerializeField] private float sunEmission = 1f;

    private RenderTexture accumulativeTexture;
    private RenderTexture rayTracedTexture;
    private Camera renderCamrera;

    private RayTracedSphere[] cachedSpheres;

    private RayTracedMesh[] cachedMeshes;

    private int accumulatorKernelHandle;
    private int rayTracerKernelHandle;
    private int iterationCount = 0;

    private bool isInitialized = false;

    public static bool UpdateBuffersNextUpdate { get; set; } = false;

    private void Start()
    {
        InitializeShader();
    }    

    private void Update()
    {
        RunShader();
    }

    private void InitializeShader()
    {
        rayTracerKernelHandle = rayTracer.FindKernel("CSMain");
        accumulatorKernelHandle = accumulativeShader.FindKernel("CSMain");
        cachedSpheres = CollectSpheresInScene();
        cachedMeshes = CollectMeshesInScene();
        renderCamrera = Camera.current;
        renderCamrera = renderCamrera != null ? renderCamrera : Camera.main;

        ShaderTool.InitRenderTexture(ref rayTracedTexture);
        if (accumulateFrames) ShaderTool.InitRenderTexture(ref accumulativeTexture);

        GetSphereDataFromObjects();
        ResetBuffers();

        isInitialized = true;
    }

    private void RunShader()
    {
        if (resetBuffersOnUpdate) UpdateBuffersNextUpdate = true;
        if (!isInitialized) ShaderTool.InitRenderTexture(ref rayTracedTexture);

        UpdateParameters();
        UpdateMeshObjectMatrices();

        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);

        rayTracer.Dispatch(rayTracerKernelHandle, threadGroupsX, threadGroupsY, 1);

        if (accumulateFrames && accumulativeTexture != null)
        {
            SetAccumulativeParameters();
            accumulativeShader.Dispatch(accumulatorKernelHandle, threadGroupsX, threadGroupsY, 1);
        }

        iterationCount++;
    }
    
    #region SphereMethods

    private void CreateAndSetSphereBuffer()
    {
        Sphere[] spheres = GetSphereDataFromObjects();
        if (spheres.Length == 0) return;

        ShaderTool.CreateComputeBuffer<Sphere>(spheres.Length);
        ShaderTool.SetBuffer<Sphere>(spheres);

        rayTracer.SetBuffer(rayTracerKernelHandle, "_Spheres", ShaderTool.GetComputeBuffer<Sphere>());
    }

    private RayTracedSphere[] CollectSpheresInScene()
    {
        return FindObjectsByType<RayTracedSphere>(FindObjectsSortMode.None);
    }

    private Sphere[] GetSphereDataFromObjects()
    {
        Sphere[] spheres = new Sphere[cachedSpheres.Length];
        for (int i = 0; i < cachedSpheres.Length; i++)
        {
            if (cachedSpheres[i] == null) continue;
            spheres[i] = cachedSpheres[i].SphereInformation;
        }

        return spheres;
    }

    #endregion

    #region MeshMethods

    private void CreateAndSetMeshBuffer()
    {
        foreach (var mesh in cachedMeshes) mesh.AddMeshToSplitter();
        (MeshInfo[], Triangle[]) allMeshInfo = MeshSplitter.FlushData();

        if (allMeshInfo.Item1.Length == 0 || allMeshInfo.Item2.Length == 0) return;

        ShaderTool.CreateComputeBuffer<MeshInfo>(allMeshInfo.Item1.Length);
        ShaderTool.SetBuffer(allMeshInfo.Item1);

        ShaderTool.CreateComputeBuffer<Triangle>(allMeshInfo.Item2.Length);
        ShaderTool.SetBuffer(allMeshInfo.Item2);

        rayTracer.SetBuffer(rayTracerKernelHandle, "_MeshInfo", ShaderTool.GetComputeBuffer<MeshInfo>());
        rayTracer.SetBuffer(rayTracerKernelHandle, "_Tris", ShaderTool.GetComputeBuffer<Triangle>());
    }

    private RayTracedMesh[] CollectMeshesInScene()
    {
        return FindObjectsByType<RayTracedMesh>(FindObjectsSortMode.None);
    }

    private void UpdateMeshObjectMatrices()
    {
        Matrix4x4[] localToWorldMatrices = new Matrix4x4[cachedMeshes.Length];
        Matrix4x4[] worldToLocalMatrices = new Matrix4x4[cachedMeshes.Length];

        if (cachedMeshes.Length == 0) return;

        for (int i = 0; i < cachedMeshes.Length; i++)
        {
            //localToWorldMatrices[i] = cachedMeshes[i].transform.localToWorldMatrix;
            //worldToLocalMatrices[i] = cachedMeshes[i].transform.worldToLocalMatrix;

            localToWorldMatrices[i] = Matrix4x4.identity;
            worldToLocalMatrices[i] = Matrix4x4.identity;
        }

        ShaderTool.CreateComputeBuffer<Matrix4x4>(cachedMeshes.Length);
        ShaderTool.SetBuffer(localToWorldMatrices);

        rayTracer.SetBuffer(rayTracerKernelHandle, "_LocalToWorldMatrices", ShaderTool.GetComputeBuffer<Matrix4x4>());

        ShaderTool.CreateComputeBuffer<Matrix4x4>(cachedMeshes.Length, true);
        ShaderTool.SetBuffer(worldToLocalMatrices);

        rayTracer.SetBuffer(rayTracerKernelHandle, "_WorldToLocalMatrices", ShaderTool.GetComputeBuffer<Matrix4x4>());
    }

    #endregion

    private void UpdateParameters()
    {
        rayTracer.SetTexture(rayTracerKernelHandle, "Result", rayTracedTexture);

        rayTracer.SetVector("_CameraPosition", renderCamrera.transform.position);
        rayTracer.SetVector("_HorizonColor", HorizonColor);
        rayTracer.SetVector("_SkyColor", SkyColor);
        rayTracer.SetVector("_SunDirection", FindAnyObjectByType<Light>().transform.forward.normalized);
        rayTracer.SetMatrix("_CameraToWorld", renderCamrera.cameraToWorldMatrix);
        rayTracer.SetMatrix("_CameraInverseProjection", renderCamrera.projectionMatrix.inverse);
        rayTracer.SetInt("_NumRaysPerPixel", raysPerPixel);
        rayTracer.SetInt("_IterationCount", iterationCount % int.MaxValue);
        rayTracer.SetInt("_MaxBounces", maxBouces);
        rayTracer.SetFloat("_SkyEmission", skyEmissionStrength);
        rayTracer.SetFloat("_SunEye", skyEyeStrength);
        rayTracer.SetVector("_SunColor", sunColor * sunEmission);

        if (UpdateBuffersNextUpdate) ResetBuffers();
    }

    private void SetAccumulativeParameters()
    {
        accumulativeShader.SetTexture(accumulatorKernelHandle, "Result", accumulativeTexture);
        accumulativeShader.SetTexture(accumulatorKernelHandle, "_RenderedFrame", rayTracedTexture);
        accumulativeShader.SetInt("_FrameCount", iterationCount);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (Camera.current.name == "SceneCamera" && useShaderInSceneView && useRayTracer)
        {
            if (!isInitialized) InitializeShader();
            RunShader();
            Graphics.Blit(rayTracedTexture, destination);
        }
        else if (Camera.current == Camera.main && useRayTracer) Graphics.Blit(accumulateFrames && accumulativeTexture != null ? accumulativeTexture : rayTracedTexture, destination);
        else Graphics.Blit(source, destination);
    }

    public void ResetBuffers()
    {
        Debug.Log("Resetting buffers");

        cachedSpheres = CollectSpheresInScene();
        cachedMeshes = CollectMeshesInScene();

        CreateAndSetSphereBuffer();
        CreateAndSetMeshBuffer();

        UpdateBuffersNextUpdate = false;
    }

    private void OnDisable()
    {
        isInitialized = false;
    }

    private void OnDestroy()
    {
        isInitialized = false;
    }

    private void OnValidate()
    {
        isInitialized = false;
    }
}
