using UnityEngine;

public struct MeshInfo
{
    public int firstTirangeIndex;
    public int numTriangles;

    public RayTracedMaterial material;

    public Vector3 minBounds;
    public Vector3 maxBounds;

    public Matrix4x4 localToWorldMatrix;
}
