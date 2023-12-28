namespace Catan;

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

    /// <summary>
    /// Executes <see cref="GameState.AdvanceTurn"/>.
    /// </summary>
    protected override void DoExecute(GameState gameState)
    {
        gameState.AdvanceTurn();
    }
}