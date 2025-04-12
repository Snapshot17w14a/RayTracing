using UnityEngine;
using System;

[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
[Serializable]
public struct RayTracedMaterial
{
    [SerializeField] public Color color;
    [SerializeField] float smoothness;
    [SerializeField] float emission;
    [SerializeField] Color emissionColor;
    Vector2 padding;

    public RayTracedMaterial(Vector4 color, float smoothness, float emission, Color emissionColor)
    {
        this.color = color;
        this.smoothness = smoothness;
        this.emission = emission;
        this.emissionColor = emissionColor;
        padding = Vector3.zero;
    }

    public RayTracedMaterial(Color color, float smoothness, float emission, Color emissionColor)
    {
        this.color = color;
        this.smoothness = smoothness;
        this.emission = emission;
        this.emissionColor = emissionColor;
        padding = Vector3.zero;
    }
}
