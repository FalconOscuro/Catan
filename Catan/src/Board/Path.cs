using Grid.Hexagonal;

namespace Catan;

/// <summary>
/// Singular edge on the game board,
/// </summar>
/// <remarks>
/// can be improved by a player with a road
/// </remarks>
public class Path : Edge
{
    /// <summary>
    /// Protected <see cref="OwnerID"/> field.
    /// </summary>
    /// <remarks>
    /// should only be modified via <see cref="OwnerID"/> setter.
    /// </remarks>
    private int m_OwnerID = -1;

    /// <summary>
    /// ID for the current owner.
    /// </summary>
    /// <value> -1 if un-owned </value>
    public int OwnerID {
        get { return m_OwnerID; }
        set { m_OwnerID = value; Colour = Rules.GetPlayerIDColour(value); }
    }

    public Path()
    {}
}