using Grid.Hexagonal;

namespace Catan;

// TODO: Ports

/// <summary>
/// Singular node on the game board.
/// </summary>
/// <remarks>
/// Can be improved by a player with either a settlement or city.
/// </remarks>
public class Node : Vertex
{
    /// <summary>
    /// Protected <see cref="OwnerID"/> field.
    /// </summary>
    /// <remarks>
    /// Should only be modified via <see cref="OwnerId"/> setter.
    /// </remarks>
    private int m_OwnerID = -1;

    /// <summary>
    /// ID for the current owner,
    /// </summary>
    /// <value> -1 if un-owned, Determines the <see cref="Colour"/></value>
    public int OwnerID {
        get { return m_OwnerID; }
        set { m_OwnerID = value; Colour = Rules.GetPlayerIDColour(value); }
    }

    /// <summary>
    /// If owned (<see cref="OwnerId"/>), indicates if improved with a settlement or city
    /// </summary>
    /// <value> <see cref="DrawFilled"/> = NOT(<see cref="City"/>)</value>
    public bool City {
        get { return !DrawFilled; }
        set { DrawFilled = !value; }
    }

    public Node()
    {}
}