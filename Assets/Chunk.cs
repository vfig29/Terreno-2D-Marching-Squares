using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

public class Chunk
{
    bool m_isLoaded;
    Vector2Int m_chunkPos;
    const int m_chunkHeight = 20;
    const int m_chunkWidth = 20;
    const float baseSquareScale = 1.0f;
    ParticleNode[,] particleNodeMap;
    NodeDensityLoader m_nodeDensityLoader;
    public MeshData m_meshData { get; set; }
    public MeshData m_surfaceMeshData { get; set; }


    public Chunk(Vector2Int chunkPos)
    {
        
        m_chunkPos = chunkPos;
        InstanceLoaders();
        m_isLoaded = LoadNodes();
        UpdateAllChunkMeshData();
    }

    public void UpdateAllChunkMeshData()
    {
        m_meshData = LoadMeshData(); //Requer nodeMap carregado.
        m_surfaceMeshData = LoadSurfaceMeshData(); //Requer m_meshData carregado.
    }

    void InstanceLoaders()
    {
        m_nodeDensityLoader = new NodeDensityLoader(this);
    }

    bool GetRightAdjacentChunk(out Chunk _returnedChunk)
    {
        int adjacentX = m_chunkPos.x + 1;
        int regionSizeX = WorldLoader.instance.currentRegion.regionChunkSize.x;
        if (adjacentX < regionSizeX)
        {
            _returnedChunk = WorldLoader.instance.currentRegion.regionChunks[adjacentX, m_chunkPos.y];
            return true;
        }
        else
        {
            _returnedChunk = null;
            return false;
        }

    }

    bool GetLeftAdjacentChunk(out Chunk _returnedChunk)
    {
        int adjacentX = m_chunkPos.x - 1;
        if (adjacentX >= 0)
        {
            _returnedChunk = WorldLoader.instance.currentRegion.regionChunks[adjacentX, m_chunkPos.y];
            return true;
        }
        else
        {
            _returnedChunk = null;
            return false;
        }

    }

    bool GetBottomAdjacentChunk(out Chunk _returnedChunk)
    {
        int adjacentY = m_chunkPos.y - 1;
        int regionSizeY = WorldLoader.instance.currentRegion.regionChunkSize.y;
        if (adjacentY < regionSizeY)
        {
            _returnedChunk = WorldLoader.instance.currentRegion.regionChunks[m_chunkPos.x, adjacentY];
            return true;
        }
        else
        {
            _returnedChunk = null;
            return false;
        }

    }

    bool GetTopAdjacentChunk(out Chunk _returnedChunk)
    {
        int adjacentY = m_chunkPos.y + 1;
        if (adjacentY >= 0)
        {
            _returnedChunk = WorldLoader.instance.currentRegion.regionChunks[m_chunkPos.x, adjacentY];
            return true;
        }
        else
        {
            _returnedChunk = null;
            return false;
        }

    }
    bool LoadNodes()
    {
        particleNodeMap = new ParticleNode[ConvertToNodeMapIndex(m_chunkWidth), ConvertToNodeMapIndex(m_chunkHeight)]; // faz um node extra, para compensar o vertice a menos na criação do mesh. 
        for (int x = 0; x < particleNodeMap.GetLength(0); x++)
        {
            for (int y = 0; y < particleNodeMap.GetLength(1); y++)
            {
                particleNodeMap[x,y] = new ParticleNode(false, NodeMapIndexToLocalCoord(x, y));
                LoadNodeLoadPipeline(particleNodeMap[x, y]);
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
        chunkMeshData.BuildMeshData(particleNodeMap, baseSquareScale);
        return chunkMeshData;
    }

    MeshData LoadSurfaceMeshData()
    {
        return m_meshData.BuildSurfaceMeshData();
    }
    //
    public void CalculatePhysics()
    {

    }

    //
    public GameObject CreateGO()
    {
        GameObject chunkGO = new GameObject("Chunk " + m_chunkPos.ToString());
        ChunkGO chunkGOScript = chunkGO.AddComponent<ChunkGO>();
        chunkGOScript.loadedChunk = this;
        chunkGO.transform.position = m_chunkPos*NodeMapIndexToLocalCoord(m_chunkWidth - 1, m_chunkHeight - 1);
        chunkGO.AddComponent<MeshFilter>();
        chunkGO.AddComponent<MeshRenderer>().sharedMaterial = Resources.Load<Material>("DirtMaterial");
        chunkGO.AddComponent<MeshCollider>();
        //Surface:
        GameObject chunkSurfaceGO = new GameObject("Surface");
        chunkGOScript.surfaceGO = chunkSurfaceGO;
        chunkSurfaceGO.transform.SetParent(chunkGO.transform, false);
        chunkSurfaceGO.AddComponent<MeshFilter>();
        chunkSurfaceGO.AddComponent<MeshCollider>();
        chunkSurfaceGO.AddComponent<MeshRenderer>().sharedMaterial = Resources.Load<Material>("GrassMaterial");
        //UpdateMeshComponent:
        chunkGOScript.UpdateMeshComponent();
        return chunkGO;
    }

    public void SetNodeDensity(Vector2 localCoord, bool settedDensity)
    {
        Vector2Int nodeIndex = LocalCoordToNodeMapCoord(localCoord);
        Debug.Log("From Node: " + particleNodeMap[nodeIndex.x, nodeIndex.y].localPosition);
        Debug.Log("From Conversion: " + nodeIndex);
        particleNodeMap[nodeIndex.x, nodeIndex.y].isDense = settedDensity;
    }


    public static Vector2Int WorldCoordToChunkPos(Vector2 worldCoord)
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
    Vector2 WorldCoordToLocalCoord(Vector2 worldCoord)//saída é sempre numeros positivos entre 0 e o tamanho do chunk.
    {
        Vector2 chunkPos = WorldCoordToChunkPos(worldCoord);
        float localX = ( chunkPos.x * m_chunkWidth);
        float localY = (chunkPos.y * m_chunkHeight);
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
            return densityNoise.GetSimplexFractal(x, y) < 0.3f;
        }

        bool ApplyHeightNoise(float x, float y)
        {
            float maxHeight = Region.lowestGroundRelief + (Region.highestExtraRelief * Mathf.Abs(heightNoise.GetPerlinFractal(x, 0))); //Mathf.Abs(heightNoise.GetSimplexFractal(x, 0))
            return  y < maxHeight;
        }

        internal override bool LoadAttribute(ParticleNode n)
        {
            Vector2 worldCoord = nodeChunk.LocalToWorldCoord(n.localPosition);
            n.isDense = ApplyDensityNoise(worldCoord.x, worldCoord.y) && ApplyHeightNoise(worldCoord.x, worldCoord.y);

            return true;
        }
    }
}

