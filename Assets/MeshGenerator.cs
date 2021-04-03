using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UIElements;

public class MeshGenerator : MonoBehaviour
{
    
    public int GerarSeed()
    {
        return System.Guid.NewGuid().GetHashCode();
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

    public void BuildMeshData(ParticleNode[,] densityMap, float tamanhoSquare)
    {
        //definindo os vertices
        for (int i = 0; i < densityMap.GetLength(0) - 1; i++)
        {
            for (int j = 0; j < densityMap.GetLength(1) - 1; j++)
            {
                Square square = new Square(tamanhoSquare);
                densityMap[i, j + 1].localPosition = new Vector3(i * tamanhoSquare, (j + 1) * tamanhoSquare, 0);
                square.setUpLeft(densityMap[i, j + 1]);
                densityMap[i + 1, j + 1].localPosition = new Vector3((i + 1) * tamanhoSquare, (j + 1) * tamanhoSquare, 0);
                square.setUpRight(densityMap[i + 1, j + 1]);
                densityMap[i, j].localPosition = new Vector3(i * tamanhoSquare, j * tamanhoSquare, 0);
                square.setBottomLeft(densityMap[i, j]);
                densityMap[i + 1, j].localPosition = new Vector3((i + 1) * tamanhoSquare, (j) * tamanhoSquare, 0);
                square.setBottomRight(densityMap[i + 1, j]);
                square.AddInMeshData(this);
            }
        }
    }

    public MeshData BuildSurfaceMeshData()
    {
        int profundidadeQuadSize = 20;
        MeshData surfaceMesh = new MeshData();
        VertexData[] vertexDataArray = vertexIndexes.Values.ToArray<VertexData>();
        Vector3[] coordArray = vertexIndexes.Keys.ToArray();
        for (int i = 0; i < triangulos.Count; i += 3)
        {
            if (SharedTriangles(vertexDataArray[triangulos[i]], vertexDataArray[triangulos[i + 1]]))
            {
                surfaceMesh.TriangulateBorder(coordArray[triangulos[i]], coordArray[triangulos[i + 1]], profundidadeQuadSize);
            }
            if (SharedTriangles(vertexDataArray[triangulos[i + 1]], vertexDataArray[triangulos[i + 2]]))
            {
                surfaceMesh.TriangulateBorder(coordArray[triangulos[i + 1]], coordArray[triangulos[i + 2]], profundidadeQuadSize);
            }
            if (SharedTriangles(vertexDataArray[triangulos[i]], vertexDataArray[triangulos[i + 2]]))
            {
                surfaceMesh.TriangulateBorder(coordArray[triangulos[i + 2]], coordArray[triangulos[i]], profundidadeQuadSize);
            }
        }
        return surfaceMesh;
    }

    public Mesh BuildMeshComponent()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertexIndexes.Keys.ToArray();
        mesh.triangles = triangulos.ToArray();
        mesh.RecalculateNormals();
        return mesh;
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

    public int GetQtdTriangles()
    {
        return triangulos.Count;
    }
}

public class Square
{
    float size;
    //vertice:
    ParticleNode upLeft, upRight, bottomLeft, bottomRight;
    ParticleNode bottomCenter, rightCenter, upCenter, leftCenter;
    int configuration;

    public Square(float _size)
    {
        size = _size;
        configuration = 0;
    }
    public void setUpLeft(ParticleNode node)
    {
        upLeft = node;
        upCenter = new ParticleNode(true, node.localPosition + (Vector3.right * size / 2));
        leftCenter = new ParticleNode(true, node.localPosition + (Vector3.down * size / 2));
        if (node.isDense)
        {
            configuration += 8;
        }
    }
    public void setUpRight(ParticleNode node)
    {
        upRight = node;
        upCenter = new ParticleNode(true, node.localPosition + (Vector3.left * size / 2));
        rightCenter = new ParticleNode(true, node.localPosition + (Vector3.down * size / 2));
        if (node.isDense)
        {
            configuration += 4;
        }
    }
    public void setBottomRight(ParticleNode node)
    {
        bottomRight = node;
        rightCenter = new ParticleNode(true, node.localPosition + Vector3.up * size / 2);
        bottomCenter = new ParticleNode(true, node.localPosition + Vector3.left * size / 2);
        if (node.isDense)
        {
            configuration += 2;
        }
    }

    public void setBottomLeft(ParticleNode node)
    {
        bottomLeft = node;
        leftCenter = new ParticleNode(true, node.localPosition + Vector3.up * size / 2);
        bottomCenter = new ParticleNode(true, node.localPosition + Vector3.right * size / 2);
        if (node.isDense)
        {
            configuration += 1;
        }
    }

