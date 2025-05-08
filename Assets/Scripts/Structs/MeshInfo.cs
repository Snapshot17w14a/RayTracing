using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Sequential)]
public struct MeshInfo
{
    public int firstTirangeIndex;
    public int numTriangles;
    public int matrixIndex;

    public RayTracedMaterial material;

    public Vector3 minBounds;
    public Vector3 maxBounds;

    public Vector3 padding;
}
