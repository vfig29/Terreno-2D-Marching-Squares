using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    bool m_isLoaded;
    Vector2Int m_chunkPos;
    int m_chunkHeight;
    int m_chunkWidth;
    const float baseSquareScale = 1.0f;
    Node[,] nodeMap;
    NodeDensityLoader m_nodeDensityLoader;

    public Chunk(Vector2Int chunkPos)
    {
        m_chunkPos = chunkPos;
        m_isLoaded = LoadNodes();
    }

    bool LoadNodes()
    {
        nodeMap = new Node[ConvertToNodeMapIndex(m_chunkWidth), ConvertToNodeMapIndex(m_chunkHeight)];
        for (int x = 0; x < nodeMap.Length; x++)
        {
            for (int y = 0; y < nodeMap.Length; x++)
            {
                nodeMap[x,y] = new Node(false);
                LoadNodeLoadPipeline(nodeMap[x, y]);
            }
        }
        return true;
    }
    bool LoadNodeLoadPipeline(Node n)
    {
        m_nodeDensityLoader.LoadAttribute(n);
        return true;
    }

    MeshData GetMeshData()
    {
        MeshData chunkMeshData = new MeshData();
        return chunkMeshData;
    }

    Vector2Int WorldCoordToChunkPos(Vector2 worldCoord)
    {
        int chunkPosX = Mathf.FloorToInt(worldCoord.x / m_chunkWidth);
        int chunkPosY = Mathf.FloorToInt(worldCoord.y / m_chunkHeight);
        return new Vector2Int(chunkPosX, chunkPosY);
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
        float localY = m_chunkPos.x * m_chunkHeight;
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
        internal abstract bool LoadAttribute(Node n);
    }
    class NodeDensityLoader : AbstractNodeAttributeLoader
    {
        static FastNoise densityNoise = new FastNoise(1);

        bool ApplyDensityNoise(float x, float y)
        {
            return densityNoise.GetSimplexFractal(x, y) > 0.1f;
        }

        internal override bool LoadAttribute(Node n)
        {
            n.ativo = ApplyDensityNoise(n.worldPosition.x, n.worldPosition.y);
            return true;
        }
    }

}

