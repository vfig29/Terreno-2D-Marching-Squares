using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    bool m_isLoaded;
    Vector2Int m_chunkPos;
    const int m_chunkHeight = 20;
    const int m_chunkWidth = 20;
    const float baseSquareScale = 2.0f;
    ParticleNode[,] nodeMap;
    NodeDensityLoader m_nodeDensityLoader;
    MeshData m_meshData;
    MeshData m_surfaceMeshData;

    public Chunk(Vector2Int chunkPos)
    {
        
        m_chunkPos = chunkPos;
        InstanceLoaders();
        m_isLoaded = LoadNodes();
        m_meshData = LoadMeshData();//Requer nodeMap carregado.
        m_surfaceMeshData = LoadSurfaceMeshData(); //Requer m_meshData carregado.
    }

    void InstanceLoaders()
    {
        m_nodeDensityLoader = new NodeDensityLoader(this);
    }
    bool LoadNodes()
    {
        nodeMap = new ParticleNode[ConvertToNodeMapIndex(m_chunkWidth) + 1, ConvertToNodeMapIndex(m_chunkHeight) + 1]; // faz um node extra, para compensar o vertice a menos na criação do mesh. 
        for (int x = 0; x < nodeMap.GetLength(0); x++)
        {
            for (int y = 0; y < nodeMap.GetLength(1); y++)
            {
                nodeMap[x,y] = new ParticleNode(false);
                nodeMap[x, y].localPosition = NodeMapIndexToLocalCoord(x, y);
                LoadNodeLoadPipeline(nodeMap[x, y]);
            }
        }
        return true;
    }
    bool LoadNodeLoadPipeline(ParticleNode n)
    {
        m_nodeDensityLoader.LoadAttribute(n);
        return true;
    }

    MeshData LoadMeshData()
    {
        MeshData chunkMeshData = new MeshData();
        chunkMeshData.BuildMeshData(nodeMap, baseSquareScale);
        return chunkMeshData;
    }

    MeshData LoadSurfaceMeshData()
    {
        return m_meshData.BuildSurfaceMeshData();
    }

    public GameObject CreateGO()
    {
        GameObject chunkGO = new GameObject("Chunk " + m_chunkPos.ToString());
        chunkGO.transform.position = LocalToWorldCoord(Vector2.zero);
        MeshFilter  meshFilter = chunkGO.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = m_meshData.BuildMeshComponent();
        chunkGO.AddComponent<MeshRenderer>().sharedMaterial = Resources.Load<Material>("DirtMaterial");
        UVMapper.BoxUV(meshFilter.sharedMesh, chunkGO.transform);
        //Surface:
        GameObject chunkSurfaceGO = new GameObject("Surface");
        chunkSurfaceGO.transform.SetParent(chunkGO.transform, false); 
        MeshFilter meshFilterSurface = chunkSurfaceGO.AddComponent<MeshFilter>();
        meshFilterSurface.sharedMesh = m_surfaceMeshData.BuildMeshComponent();
        UVMapper.BoxUV(meshFilterSurface.sharedMesh, chunkSurfaceGO.transform);
        chunkSurfaceGO.AddComponent<MeshRenderer>().sharedMaterial = Resources.Load<Material>("GrassMaterial"); ;
        return chunkGO;
    }

    Vector2Int WorldCoordToChunkPos(Vector2 worldCoord)
    {
        int chunkPosX = Mathf.FloorToInt(worldCoord.x / m_chunkWidth);
        int chunkPosY = Mathf.FloorToInt(worldCoord.y / m_chunkHeight);
        return new Vector2Int(chunkPosX, chunkPosY);
    }

    Vector2 NodeMapIndexToLocalCoord(int x, int y)
    {
        return new Vector2(x*baseSquareScale, y*baseSquareScale);
    }
    Vector2 LocalToWorldCoord(Vector2 localCoord)
    {
        float worldCoordX = (m_chunkPos.x * m_chunkWidth) + localCoord.x;
        float worldCoordY = (m_chunkPos.y * m_chunkHeight) + localCoord.y;
        return new Vector2(worldCoordX, worldCoordY);
    }
    Vector2 WorldCoordToLocalCoord(Vector2 worldCoord)
    {
        float localX = m_chunkPos.x * m_chunkWidth;
        float localY = m_chunkPos.y * m_chunkHeight;
        return (worldCoord - new Vector2(localX, localY));
    }
    Vector2Int LocalCoordToNodeMapCoord(Vector2 localCoord)
    {
        return new Vector2Int(ConvertToNodeMapIndex(localCoord.x), ConvertToNodeMapIndex(localCoord.y));
    }

    int ConvertToNodeMapIndex(float localCoordPoint) //Converte valores de WorldCoordinate para DensityIndex;
    {
        return Mathf.FloorToInt(localCoordPoint / baseSquareScale);
    }

    //Attribute Classes
    abstract class AbstractNodeAttributeLoader
    {
        protected Chunk nodeChunk;

        internal AbstractNodeAttributeLoader(Chunk _nodeChunk)
        {
            nodeChunk = _nodeChunk;
        }
        internal abstract bool LoadAttribute(ParticleNode n);
    }

    class NodeDensityLoader : AbstractNodeAttributeLoader
    {
        static FastNoise heightNoise = new FastNoise(WorldLoader.gameSeed);
        static FastNoise densityNoise = new FastNoise(WorldLoader.gameSeed);

        internal NodeDensityLoader(Chunk _nodeChunk) : base(_nodeChunk)
        {

        }

        bool ApplyDensityNoise(float x, float y)
        {
            return true;//densityNoise.GetSimplexFractal(x, y) < 0.3f;
        }

        bool ApplyHeightNoise(float x, float y)
        {
            float value = 50f + (Region.highestGround * Mathf.Abs(heightNoise.GetSimplexFractal(x, 0)));
            Debug.Log(value);
            return  y < value;
        }

        internal override bool LoadAttribute(ParticleNode n)
        {
            Vector2 worldCoord = nodeChunk.LocalToWorldCoord(n.localPosition);
            n.isDense = ApplyDensityNoise(worldCoord.x, worldCoord.y) && ApplyHeightNoise(worldCoord.x, worldCoord.y);

            return true;
        }
    }

}

