using UnityEngine;

public class RayTracedMesh : MonoBehaviour
{
    [SerializeField] private RayTracedMaterial material;

    public void AddMeshToSplitter()
    {
        MeshSplitter.SplitMesh(GetComponent<MeshFilter>().sharedMesh, material, transform.localToWorldMatrix);
    }

    private void OnValidate()
    {
        CameraRayTraceRender.UpdateBuffersNextUpdate = true;
    }
}
