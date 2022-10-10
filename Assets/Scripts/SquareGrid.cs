using UnityEngine;

public class SquareGrid
{
    public readonly Square[,] Squares;

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