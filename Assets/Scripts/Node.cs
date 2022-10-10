using UnityEngine;

public class Node
{
    public Vector3 Position;
    public int VertexIndex = -1; // -1 means not assigned by default

    public Node(Vector3 position)
    {
        Position = position;
    }
}