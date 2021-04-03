using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Region
{

    public static float highestExtraRelief = 200;
    public static float lowestGroundRelief = 50;
    Vector3Int regionChunkSize;
    Chunk[,] regionChunks = new Chunk[30, 30];

    public Region()
    {
        LoadRegion();
        SpawnChunks();
    }

    void LoadRegion()
    {
        LoadChunks();
    }

    void LoadChunks()
    {
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
}
