using Grid.Hexagonal;

namespace Catan;

/// <summary>
/// Build a new road
/// </summary>
public class BuildRoadAction : IAction
{
    public int OwnerID;
    public Edge.Key Position;

    public BuildRoadAction(int ownerID, Edge.Key position)
    {
        OwnerID = ownerID;
        Position = position;
    }

    public void Execute(ref GameState gameState)
    {}
}