using Grid.Hexagonal;

namespace Catan;

/// <summary>
/// Build a new road
/// </summary>
public class BuildRoadAction : IAction
{
    public int OwnerID;
    public Edge.Key Position;
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

    protected override void DoExecute(GameState gameState)
    {
        gameState.BuildRoad(OwnerID, Position, Free);
    }
}