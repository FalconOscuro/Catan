namespace Catan;

public class EndTurn : IAction
{
    protected override void DoExecute(GameState gameState)
    {
        gameState.AdvanceTurn();
    }
}