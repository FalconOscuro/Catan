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
    public override string ToString()
    {
        return "Turn end"; // Should display playerID
    }

    public override string GetDescription()
    {
        return ToString();
    }

    /// <summary>
    /// Executes <see cref="GameState.AdvanceTurn"/>.
    /// </summary>
    protected override void DoExecute(GameState gameState)
    {
        gameState.AdvanceTurn();
    }
}