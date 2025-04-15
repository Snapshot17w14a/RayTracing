using UnityEngine;

public class RayTracedMesh : RayTracedObject
{
    [SerializeField] private RayTracedMaterial material;

    public void AddMeshToSplitter()
    {
        MeshSplitter.SplitMesh(GetComponent<MeshFilter>().sharedMesh, material, transform.localToWorldMatrix);
    }

    protected override void OnValidate()
    {
        base.OnValidate();
    }
}
