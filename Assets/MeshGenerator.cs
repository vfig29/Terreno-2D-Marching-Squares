using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UIElements;

public class MeshGenerator : MonoBehaviour
{
    public int seed;
    [Header("Mesh Attributes")]
    public int alturaMesh;
    public int larguraMesh;
    public float tamanhoSquare; //valores dos lados do square.
    [Header("Terrain Attributes")]
    int alturaTerreno;
    int larguraTerreno;
    float relevoMax;
    [Range(0.0f, 1.0f)]
    public float relevoMaxPercent;
    float relevoMin;
    [Range(0.0f, 0.999f)]
    public float relevoMinPercent;
    public Node[,] densityMap;
    [Header("HeightMap Noise Attributes")]
    public float scale;
    public float lacunarity;
    public float gain;
    public int octaves;
    public FastNoise.FractalType fractalType;
    public FastNoise.NoiseType noiseType;
    [Header("DensityMap Noise Attributes")]
    public float scaleD;
    public float lacunarityD;
    public float gainD;
    public int octavesD;
    public FastNoise.FractalType fractalTypeD;
    public FastNoise.NoiseType noiseTypeD;
    [Range(0.0f, 1.0f)]
    public float densityFloat;
    void Start()
    {
        GerarSeed();
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Init();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            GerarSeed();
        }
    }

    public void Init()
    {
        //DateTime ini = DateTime.Now;
        UnityEngine.Random.InitState(seed);
        //relevoMin = UnityEngine.Random.Range(0, alturaMesh / 2);
        //relevoMax = UnityEngine.Random.Range(alturaMesh / 2, alturaMesh);
        alturaTerreno = (int)(alturaMesh / tamanhoSquare);
        larguraTerreno = (int)(larguraMesh / tamanhoSquare);
        relevoMin = alturaTerreno*relevoMinPercent;
        relevoMax = alturaTerreno*relevoMaxPercent;
        densityMap = GerarDensityMap();
        GerarMesh(densityMap);
        //print(DateTime.Now - ini);
    }

    public void GerarSeed()
    {
        seed = System.Guid.NewGuid().GetHashCode();
    }

    public MeshData GerarMesh(Node[,] densityMap)
    {
        MeshData meshData = new MeshData();
        //definindo os vertices
        for (int i = 0; i < densityMap.GetLength(0) - 1; i++)
        {
            for (int j = 0; j < densityMap.GetLength(1) - 1; j++)
            {
                    Square square = new Square(tamanhoSquare);
                    densityMap[i, j + 1].worldPosition = new Vector3(i * tamanhoSquare, (j + 1) * tamanhoSquare, 0);
                    square.setUpLeft(densityMap[i, j + 1]);
                    densityMap[i + 1, j + 1].worldPosition = new Vector3((i + 1) * tamanhoSquare, (j + 1) * tamanhoSquare, 0);
                    square.setUpRight(densityMap[i + 1 , j + 1]);
                    densityMap[i, j].worldPosition = new Vector3(i * tamanhoSquare, j * tamanhoSquare, 0);
                    square.setBottomLeft(densityMap[i, j]);
                    densityMap[i + 1, j].worldPosition = new Vector3((i + 1) * tamanhoSquare, (j) * tamanhoSquare, 0);
                    square.setBottomRight(densityMap[i + 1, j]);
                    square.AddInMeshData(meshData);

            }

        }
        return meshData;
    }
    float[] GerarHeightMap()
    {
        float[] heightMap = new float[larguraTerreno]; //largura dividido pelo tamanho do cubo x 2(pois são 2 vertices por eixo)
        FastNoise noise = new FastNoise(seed);
        noise.SetFractalOctaves(octaves);
        noise.SetFractalLacunarity(lacunarity);
        noise.SetFractalGain(gain);
        noise.SetFractalType(fractalType);
        noise.SetNoiseType(noiseType);
        for (int i = 0; i < heightMap.Length; i++)
        {
            heightMap[i] = noise.GetNoise(i / scale, 0);
        }

        return heightMap;
    }

    Node[,] GerarDensityMap()
    {
        //Gerando heightMap;
        float[] heightMap = GerarHeightMap();
        Node[,] densityMap = new Node[larguraTerreno , alturaTerreno];
        FastNoise noise = new FastNoise((seed - 10) * 2);
        noise.SetFractalOctaves(octavesD);
        noise.SetFractalLacunarity(lacunarityD);
        noise.SetFractalGain(gainD);
        noise.SetFractalType(fractalTypeD);
        noise.SetNoiseType(noiseTypeD);
        for (int i = 0; i < densityMap.GetLength(0); i++)
        {
            float altura = Mathf.Lerp(relevoMin, relevoMax, heightMap[i]);
            for (int j = 0; j < densityMap.GetLength(1); j++)
            {
                densityMap[i, j] = new Node(false);
                if (j < altura)
                {
                    if (noise.GetNoise(i, j) < densityFloat)
                    {   
                        densityMap[i, j].ativo = true;
                    }
                }
            }
        }


        return densityMap;
    }

}

