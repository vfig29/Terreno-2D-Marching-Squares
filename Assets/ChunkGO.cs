using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGO : MonoBehaviour
{
    public Chunk loadedChunk { get; set; }
    public MeshFilter surfaceMesh { get; set; }


    private void Update()
    {
    }

    void ChangeDensity(Vector2 targetNodePoint, bool settedDensity)
    {
        loadedChunk.UpdateAllChunkMeshData();
    }

    public void UpdateMeshComponent()
    {
        Mesh thisChunkGOMesh = GetComponent<MeshFilter>().sharedMesh = loadedChunk.m_meshData.BuildMeshComponent();
        surfaceMesh.sharedMesh = loadedChunk.m_surfaceMeshData.BuildMeshComponent();
        //Setting Uvs
        UVMapper.BoxUV(surfaceMesh.sharedMesh, surfaceMesh.gameObject.transform);
        UVMapper.BoxUV(thisChunkGOMesh, transform);
    }


}