    public void AddInMeshData(MeshData meshdata)
    {
        switch (configuration)
        {
            case 1://conferido//conferido
                meshdata.AddTriangleSimultaneo(bottomCenter.GetLocalPosition(), bottomLeft.GetLocalPosition(), leftCenter.GetLocalPosition());
                break;
            case 2://conferido //conferido
                meshdata.AddTriangleSimultaneo(rightCenter.GetLocalPosition(), bottomRight.GetLocalPosition(), bottomCenter.GetLocalPosition());
                break;
            case 3://conferido//conferido
                meshdata.AddTriangleSimultaneo(leftCenter.GetLocalPosition(), rightCenter.GetLocalPosition(), bottomLeft.GetLocalPosition());
                meshdata.AddTriangleSimultaneo(rightCenter.GetLocalPosition(), bottomRight.GetLocalPosition(), bottomLeft.GetLocalPosition());
                break;
            case 4://conferido //conferido
                meshdata.AddTriangleSimultaneo(upCenter.GetLocalPosition(), upRight.GetLocalPosition(), rightCenter.GetLocalPosition());
                break;
            case 5://conferido//conferido
                meshdata.AddTriangleSimultaneo(upCenter.GetLocalPosition(), upRight.GetLocalPosition(), rightCenter.GetLocalPosition());
                meshdata.AddTriangleSimultaneo(upCenter.GetLocalPosition(), rightCenter.GetLocalPosition(), bottomCenter.GetLocalPosition());
                meshdata.AddTriangleSimultaneo(bottomLeft.GetLocalPosition(), upCenter.GetLocalPosition(), bottomCenter.GetLocalPosition());
                meshdata.AddTriangleSimultaneo(leftCenter.GetLocalPosition(), upCenter.GetLocalPosition(), bottomLeft.GetLocalPosition());
                break;
            case 6://conferido //conferido
                meshdata.AddTriangleSimultaneo(upCenter.GetLocalPosition(), upRight.GetLocalPosition(), bottomRight.GetLocalPosition());
                meshdata.AddTriangleSimultaneo(bottomCenter.GetLocalPosition(), upCenter.GetLocalPosition(), bottomRight.GetLocalPosition());
                break;
            case 7://conferido//conferido
                meshdata.AddTriangleSimultaneo(bottomRight.GetLocalPosition(), upCenter.GetLocalPosition(), upRight.GetLocalPosition());
                meshdata.AddTriangleSimultaneo(bottomLeft.GetLocalPosition(), upCenter.GetLocalPosition(), bottomRight.GetLocalPosition());
                meshdata.AddTriangleSimultaneo(leftCenter.GetLocalPosition(), upCenter.GetLocalPosition(), bottomLeft.GetLocalPosition());
                break;
            case 8://conferido //conferido
                meshdata.AddTriangleSimultaneo(leftCenter.GetLocalPosition(), upLeft.GetLocalPosition(), upCenter.GetLocalPosition());
                break;
            case 9://conferido //conferido
                meshdata.AddTriangleSimultaneo(upLeft.GetLocalPosition(), bottomCenter.GetLocalPosition(), bottomLeft.GetLocalPosition());
                meshdata.AddTriangleSimultaneo(upCenter.GetLocalPosition(), bottomCenter.GetLocalPosition(), upLeft.GetLocalPosition());
                break;
            case 10://conferido//conferido
                meshdata.AddTriangleSimultaneo(upCenter.GetLocalPosition(), rightCenter.GetLocalPosition(), upLeft.GetLocalPosition());
                meshdata.AddTriangleSimultaneo(rightCenter.GetLocalPosition(), bottomRight.GetLocalPosition(), upLeft.GetLocalPosition());
                meshdata.AddTriangleSimultaneo(upLeft.GetLocalPosition(), bottomRight.GetLocalPosition(), bottomCenter.GetLocalPosition());
                meshdata.AddTriangleSimultaneo(upLeft.GetLocalPosition(), bottomCenter.GetLocalPosition(), leftCenter.GetLocalPosition());
                break;
            case 11://conferido //conferido
                meshdata.AddTriangleSimultaneo(bottomRight.GetLocalPosition(), bottomLeft.GetLocalPosition(), upLeft.GetLocalPosition());
                meshdata.AddTriangleSimultaneo(rightCenter.GetLocalPosition(), bottomRight.GetLocalPosition(), upLeft.GetLocalPosition());
                meshdata.AddTriangleSimultaneo(upCenter.GetLocalPosition(), rightCenter.GetLocalPosition(), upLeft.GetLocalPosition());
                break;
            case 12://conferido //conferido
                meshdata.AddTriangleSimultaneo(rightCenter.GetLocalPosition(), leftCenter.GetLocalPosition(), upLeft.GetLocalPosition());
                meshdata.AddTriangleSimultaneo(upRight.GetLocalPosition(), rightCenter.GetLocalPosition(), upLeft.GetLocalPosition());
                break;
            case 13://conferido //conferido
                meshdata.AddTriangleSimultaneo(upRight.GetLocalPosition(), rightCenter.GetLocalPosition(), upLeft.GetLocalPosition());
                meshdata.AddTriangleSimultaneo(rightCenter.GetLocalPosition(), bottomCenter.GetLocalPosition(), upLeft.GetLocalPosition());
                meshdata.AddTriangleSimultaneo(upLeft.GetLocalPosition(), bottomCenter.GetLocalPosition(), bottomLeft.GetLocalPosition());
                break;
            case 14://conferido //conferido
                meshdata.AddTriangleSimultaneo(upRight.GetLocalPosition(), bottomRight.GetLocalPosition(), upLeft.GetLocalPosition());
                meshdata.AddTriangleSimultaneo(bottomRight.GetLocalPosition(), bottomCenter.GetLocalPosition(), upLeft.GetLocalPosition());
                meshdata.AddTriangleSimultaneo(bottomCenter.GetLocalPosition(), leftCenter.GetLocalPosition(), upLeft.GetLocalPosition());
                break;
            case 15://conferido //conferido
                meshdata.AddTriangleSimultaneo(upRight.GetLocalPosition(), bottomRight.GetLocalPosition(), upLeft.GetLocalPosition());
                meshdata.AddTriangleSimultaneo(bottomRight.GetLocalPosition(), bottomLeft.GetLocalPosition(), upLeft.GetLocalPosition());
                break;

        }
        
    }


}

public class ParticleNode
{
    public bool isDense { get; set; }
    public Vector3 localPosition { get; set; }

    public ParticleNode(bool _isDense)
    {
        isDense = _isDense;
    }
    public ParticleNode(bool _isDense, Vector3 _localPosition)
    {
        isDense = _isDense;
        localPosition = _localPosition;
    }
    public Vector3 GetLocalPosition()
    {
        return localPosition;
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

