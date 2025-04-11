using UnityEngine;
using System;

[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
[Serializable]
public struct RayTracedMaterial
{
    [SerializeField] Color color;
    [SerializeField] float smoothness;
    Vector3 padding;

    public RayTracedMaterial(Vector4 color, float smoothness)
    {
        this.color = color;
        this.smoothness = smoothness;
        padding = Vector3.zero;
    }

    public RayTracedMaterial(Color color, float smoothness)
    {
        this.color = color;
        this.smoothness = smoothness;
        padding = Vector3.zero;
    }
}