public class MeshData
{
    public List<int> triangulos;
    Dictionary<Vector3, VertexData> vertexIndexes;
    //
    private int altura;
    private int largura;
    //
    public struct VertexData
    {
        public int index;
        public List<int> triangles;
    }
    public MeshData()
    {
        triangulos = new List<int>();
        vertexIndexes = new Dictionary<Vector3, VertexData>(new Vector3CoordComparer());
    }

    VertexData AssignVertice(Vector3 coord)
    {
        if (!vertexIndexes.TryGetValue(coord, out VertexData vIndex))
        {
            VertexData vertexDataCriado;
            vertexDataCriado.index = vertexIndexes.Count;
            vertexDataCriado.triangles = new List<int>();
            vertexIndexes.Add(coord, vertexDataCriado);
            return vertexDataCriado;
        }
        else
        {
            return vIndex;
        }

    }

    public void AddTriangleSimultaneo(Vector3 a, Vector3 b, Vector3 c)
    {
        AddTriangle(AssignVertice(a), AssignVertice(b), AssignVertice(c));

    }

    public void AddTriangle(VertexData a, VertexData b, VertexData c)
    {
        a.triangles.Add(Mathf.FloorToInt(triangulos.Count / 3));
        triangulos.Add(a.index);
        b.triangles.Add(Mathf.FloorToInt(triangulos.Count / 3));
        triangulos.Add(b.index);
        c.triangles.Add(Mathf.FloorToInt(triangulos.Count / 3));
        triangulos.Add(c.index);
    }

    public Mesh CriarMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertexIndexes.Keys.ToArray();
        mesh.triangles = triangulos.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }
    bool SharedTriangles(VertexData verticeA, VertexData verticeB)
    {
        int shared = 0;
        foreach (int trianguloA in verticeA.triangles)
        {
            foreach (int trianguloB in verticeB.triangles)
            {
                if (trianguloA == trianguloB)
                {
                    shared++;
                }
                if (shared > 1)
                {
                    return false;
                }
            }

        }
        return true;
    }

    void TriangulateBorder(Vector3 coordA, Vector3 coordB, float profundidadeQuadSide)
    {
        int quadsPorX = 1;
        for (int i = 1; i <= quadsPorX; i++)
        {
            AddTriangleSimultaneo(coordA, coordA + new Vector3(0, 0, i*profundidadeQuadSide), coordB);
            AddTriangleSimultaneo(coordB, coordA + new Vector3(0, 0, i*profundidadeQuadSide), coordB + new Vector3(0, 0, i*profundidadeQuadSide));
        }
    }
    

    public MeshData CalculateSurfaceMesh()
    {
        int profundidadeQuadSize = 20;
        MeshData surfaceMesh = new MeshData();
        VertexData[] vertexDataArray = vertexIndexes.Values.ToArray<VertexData>();
        Vector3[] coordArray = vertexIndexes.Keys.ToArray();
        for (int i = 0; i < triangulos.Count; i+=3)
        {
            if (SharedTriangles(vertexDataArray[triangulos[i]], vertexDataArray[triangulos[i+1]]))
            {
                surfaceMesh.TriangulateBorder(coordArray[triangulos[i]], coordArray[triangulos[i + 1]], profundidadeQuadSize);
            }
            if (SharedTriangles(vertexDataArray[triangulos[i +1]], vertexDataArray[triangulos[i + 2]]))
            {
                surfaceMesh.TriangulateBorder(coordArray[triangulos[i+1]], coordArray[triangulos[i + 2]], profundidadeQuadSize);
            }
            if (SharedTriangles(vertexDataArray[triangulos[i]], vertexDataArray[triangulos[i + 2]]))
            {
                surfaceMesh.TriangulateBorder(coordArray[triangulos[i + 2]], coordArray[triangulos[i]], profundidadeQuadSize);
            }
        }
        return surfaceMesh;
    }

    public int GetQtdTriangles()
    {
        return triangulos.Count;
    }
}

