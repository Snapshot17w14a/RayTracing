using UnityEngine;

public class RayTracedMesh : RayTracedObject
{
    [SerializeField] private RayTracedMaterial material;

    public void AddMeshToSplitter()
    {
        MeshSplitter.SplitMesh(GetComponent<MeshFilter>().sharedMesh, material);
    }
}
