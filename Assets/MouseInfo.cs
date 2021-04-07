using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseInfo : MonoBehaviour
{
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ModifyTerrain();
    }

    void ModifyTerrain()
    {
        if (Input.GetMouseButton(0))
        {
            if (MouseRaycast(out RaycastHit _hittedInfo))
            {
                if (GetClosestMeshVertex(_hittedInfo, out Vector3 vertexLocalCoord))
                {
                    if (_hittedInfo.transform.TryGetComponent<ChunkGO>(out ChunkGO chunkGO))
                    {
                        chunkGO.ChangeDensity(vertexLocalCoord, false);
                    }
                }
            }
        }
    }
    bool MouseRaycast(out RaycastHit _hittedInfo)
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(mouseRay, out _hittedInfo);
    }

    bool GetClosestMeshVertex(RaycastHit _hittedInfo,out Vector3 vertexLocalCoord)
    {
        vertexLocalCoord = Vector3.zero;
            MeshCollider hittedMeshCollider = _hittedInfo.collider as MeshCollider;
            if (hittedMeshCollider == null || hittedMeshCollider.sharedMesh == null)
                return false;
            Mesh hittedMesh = hittedMeshCollider.sharedMesh;
            Vector3[] meshVertices = hittedMesh.vertices;
            int[] triangleVertices = new int[3] {//através do index do triangle, pega todos o index de todos vertices desse triangulo.
                hittedMesh.triangles[_hittedInfo.triangleIndex * 3 + 0],
                hittedMesh.triangles[_hittedInfo.triangleIndex * 3 + 1],
                hittedMesh.triangles[_hittedInfo.triangleIndex * 3 + 2]
                };
            //Vector3 v1 = meshVertices[triangleVertices[0]];
            //Vector3 v2 = meshVertices[triangleVertices[1]];
            //Vector3 v3 = meshVertices[triangleVertices[2]];
            float closestDistance = Vector3.Distance(hittedMesh.vertices[triangleVertices[0]], _hittedInfo.point);
            int closestVertexIndex = triangleVertices[0];
            for (int i = 0; i < triangleVertices.Length; i++)
            {
                float distanceBetween = Vector3.Distance(hittedMesh.vertices[triangleVertices[i]], _hittedInfo.point);
                if (distanceBetween < closestDistance)
                {
                    closestDistance = distanceBetween;
                    closestVertexIndex = triangleVertices[i];
                }
            }
            vertexLocalCoord = hittedMesh.vertices[closestVertexIndex];
            return true;

    }
}
