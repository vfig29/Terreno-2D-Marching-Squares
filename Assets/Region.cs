using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Region
{

    public static float highestExtraRelief = 200;
    public static float lowestGroundRelief = 50;
    public Vector2Int regionChunkSize;
    public Chunk[,] regionChunks = new Chunk[30, 30];

    public Region()
    {
        regionChunkSize = new Vector2Int(30, 30);
        LoadRegion();
        SpawnChunks();
    }

    void LoadRegion()
    {
        LoadChunks();
    }


    void LoadChunks()
    {
        regionChunks = new Chunk[regionChunkSize.x, regionChunkSize.y];
        for (int x = 0; x < regionChunks.GetLength(0); x++)
        {
            for (int y = 0; y < regionChunks.GetLength(1); y++)
            {
                regionChunks[x, y] = new Chunk(new Vector2Int(x, y));
            }
        }
    }
    

    void SpawnChunks()
    {
        for (int x = 0; x < regionChunks.GetLength(0); x++)
        {
            for (int y = 0; y < regionChunks.GetLength(1); y++)
            {
                regionChunks[x, y].CreateGO();
            }
        }
    }

    void UpdatePhysics()
    {
        for (int x = 0; x < regionChunks.GetLength(0); x++)
        {
            for (int y = 0; y < regionChunks.GetLength(1); y++)
            {
                regionChunks[x, y].CalculatePhysics();
            }
        }
    }
}
