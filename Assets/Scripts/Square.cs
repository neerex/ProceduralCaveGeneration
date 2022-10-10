public class Square
{
    public readonly ControlNode TopLeft;
    public readonly ControlNode TopRight;
    public readonly ControlNode BottomRight;
    public readonly ControlNode BottomLeft;

    public readonly Node CenterTop;
    public readonly Node CenterRight;
    public readonly Node CenterLeft;
    public readonly Node CenterBottom;

    public readonly int Configuration;

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

        // topLeft 2^3, topRight = 2^2, bottomRight = 2^1, bottomLeft = 2^0, total of 16 configurations
        if(topLeft.Active) Configuration += 8;
        if(topRight.Active) Configuration += 4;
        if(bottomRight.Active) Configuration += 2;
        if(bottomLeft.Active) Configuration += 1;
    }
}