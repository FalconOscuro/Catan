using Grid.Hexagonal;

namespace Catan;

/// <summary>
/// Build a new city
/// </summary>
public class BuildCityAction : IAction
{
    public int OwnerID;
    public Vertex.Key Position;

    public BuildCityAction(int ownerID, Vertex.Key position)
    {
        OwnerID = ownerID;
        Position = position;
    }

    public void Execute(ref GameState gameState)
    {}
}