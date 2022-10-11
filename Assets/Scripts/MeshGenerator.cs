using UnityEngine;
using System.Collections.Generic;

public class MeshGenerator : MonoBehaviour
{
    [SerializeField] private MeshFilter _wallsMeshFilter;
    [SerializeField] private float _wallHeight = 10;
    
    private readonly List<Vector3> _vertices = new();
    private readonly List<int> _triangles = new();

    private readonly Dictionary<int, List<Triangle>> _triangleDict = new(); // int - vertexIndex, list - triangles with this vertex
    private readonly List<List<int>> _outlines = new();
    private readonly HashSet<int> _checkedVertices = new();

    private SquareGrid _squareGrid;
    private MeshFilter _mapMeshFilter;

    private void Awake()
    {
        _mapMeshFilter = GetComponent<MeshFilter>();
    }

    public void GenerateMesh(int[,] map, float squareSize)
    {
        ClearCommonDataHolders();

        _squareGrid = new SquareGrid(map, squareSize);
        
        for (int x = 0; x < _squareGrid.Squares.GetLength(0); x++)
        {
            for (int y = 0; y < _squareGrid.Squares.GetLength(1); y++)
            {
                TriangulateSquare(_squareGrid.Squares[x,y]);
            }
        }

        Mesh mesh = new Mesh();
        _mapMeshFilter.mesh = mesh;
        mesh.vertices = _vertices.ToArray();
        mesh.triangles = _triangles.ToArray();
        mesh.RecalculateNormals();

        CreateWallMesh();
    }

    private void ClearCommonDataHolders()
    {
        _vertices.Clear();
        _triangles.Clear();
        _checkedVertices.Clear();
        _outlines.Clear();
        _triangleDict.Clear();
    }

