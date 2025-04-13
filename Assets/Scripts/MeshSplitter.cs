using System.Collections.Generic;
using UnityEngine;

public static class MeshSplitter
{
    private static List<MeshInfo> splitMeshes = new();
    private static List<Triangle> triangles = new();

    private static int currentTriangleIndex = 0;

    public static void SplitMesh(Mesh mesh, RayTracedMaterial material, Matrix4x4 localToWorldMatrix)
    {
        MeshInfo info = new MeshInfo();

        //Set the index of the first triange, and set the index for the next split mesh
        info.firstTirangeIndex = currentTriangleIndex;
        info.numTriangles = mesh.triangles.Length / 3;
        currentTriangleIndex = mesh.triangles.Length / 3;

        //Make sure the norals are calculated
        mesh.RecalculateNormals();

        int[] tris = mesh.triangles;
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;

        //Iterate through all tris and create a triange struct for it
        for(int i = 0; i < mesh.triangles.Length - 3; i += 3)
        {
            Triangle tri = new Triangle();

            tri.a = vertices[tris[i]];
            tri.b = vertices[tris[i + 1]];
            tri.c = vertices[tris[i + 2]];

            tri.aNormal = normals[tris[i]];
            tri.bNormal = normals[tris[i + 1]];
            tri.cNormal = normals[tris[i + 2]];

            triangles.Add(tri);
        }

        //Get the bounds of the mesh and store it in the info
        Bounds meshBounds = mesh.bounds;
        info.minBounds = meshBounds.min;
        info.maxBounds = meshBounds.max;

        //Add the used material to the info
        info.material = material;

        info.localToWorldMatrix = localToWorldMatrix;

        splitMeshes.Add(info);
    }

    public static (MeshInfo[], Triangle[]) FlushData()
    {
        MeshInfo[] meshInfos = splitMeshes.ToArray();
        Triangle[] trianglesArray = triangles.ToArray();

        splitMeshes.Clear();
        triangles.Clear();

        currentTriangleIndex = 0;

        return (meshInfos, trianglesArray);
    }
}
