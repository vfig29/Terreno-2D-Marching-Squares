using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGO : MonoBehaviour
{
    public Chunk loadedChunk { get; set; }
    public GameObject surfaceGO { get; set; }


    private void Update()
    {
    }

    public void ChangeDensity(Vector2 targetNodeLocalCoord, bool settedDensity)
    {
        loadedChunk.SetNodeDensity(targetNodeLocalCoord, settedDensity);
        loadedChunk.UpdateAllChunkMeshData();
        UpdateMeshComponent();
    }

    public void UpdateMeshComponent()
    {
        Mesh thisChunkGOMesh = GetComponent<MeshFilter>().sharedMesh = loadedChunk.m_meshData.BuildMeshComponent();
        Mesh surfaceGOMesh = surfaceGO.GetComponent<MeshFilter>().sharedMesh = loadedChunk.m_surfaceMeshData.BuildMeshComponent();
        //Setting Colliders
        GetComponent<MeshCollider>().sharedMesh = thisChunkGOMesh;
        surfaceGO.GetComponent<MeshCollider>().sharedMesh = surfaceGOMesh;
        //Setting Uvs
        UVMapper.BoxUV(surfaceGOMesh, surfaceGO.transform);
        UVMapper.BoxUV(thisChunkGOMesh, transform);
    }


}
