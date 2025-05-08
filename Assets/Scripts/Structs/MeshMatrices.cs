using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Sequential)]
public struct MeshMatrices
{
    public Matrix4x4 localToWorldMatrix;
    public Matrix4x4 worldToLocalMatix;
    public Matrix4x4 normalMatrix;
}