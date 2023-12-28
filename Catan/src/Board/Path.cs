using Grid.Hexagonal;

namespace Catan;

public class Path : Edge
{
    private int m_OwnerID = -1;

    public int OwnerID {
        get { return m_OwnerID; }
        set { m_OwnerID = value; Colour = Rules.GetPlayerIDColour(value); }
    }

    public Path()
    {}
}