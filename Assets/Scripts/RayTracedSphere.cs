using UnityEngine;

public class RayTracedSphere : MonoBehaviour
{
    [SerializeField] private float radius;
    [SerializeField] private RayTracedMaterial material;

    public Sphere SphereInformation => new(transform.position, radius, material);

    private void OnValidate()
    {
        transform.localScale = Vector3.one * (radius * 2);
        if (CameraRayTraceRender.Instance != null) CameraRayTraceRender.Instance.ResetBuffers();
    }
}