    private void CreateWallMesh()
    {
        CalculateMeshOutlines();
        List<Vector3> wallVertices = new();
        List<int> wallTriangles = new();

        Mesh wallMesh = new Mesh();

        foreach (List<int> outline in _outlines)
        {
            for (int i = 0; i < outline.Count - 1; i++)
            {
                int startIndex = wallVertices.Count;
                
                wallVertices.Add(_vertices[outline[i]]);     // top left vertex -> 0 
                wallVertices.Add(_vertices[outline[i + 1]]); // top right vertex -> 1
                wallVertices.Add(_vertices[outline[i]] - Vector3.up * _wallHeight);     // bottom left vertex -> 2
                wallVertices.Add(_vertices[outline[i + 1]] - Vector3.up * _wallHeight); // bottom right vertex -> 3
                
                //0 - 2 - 3 triangle (anticlockwise)
                wallTriangles.Add(startIndex + 0);
                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 3);
                
                //3 - 1 - 0 triangle (anticlockwise)
                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 1);
                wallTriangles.Add(startIndex + 0);
            }
        }

        wallMesh.vertices = wallVertices.ToArray();
        wallMesh.triangles = wallTriangles.ToArray();
        _wallsMeshFilter.mesh = wallMesh;
    }

    private void TriangulateSquare(Square square)
    {
        // see scheme.png in Sprites folder
        switch (square.Configuration)
        {
            case 0:
                break;
            
            //1 point active
            case 1: // 0001
                MeshFromPoints(square.CenterLeft, square.CenterBottom, square.BottomLeft);
                break;
            
            case 2: // 0010
                MeshFromPoints(square.BottomRight, square.CenterBottom, square.CenterRight);
                break;
            
            case 4: //0100
                MeshFromPoints(square.TopRight, square.CenterRight, square.CenterTop);
                break;
            
            case 8: //1000
                MeshFromPoints(square.TopLeft, square.CenterTop, square.CenterLeft);
                break;
            
            //2 points active
            case 3: // 0011
                MeshFromPoints(square.CenterRight, square.BottomRight, square.BottomLeft, square.CenterLeft);
                break;
            
            case 5: // 0101
                MeshFromPoints(square.CenterTop, square.TopRight, square.CenterRight, square.CenterBottom, square.BottomLeft, square.CenterLeft);
                break;
            
            case 6: // 0110
                MeshFromPoints(square.CenterTop, square.TopRight, square.BottomRight, square.CenterBottom);
                break;

            case 9: // 1001
                MeshFromPoints(square.TopLeft, square.CenterTop, square.CenterBottom, square.BottomLeft);
                break;

            case 10: // 1010
                MeshFromPoints(square.TopLeft, square.CenterTop, square.CenterRight, square.BottomRight, square.CenterBottom, square.CenterLeft);
                break;
            
            case 12: // 1100
                MeshFromPoints(square.TopLeft, square.TopRight, square.CenterRight, square.CenterLeft);
                break;
            
            //3 points active
            case 7: // 0111
                MeshFromPoints(square.CenterTop, square.TopRight, square.BottomRight, square.BottomLeft, square.CenterLeft);
                break;
            
            case 11: // 1011
                MeshFromPoints(square.TopLeft, square.CenterTop, square.CenterRight, square.BottomRight, square.BottomLeft);
                break;
            
            case 13: // 1101
                MeshFromPoints(square.TopLeft, square.TopRight, square.CenterRight, square.CenterBottom, square.BottomLeft);
                break;
            
            case 14: // 1110
                MeshFromPoints(square.TopLeft, square.TopRight, square.BottomRight, square.CenterBottom, square.CenterLeft);
                break;
            
            //4 point active
            case 15: // 1111
                MeshFromPoints(square.TopLeft, square.TopRight, square.BottomRight, square.BottomLeft);
                //outline calculations for this case, no inside walls out of this case
                _checkedVertices.Add(square.TopLeft.VertexIndex);
                _checkedVertices.Add(square.TopRight.VertexIndex);
                _checkedVertices.Add(square.BottomRight.VertexIndex);
                _checkedVertices.Add(square.BottomLeft.VertexIndex);
                break;
        }
    }

    private void MeshFromPoints(params Node[] points)
    {
        // this method suitable for our case only with maximum of 4 triangles (see schema.png)
        AssignVertices(points);
        if(points.Length >= 3) CreateTriangle(points[0], points[1], points[2]);
        if(points.Length >= 4) CreateTriangle(points[0], points[2], points[3]);
        if(points.Length >= 5) CreateTriangle(points[0], points[3], points[4]);
        if(points.Length >= 6) CreateTriangle(points[0], points[4], points[5]);
    }

    private void AssignVertices(Node[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].VertexIndex == -1)
            {
                points[i].VertexIndex = _vertices.Count;
                _vertices.Add(points[i].Position);
            }
        }
    }

    private void CreateTriangle(Node a, Node b, Node c)
    {
        _triangles.Add(a.VertexIndex);
        _triangles.Add(b.VertexIndex);
        _triangles.Add(c.VertexIndex);

        Triangle triangle = new Triangle(a.VertexIndex, b.VertexIndex, c.VertexIndex);
        AddTriangleToDictionary(triangle.VertexIndexA, triangle);
        AddTriangleToDictionary(triangle.VertexIndexB, triangle);
        AddTriangleToDictionary(triangle.VertexIndexC, triangle);
    }

    private void CalculateMeshOutlines()
    {
        for (int vertexIndex = 0; vertexIndex < _vertices.Count; vertexIndex++)
        {
            if (!_checkedVertices.Contains(vertexIndex))
            {
                int newOutLineVertex = GetConnectedOutLineVertex(vertexIndex);
                if (newOutLineVertex != -1)
                {
                    _checkedVertices.Add(vertexIndex);
                    List<int> newOutline = new();
                    newOutline.Add(vertexIndex);
                    _outlines.Add(newOutline);
                    FollowOutline(newOutLineVertex, _outlines.Count - 1);
                    _outlines[^1].Add(vertexIndex);
                }
            }
        }
    }

    private void FollowOutline(int vertexIndex, int outlineIndex)
    {
        _outlines[outlineIndex].Add(vertexIndex);
        _checkedVertices.Add(vertexIndex);
        int nextVertexIndex = GetConnectedOutLineVertex(vertexIndex);
        if (nextVertexIndex != -1)
        {
            FollowOutline(nextVertexIndex, outlineIndex);
        }
    }

    private int GetConnectedOutLineVertex(int vertexIndex)
    {
        List<Triangle> trianglesContainingVertex = _triangleDict[vertexIndex];

        for (int i = 0; i < trianglesContainingVertex.Count; i++)
        {
            Triangle triangle = trianglesContainingVertex[i];

            for (int j = 0; j < 3; j++)
            {
                int vertexB = triangle[j];
                if (vertexB != vertexIndex && !_checkedVertices.Contains(vertexB))
                {
                    if (IsOutlineEdge(vertexIndex, vertexB))
                        return vertexB;
                }
            }
        }

        return -1;
    }
    
    private bool IsOutlineEdge(int vertexA, int vertexB)
    {
        // if 2 vertices share only 1 triangle => outline edge

        List<Triangle> triangleContainingVertexA = _triangleDict[vertexA];
        int sharedTriangleCount = 0;
        for (int i = 0; i < triangleContainingVertexA.Count; i++)
        {
            if (triangleContainingVertexA[i].Contains(vertexB))
            {
                sharedTriangleCount++;
                if (sharedTriangleCount > 1) break;
            }
        }

        return sharedTriangleCount == 1;
    }

    private void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle)
    {
        if (_triangleDict.ContainsKey(vertexIndexKey))
        {
            _triangleDict[vertexIndexKey].Add(triangle);
        }
        else
        {
            List<Triangle> triangleList = new();
            triangleList.Add(triangle);
            _triangleDict.Add(vertexIndexKey, triangleList);
        }
    }

    // private void OnDrawGizmos()
    // {
    //     if (_squareGrid == null) return;
    //
    //     for (int x = 0; x < _squareGrid.Squares.GetLength(0); x++)
    //     {
    //         for (int y = 0; y < _squareGrid.Squares.GetLength(1); y++)
    //         {
    //             //Draw control nodes
    //             Gizmos.color = _squareGrid.Squares[x, y].TopLeft.Active ? Color.black : Color.white;
    //             Gizmos.DrawCube(_squareGrid.Squares[x,y].TopLeft.Position, Vector3.one * 0.4f);
    //             
    //             Gizmos.color = _squareGrid.Squares[x, y].TopRight.Active ? Color.black : Color.white;
    //             Gizmos.DrawCube(_squareGrid.Squares[x,y].TopRight.Position, Vector3.one * 0.4f);
    //             
    //             Gizmos.color = _squareGrid.Squares[x, y].BottomLeft.Active ? Color.black : Color.white;
    //             Gizmos.DrawCube(_squareGrid.Squares[x,y].BottomLeft.Position, Vector3.one * 0.4f);
    //             
    //             Gizmos.color = _squareGrid.Squares[x, y].BottomRight.Active ? Color.black : Color.white;
    //             Gizmos.DrawCube(_squareGrid.Squares[x,y].BottomRight.Position, Vector3.one * 0.4f);
    //
    //             //draw (middle) nodes
    //             Gizmos.color = Color.grey;
    //             Gizmos.DrawCube(_squareGrid.Squares[x,y].CenterLeft.Position, Vector3.one * 0.2f);
    //             Gizmos.DrawCube(_squareGrid.Squares[x,y].CenterTop.Position, Vector3.one * 0.2f);
    //             Gizmos.DrawCube(_squareGrid.Squares[x,y].CenterRight.Position, Vector3.one * 0.2f);
    //             Gizmos.DrawCube(_squareGrid.Squares[x,y].CenterBottom.Position, Vector3.one * 0.2f);
    //         }
    //     }
    // }
}