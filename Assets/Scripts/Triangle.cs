public readonly struct Triangle
{
    public readonly int VertexIndexA;
    public readonly int VertexIndexB;
    public readonly int VertexIndexC;
    private readonly int[] _vertices;
    
    public int this[int i] => _vertices[i];

    public Triangle(int vertexIndexA, int vertexIndexB, int vertexIndexC)
    {
        VertexIndexA = vertexIndexA;
        VertexIndexB = vertexIndexB;
        VertexIndexC = vertexIndexC;

        _vertices = new[] {VertexIndexA, VertexIndexB, VertexIndexC};
    }

    public bool Contains(int vertexIndex)
    {
        return vertexIndex == VertexIndexA || 
               vertexIndex == VertexIndexB || 
               vertexIndex == VertexIndexC;
    }
}