public class Square
{
    float size;
    //vertice:
    Node upLeft, upRight, bottomLeft, bottomRight;
    Node bottomCenter, rightCenter, upCenter, leftCenter;
    int configuration;

    public Square(float _size)
    {
        size = _size;
        configuration = 0;
    }
    public void setUpLeft(Node node)
    {
        upLeft = node;
        upCenter = new Node(true, node.worldPosition + (Vector3.right * size / 2));
        leftCenter = new Node(true, node.worldPosition + (Vector3.down * size / 2));
        if (node.ativo)
        {
            configuration += 8;
        }
    }
    public void setUpRight(Node node)
    {
        upRight = node;
        upCenter = new Node(true, node.worldPosition + (Vector3.left * size / 2));
        rightCenter = new Node(true, node.worldPosition + (Vector3.down * size / 2));
        if (node.ativo)
        {
            configuration += 4;
        }
    }
    public void setBottomRight(Node node)
    {
        bottomRight = node;
        rightCenter = new Node(true, node.worldPosition + Vector3.up * size / 2);
        bottomCenter = new Node(true, node.worldPosition + Vector3.left * size / 2);
        if (node.ativo)
        {
            configuration += 2;
        }
    }

    public void setBottomLeft(Node node)
    {
        bottomLeft = node;
        leftCenter = new Node(true, node.worldPosition + Vector3.up * size / 2);
        bottomCenter = new Node(true, node.worldPosition + Vector3.right * size / 2);
        if (node.ativo)
        {
            configuration += 1;
        }
    }

