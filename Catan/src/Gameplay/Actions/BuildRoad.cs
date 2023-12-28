using Grid.Hexagonal;

namespace Catan;

/// <summary>
/// Build a new road
/// </summary>
/// <remarks>
/// Logic: <see cref="GameState.BuildRoad(int, Edge.Key, bool)"/><br/>
/// Phases: <see cref="PreGameRoad"/>, <see cref="TurnMain"/>
/// </remarks> 
public class BuildRoadAction : IAction
{
    /// <summary>
    /// ID for player building road
    /// </summary>
    public int OwnerID;

    /// <summary>
    /// Position for built road
    /// </summary>
    public Edge.Key Position;

    /// <summary>
    /// Does this action cost resources
    /// </summary>
    /// <remarks>
    /// Used in <see cref="PreGameRoad"/> and <see cref="RoadBuilding"/>.
    /// </remarks>
    public bool Free;

    public BuildRoadAction(int ownerID, Edge.Key position, bool free = false)
    {
        OwnerID = ownerID;
        Position = position;
        Free = free;
    }

    public override string ToString()
    {
        return string.Format("{0} build road", OwnerID);
    }

    /// <summary>
    /// Executes <see cref="GameState.BuildRoad(int, Edge.Key, bool)"/>.
    /// </summary>
    protected override void DoExecute(GameState gameState)
    {
        gameState.BuildRoad(OwnerID, Position, Free);
    }
}