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

    public override string ToString()
    {
        return string.Format("{0} build settlement", OwnerID);
    }

    protected override void DoExecute(GameState gameState)
    {
        gameState.BuildSettlement(OwnerID, Position, Free);
    }
}