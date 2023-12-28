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

    public override string ToString()
    {
        return string.Format("{0} build city", OwnerID);
    }

    protected override void DoExecute(GameState gameState)
    {
        gameState.BuildCity(OwnerID, Position);
    }
}