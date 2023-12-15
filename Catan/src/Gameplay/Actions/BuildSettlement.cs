using Grid.Hexagonal;

namespace Catan;

/// <summary>
/// Build a new settlement
/// </summary>
public class BuildSettlementAction : IAction
{
    public Vertex.Key Position;
    public int OwnerID;

    public void Execute(ref GameState gameState)
    {}
}