using UnityEngine;

[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
public struct Sphere
{
    Vector3 position;
    float radius;

    RayTracedMaterial material;

    public Sphere(Vector3 position, float radius, RayTracedMaterial material)
    {
        this.position = position;
        this.radius = radius;
        this.material = material;
    }
}
