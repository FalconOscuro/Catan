using Grid.Hexagonal;

namespace Catan;

/// <summary>
/// Build a new settlement
/// </summary>
public class BuildSettlementAction : IAction
{
    public int OwnerID;
    public Vertex.Key Position;

    public BuildSettlementAction(int ownerID, Vertex.Key position)
    {
        OwnerID = ownerID;
        Position = position;
    }

    public void Execute(ref GameState gameState)
    {}
}