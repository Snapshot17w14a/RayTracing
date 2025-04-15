using System.Runtime.InteropServices;
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
    private ComputeBuffer sphereBuffer;

    private RayTracedMesh[] cachedMeshes;
    private ComputeBuffer meshInfoBuffer;
    private ComputeBuffer triBuffer;

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

        InitRenderTexture(ref rayTracedTexture);
        if (accumulateFrames) InitRenderTexture(ref accumulativeTexture);

        GetSphereDataFromObjects();
        ResetBuffers();

        isInitialized = true;
    }

    private void RunShader()
    {
        if (resetBuffersOnUpdate) UpdateBuffersNextUpdate = true;
        if (!isInitialized) InitRenderTexture(ref rayTracedTexture);

        UpdateParameters();

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

    private void InitRenderTexture(ref RenderTexture texture)
    {
        if (texture == null || texture.width != Screen.width || texture.height != Screen.height)
        {
            if (texture != null) texture.Release();

            texture = new RenderTexture(Screen.width, Screen.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);

            texture.enableRandomWrite = true;
            texture.Create();
        }
    }

    #region SphereMethods

    private void CreateAndSetSphereBuffer()
    {
        Sphere[] spheres = GetSphereDataFromObjects();

        if (spheres.Length == 0) return;

        if (sphereBuffer == null || sphereBuffer.count != spheres.Length)
        {
            sphereBuffer = new(spheres.Length, Marshal.SizeOf(typeof(Sphere)));
        }
        sphereBuffer.SetData(spheres);

        rayTracer.SetBuffer(rayTracerKernelHandle, "_Spheres", sphereBuffer);
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

        //MeshInfo[] asd = new MeshInfo[] { new MeshInfo() { firstTirangeIndex = 0, numTriangles = 1, material = new(Color.white, 0, 0, Color.clear), minBounds = new Vector3(-1, -1, -1), maxBounds = new Vector3(1, 1, 1) } };
        //Triangle[] dsa = new Triangle[] { new Triangle() { a = new Vector3(-0.5f, 0, 0), b = new Vector3(-0.5f, 0.5f, 0), c = new Vector3(0.5f, 0, 0), aNormal = new Vector3(0, 0, -1), bNormal = new Vector3(0, 0, -1) , cNormal = new Vector3(0, 0, -1) } };

        //(MeshInfo[], Triangle[]) allMeshInfo = (asd, dsa);

        if (allMeshInfo.Item1.Length == 0 || allMeshInfo.Item2.Length == 0) return;

        if (meshInfoBuffer == null || meshInfoBuffer.count != allMeshInfo.Item1.Length)
        {
            meshInfoBuffer = new(allMeshInfo.Item1.Length, Marshal.SizeOf(typeof(MeshInfo)));
        }
        meshInfoBuffer.SetData(allMeshInfo.Item1);

        if (triBuffer == null || triBuffer.count != allMeshInfo.Item2.Length)
        {
            triBuffer = new(allMeshInfo.Item2.Length, Marshal.SizeOf(typeof(Triangle)));
        }
        triBuffer.SetData(allMeshInfo.Item2);

        rayTracer.SetBuffer(rayTracerKernelHandle, "_MeshInfo", meshInfoBuffer);
        rayTracer.SetBuffer(rayTracerKernelHandle, "_Tris", triBuffer);
    }

    private RayTracedMesh[] CollectMeshesInScene()
    {
        return FindObjectsByType<RayTracedMesh>(FindObjectsSortMode.None);
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
        rayTracer.SetInt("_IterationCount", iterationCount);
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
        sphereBuffer?.Release();
        meshInfoBuffer?.Release();
        triBuffer?.Release();

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
