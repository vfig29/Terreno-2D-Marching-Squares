using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldLoader : MonoBehaviour
{
    public static int gameSeed;
    public Region currentRegion;
    public static WorldLoader instance;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        gameSeed = GerarSeed();
        currentRegion = new Region();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int GerarSeed()
    {
        return System.Guid.NewGuid().GetHashCode();
    }
}