    public void AddInMeshData(MeshData meshdata)
    {
        switch (configuration)
        {
            case 1://conferido//conferido
                meshdata.AddTriangleSimultaneo(bottomCenter.GetPosicao(), bottomLeft.GetPosicao(), leftCenter.GetPosicao());
                break;
            case 2://conferido //conferido
                meshdata.AddTriangleSimultaneo(rightCenter.GetPosicao(), bottomRight.GetPosicao(), bottomCenter.GetPosicao());
                break;
            case 3://conferido//conferido
                meshdata.AddTriangleSimultaneo(leftCenter.GetPosicao(), rightCenter.GetPosicao(), bottomLeft.GetPosicao());
                meshdata.AddTriangleSimultaneo(rightCenter.GetPosicao(), bottomRight.GetPosicao(), bottomLeft.GetPosicao());
                break;
            case 4://conferido //conferido
                meshdata.AddTriangleSimultaneo(upCenter.GetPosicao(), upRight.GetPosicao(), rightCenter.GetPosicao());
                break;
            case 5://conferido//conferido
                meshdata.AddTriangleSimultaneo(upCenter.GetPosicao(), upRight.GetPosicao(), rightCenter.GetPosicao());
                meshdata.AddTriangleSimultaneo(upCenter.GetPosicao(), rightCenter.GetPosicao(), bottomCenter.GetPosicao());
                meshdata.AddTriangleSimultaneo(bottomLeft.GetPosicao(), upCenter.GetPosicao(), bottomCenter.GetPosicao());
                meshdata.AddTriangleSimultaneo(leftCenter.GetPosicao(), upCenter.GetPosicao(), bottomLeft.GetPosicao());
                break;
            case 6://conferido //conferido
                meshdata.AddTriangleSimultaneo(upCenter.GetPosicao(), upRight.GetPosicao(), bottomRight.GetPosicao());
                meshdata.AddTriangleSimultaneo(bottomCenter.GetPosicao(), upCenter.GetPosicao(), bottomRight.GetPosicao());
                break;
            case 7://conferido//conferido
                meshdata.AddTriangleSimultaneo(bottomRight.GetPosicao(), upCenter.GetPosicao(), upRight.GetPosicao());
                meshdata.AddTriangleSimultaneo(bottomLeft.GetPosicao(), upCenter.GetPosicao(), bottomRight.GetPosicao());
                meshdata.AddTriangleSimultaneo(leftCenter.GetPosicao(), upCenter.GetPosicao(), bottomLeft.GetPosicao());
                break;
            case 8://conferido //conferido
                meshdata.AddTriangleSimultaneo(leftCenter.GetPosicao(), upLeft.GetPosicao(), upCenter.GetPosicao());
                break;
            case 9://conferido //conferido
                meshdata.AddTriangleSimultaneo(upLeft.GetPosicao(), bottomCenter.GetPosicao(), bottomLeft.GetPosicao());
                meshdata.AddTriangleSimultaneo(upCenter.GetPosicao(), bottomCenter.GetPosicao(), upLeft.GetPosicao());
                break;
            case 10://conferido//conferido
                meshdata.AddTriangleSimultaneo(upCenter.GetPosicao(), rightCenter.GetPosicao(), upLeft.GetPosicao());
                meshdata.AddTriangleSimultaneo(rightCenter.GetPosicao(), bottomRight.GetPosicao(), upLeft.GetPosicao());
                meshdata.AddTriangleSimultaneo(upLeft.GetPosicao(), bottomRight.GetPosicao(), bottomCenter.GetPosicao());
                meshdata.AddTriangleSimultaneo(upLeft.GetPosicao(), bottomCenter.GetPosicao(), leftCenter.GetPosicao());
                break;
            case 11://conferido //conferido
                meshdata.AddTriangleSimultaneo(bottomRight.GetPosicao(), bottomLeft.GetPosicao(), upLeft.GetPosicao());
                meshdata.AddTriangleSimultaneo(rightCenter.GetPosicao(), bottomRight.GetPosicao(), upLeft.GetPosicao());
                meshdata.AddTriangleSimultaneo(upCenter.GetPosicao(), rightCenter.GetPosicao(), upLeft.GetPosicao());
                break;
            case 12://conferido //conferido
                meshdata.AddTriangleSimultaneo(rightCenter.GetPosicao(), leftCenter.GetPosicao(), upLeft.GetPosicao());
                meshdata.AddTriangleSimultaneo(upRight.GetPosicao(), rightCenter.GetPosicao(), upLeft.GetPosicao());
                break;
            case 13://conferido //conferido
                meshdata.AddTriangleSimultaneo(upRight.GetPosicao(), rightCenter.GetPosicao(), upLeft.GetPosicao());
                meshdata.AddTriangleSimultaneo(rightCenter.GetPosicao(), bottomCenter.GetPosicao(), upLeft.GetPosicao());
                meshdata.AddTriangleSimultaneo(upLeft.GetPosicao(), bottomCenter.GetPosicao(), bottomLeft.GetPosicao());
                break;
            case 14://conferido //conferido
                meshdata.AddTriangleSimultaneo(upRight.GetPosicao(), bottomRight.GetPosicao(), upLeft.GetPosicao());
                meshdata.AddTriangleSimultaneo(bottomRight.GetPosicao(), bottomCenter.GetPosicao(), upLeft.GetPosicao());
                meshdata.AddTriangleSimultaneo(bottomCenter.GetPosicao(), leftCenter.GetPosicao(), upLeft.GetPosicao());
                break;
            case 15://conferido //conferido
                meshdata.AddTriangleSimultaneo(upRight.GetPosicao(), bottomRight.GetPosicao(), upLeft.GetPosicao());
                meshdata.AddTriangleSimultaneo(bottomRight.GetPosicao(), bottomLeft.GetPosicao(), upLeft.GetPosicao());
                break;

        }
        
    }


}

public class Node
{
    public bool ativo { get; set; }
    public Vector3 worldPosition { get; set; }

    public Node(bool _ativo)
    {
        ativo = _ativo;
    }
    public Node(bool _ativo, Vector3 _posicao)
    {
        ativo = _ativo;
        if (ativo == true)
        {
            worldPosition = _posicao;
        }
    }
    public Vector3 GetPosicao()
    {
        return worldPosition;
    }

}

class Vector3CoordComparer : IEqualityComparer<Vector3>
{
    public bool Equals(Vector3 a, Vector3 b)
    {
        if (Mathf.Abs(a.x - b.x) > 0.001) return false;
        if (Mathf.Abs(a.y - b.y) > 0.001) return false;
        if (Mathf.Abs(a.z - b.z) > 0.001) return false;

        return true; //indeed, very close
    }

    public int GetHashCode(Vector3 obj)
    {
        //a cruder than default comparison, allows to compare very close-vector3's into same hash-code.
        return Math.Round(obj.x, 3).GetHashCode()
             ^ Math.Round(obj.y, 3).GetHashCode() << 2
             ^ Math.Round(obj.z, 3).GetHashCode() >> 2;
    }
}

