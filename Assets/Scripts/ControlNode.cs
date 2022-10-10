using UnityEngine;

public class ControlNode : Node
{
    public readonly bool Active; // is wall
    public readonly Node Above;
    public readonly Node Right;

    public ControlNode(Vector3 position, bool active, float squareSize) : base(position)
    {
        Active = active;
        Above = new Node(position + Vector3.forward * squareSize / 2f);
        Right = new Node(position + Vector3.right * squareSize / 2f);
    }
}