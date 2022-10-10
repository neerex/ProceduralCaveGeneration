using UnityEngine;
using System.Collections.Generic;

public class MeshGenerator : MonoBehaviour
{
    private SquareGrid _squareGrid;
    private List<Vector3> _vertices = new();
    private List<int> _triangles = new();

    private MeshFilter _meshFilter;

    private void Start()
    {
        _meshFilter = GetComponent<MeshFilter>();
    }

    public void GenerateMesh(int[,] map, float squareSize)
    {
        _vertices.Clear();
        _triangles.Clear();
        
        _squareGrid = new SquareGrid(map, squareSize);
        
        for (int x = 0; x < _squareGrid.Squares.GetLength(0); x++)
        {
            for (int y = 0; y < _squareGrid.Squares.GetLength(1); y++)
            {
                TriangulateSquare(_squareGrid.Squares[x,y]);
            }
        }

        Mesh mesh = new Mesh();
        _meshFilter.mesh = mesh;
        mesh.vertices = _vertices.ToArray();
        mesh.triangles = _triangles.ToArray();
        mesh.RecalculateNormals();
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
                MeshFromPoints(square.CenterRight, square.BottomRight, square.CenterBottom);
                break;
            
            case 4: //0100
                MeshFromPoints(square.CenterTop, square.TopRight, square.CenterRight);
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
                break;
        }
    }

    private void MeshFromPoints(params Node[] points)
    {
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