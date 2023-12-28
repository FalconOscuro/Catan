namespace Catan;

public class EndTurn : IAction
{
    public override string ToString()
    {
        return "Turn end"; // Should display playerID
    }

    protected override void DoExecute(GameState gameState)
    {
        gameState.AdvanceTurn();
    }
}