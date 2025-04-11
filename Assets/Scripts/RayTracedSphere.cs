using UnityEngine;

public class RayTracedSphere : MonoBehaviour
{
    [SerializeField] private float radius;
    [SerializeField] private RayTracedMaterial material;

    private Material meshMaterial;

    public Sphere SphereInformation => new(transform.position, radius, material);

    private void OnValidate()
    {
        transform.localScale = Vector3.one * (radius * 2);
        if (meshMaterial == null) meshMaterial = GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
        meshMaterial.color = material.color;
        if (CameraRayTraceRender.Instance != null) CameraRayTraceRender.Instance.ResetBuffers();
    }
}
