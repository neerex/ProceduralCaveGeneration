using System;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public SquareGrid SquareGrid;

    public void GenerateMesh(int[,] map, float squareSize)
    {
        SquareGrid = new SquareGrid(map, squareSize);
    }

    private void OnDrawGizmos()
    {
        if (SquareGrid == null) return;

        for (int x = 0; x < SquareGrid.Squares.GetLength(0); x++)
        {
            for (int y = 0; y < SquareGrid.Squares.GetLength(1); y++)
            {
                //Draw control nodes
                Gizmos.color = SquareGrid.Squares[x, y].TopLeft.Active ? Color.black : Color.white;
                Gizmos.DrawCube(SquareGrid.Squares[x,y].TopLeft.Position, Vector3.one * 0.4f);
                
                Gizmos.color = SquareGrid.Squares[x, y].TopRight.Active ? Color.black : Color.white;
                Gizmos.DrawCube(SquareGrid.Squares[x,y].TopRight.Position, Vector3.one * 0.4f);
                
                Gizmos.color = SquareGrid.Squares[x, y].BottomLeft.Active ? Color.black : Color.white;
                Gizmos.DrawCube(SquareGrid.Squares[x,y].BottomLeft.Position, Vector3.one * 0.4f);
                
                Gizmos.color = SquareGrid.Squares[x, y].BottomRight.Active ? Color.black : Color.white;
                Gizmos.DrawCube(SquareGrid.Squares[x,y].BottomRight.Position, Vector3.one * 0.4f);

                //draw (middle) nodes
                Gizmos.color = Color.grey;
                Gizmos.DrawCube(SquareGrid.Squares[x,y].CenterLeft.Position, Vector3.one * 0.2f);
                Gizmos.DrawCube(SquareGrid.Squares[x,y].CenterTop.Position, Vector3.one * 0.2f);
                Gizmos.DrawCube(SquareGrid.Squares[x,y].CenterRight.Position, Vector3.one * 0.2f);
                Gizmos.DrawCube(SquareGrid.Squares[x,y].CenterBottom.Position, Vector3.one * 0.2f);
            }
        }
    }
}


public class Node
{
    public Vector3 Position;
    public int VertexIndex = -1;

    public Node(Vector3 position)
    {
        Position = position;
    }
}

public class ControlNode : Node
{
    public bool Active; // is wall
    public Node Above;
    public Node Right;

    public ControlNode(Vector3 position, bool active, float squareSize) : base(position)
    {
        Active = active;
        Above = new Node(position + Vector3.forward * squareSize / 2f);
        Right = new Node(position + Vector3.right * squareSize / 2f);
    }
}

public class Square
{
    public ControlNode TopLeft;
    public ControlNode TopRight;
    public ControlNode BottomRight;
    public ControlNode BottomLeft;

    public Node CenterTop;
    public Node CenterRight;
    public Node CenterLeft;
    public Node CenterBottom;

    public Square(ControlNode topLeft, ControlNode topRight, ControlNode bottomRight, ControlNode bottomLeft)
    {
        TopLeft = topLeft;
        TopRight = topRight;
        BottomRight = bottomRight;
        BottomLeft = bottomLeft;

        CenterTop = topLeft.Right;
        CenterRight = bottomRight.Above;
        CenterLeft = bottomLeft.Above;
        CenterBottom = bottomLeft.Right;
    }
}
        
public class SquareGrid
{
    public Square[,] Squares;

    public SquareGrid(int[,] map, float squareSize)
    {
        int nodeCountX = map.GetLength(0);
        int nodeCountY = map.GetLength(1);
        
        float mapWidth = nodeCountX * squareSize;
        float mapHeight = nodeCountY * squareSize;

        ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

        for (int x = 0; x < nodeCountX; x++)
        {
            for (int y = 0; y < nodeCountY; y++)
            {
                float xPos = -mapWidth / 2 + x * squareSize + squareSize / 2;
                float zPos = -mapHeight / 2 + y * squareSize + squareSize / 2;
                
                Vector3 pos = new Vector3(xPos, 0, zPos);
                bool isWall = map[x, y] == 1;
                
                controlNodes[x, y] = new ControlNode(pos, isWall, squareSize);
            }
        }

        Squares = new Square[nodeCountX - 1, nodeCountY - 1];
        for (int x = 0; x < nodeCountX - 1; x++)
        {
            for (int y = 0; y < nodeCountY - 1; y++)
            {
                Squares[x, y] = new Square(
                    controlNodes[x, y + 1],
                    controlNodes[x + 1, y + 1],
                    controlNodes[x + 1, y],
                    controlNodes[x, y]
                );
            }
        }
    }
}
