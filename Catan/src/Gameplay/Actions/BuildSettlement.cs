using Grid.Hexagonal;

namespace Catan;

/// <summary>
/// Build a new settlement
/// </summary>
public class BuildSettlementAction : IAction
{
    public int OwnerID;
    public Vertex.Key Position;
    public bool Free;

    public BuildSettlementAction(int ownerID, Vertex.Key position, bool free = false)
    {
        OwnerID = ownerID;
        Position = position;
        Free = free;
    }

    protected override void DoExecute(GameState gameState)
    {}
}