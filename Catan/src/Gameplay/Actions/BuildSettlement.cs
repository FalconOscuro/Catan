using Grid.Hexagonal;

namespace Catan.Action;

/// <summary>
/// Build a new settlement
/// </summary>
/// <remarks>
/// Logic: <see cref="GameState.BuildSettlement(int, Vertex.Key, bool)"/><br/>
/// Phases: <see cref="PreGameSettlement"/>, <see cref="TurnMain"/>
/// </remarks>
public class BuildSettlementAction : IAction
{
    /// <summary>
    /// ID for player building settlement
    /// </summary>
    public int OwnerID;

    /// <summary>
    /// Position for built settlement
    /// </summary>
    public Vertex.Key Position;

    /// <summary>
    /// Does this action cost resources
    /// </summary>
    /// <remarks>
    /// Used by <see cref="PreGameSettlement"/>
    /// </remarks>
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

    public override string GetDescription()
    {
        return string.Format(
            "Player: {0}\n" +
            "Position: {1}\n" +
            "Free: {2}",
            OwnerID, Position.ToString(), Free
        );
    }

    /// <summary>
    /// Executes <see cref="GameState.BuildSettlement(int, Vertex.Key, bool)"/>.
    /// </summary>
    protected override void DoExecute(GameState gameState)
    {
        gameState.BuildSettlement(OwnerID, Position, Free);
    }
}