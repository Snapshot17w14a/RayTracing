using System.Collections.Generic;
using UnityEngine;

public static class MeshSplitter
{
    private static readonly List<MeshInfo> splitMeshes = new();
    private static readonly List<Triangle> triangles = new();

    private static int currentTriangleIndex = 0;
    private static readonly int maxTrisPerMesh = 12;

    private static int splitMeshCount = 0;

    public static void SplitMesh(Mesh mesh, RayTracedMaterial material)
    {
        Debug.Log("Splitting mesh: " + mesh.name);

        //MeshInfo info = new MeshInfo();
        List<MeshInfo> submeshInfos = new();

        //Set the index of the first triange, and set the index for the next split mesh
        int firstTirangeIndex = currentTriangleIndex;

        //Make sure the norals are calculated
        mesh.RecalculateNormals();

        //Cache all the necessary info from the mesh
        int[] tris = mesh.triangles;
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;

        //Create a MeshInfo to begin the loop with
        MeshInfo currentMeshInfo = new() { firstTirangeIndex = firstTirangeIndex };

        //Iterate through all tris and create a triange struct for it,
        //update the contents of currentMeshInfo to keep track of the number of triangles
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            //If the current MeshInfo has enough triangles replace it and advance the firstTriangleIndex
            if(currentMeshInfo.numTriangles >= maxTrisPerMesh)
            {
                submeshInfos.Add(currentMeshInfo);

                currentTriangleIndex += currentMeshInfo.numTriangles;
                currentMeshInfo = new()
                {
                    firstTirangeIndex = currentTriangleIndex
                };  
            }

            Triangle tri = new()
            {
                a       = vertices[tris[i    ]],
                b       = vertices[tris[i + 1]],
                c       = vertices[tris[i + 2]],

                aNormal = normals[tris[i    ]],
                bNormal = normals[tris[i + 1]],
                cNormal = normals[tris[i + 2]],
            };

            currentMeshInfo.numTriangles++;
            triangles.Add(tri);
        }

        //The last iteration of the loop will not add the last mesh object to the current mesh objects, so we add it here
        submeshInfos.Add(currentMeshInfo);
        currentTriangleIndex += currentMeshInfo.numTriangles;

        //Now iterate through all meshInfos, set the remaining data and store it in the splitMeshes list
        for (int i = 0; i < submeshInfos.Count; i++)
        {
            //Local copy of the struct
            MeshInfo info = submeshInfos[i];

            //Calculate the bounds of the submesh
            var (min, max) = CalculateBoundsForSubmesh(info);
            info.minBounds = min;
            info.maxBounds = max;

            //Add the used material to the mesh
            info.material = material;

            //Transformation matrix to render the mesh in the correct position
            info.matrixIndex = splitMeshCount;

            //Add a copy to the list
            splitMeshes.Add(info);
        }

        splitMeshCount++;
    }

    private static (Vector3 min, Vector3 max) CalculateBoundsForSubmesh(MeshInfo info)
    {
        Vector3 min = new(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new(float.MinValue, float.MinValue, float.MinValue);

        for (int i = 0; i < info.numTriangles; i++)
        {
            Triangle tri = triangles[i + info.firstTirangeIndex];

            min.Set(
                Mathf.Min(min.x, tri.a.x, tri.b.x, tri.c.x),
                Mathf.Min(min.y, tri.a.y, tri.b.y, tri.c.y),
                Mathf.Min(min.z, tri.a.z, tri.b.z, tri.c.z)
                );

            max.Set(
                Mathf.Max(max.x, tri.a.x, tri.b.x, tri.c.x),
                Mathf.Max(max.y, tri.a.y, tri.b.y, tri.c.y),
                Mathf.Max(max.z, tri.a.z, tri.b.z, tri.c.z)
                );
        }

        return (min, max);
    }

    public static (MeshInfo[], Triangle[]) FlushData()
    {
        MeshInfo[] meshInfos = splitMeshes.ToArray();
        Triangle[] trianglesArray = triangles.ToArray();

        splitMeshes.Clear();
        triangles.Clear();

        currentTriangleIndex = 0;
        splitMeshCount = 0;

        return (meshInfos, trianglesArray);
    }
}
