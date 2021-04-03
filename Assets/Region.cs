using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Region
{
    int regionWidth;
    int regionHeight;
    Chunk[,] regionChunks = new Chunk[10, 10];

    public Region()
    {
        LoadRegion();
    }

    void LoadRegion()
    {
        LoadChunks();
    }

    void LoadChunks()
    {
        for (int x = 0; x < regionChunks.GetLength(0); x++)
        {
            for (int y = 0; x < regionChunks.GetLength(1); y++)
            {
                regionChunks[x, y] = new Chunk(new Vector2Int(x, y));
            }
        }
    }
}
