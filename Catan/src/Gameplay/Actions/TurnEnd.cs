namespace Catan.Action;

/// <summary>
/// Simple turn ending action
/// </summary>
/// <remarks>
/// Logic: <see cref="GameState.AdvanceTurn"/><br/>
/// Phases: <see cref="TurnMain"/>
/// </remarks>
public class EndTurn : IAction
{
    public EndTurn()
    {
        TriggerStateChange = true;
    }

    public override string ToString()
    {
        return $"{OwnerID} Turn End"; // Should display playerID
    }

    public override string GetDescription()
    {
        return ToString();
    }

    /// <summary>
    /// Executes <see cref="GameState.AdvanceTurn"/>.
    /// </summary>
    protected override GameState DoExecute(GameState gameState)
    {
        gameState.AdvanceTurn();
        return gameState;
    }
}