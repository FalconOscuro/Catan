using Grid.Hexagonal;

namespace Catan;

/// <summary>
/// Build a new city
/// </summary>
/// <remarks>
/// Logic: <see cref="GameState.BuildCity(int, Vertex.Key)"/><br/>
/// Phases: <see cref="TurnMain"/>
/// </remarks>
public class BuildCityAction : IAction
{
    /// <summary>
    /// ID for player building city
    /// </summary>
    public int OwnerID;

    /// <summary>
    /// Position for city
    /// </summary>
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

    public override string GetDescription()
    {
        return string.Format(
            "Player: {0}\n" +
            "Position: {1}",
            OwnerID, Position.ToString()
        );
    }

    /// <summary>
    /// Executes <see cref="GameState.BuildCity(int, Vertex.Key)"/>.
    /// </summary>
    protected override void DoExecute(GameState gameState)
    {
        gameState.BuildCity(OwnerID, Position);
    }
}