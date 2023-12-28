using Grid.Hexagonal;

namespace Catan;

public class Node : Vertex
{
    private int m_OwnerID = -1;

    public int OwnerID {
        get { return m_OwnerID; }
        set { m_OwnerID = value; Colour = Rules.GetPlayerIDColour(value); }
    }

    public bool City = false;

    public Node()
    {}
}