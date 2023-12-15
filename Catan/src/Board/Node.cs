using Grid.Hexagonal;

namespace Catan;

public class Node : Vertex
{
    public int OwnerID = -1;

    public bool City = false;

    public Node()
    {}
}