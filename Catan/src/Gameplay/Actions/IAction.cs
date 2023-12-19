namespace Catan;

/// <summary>
/// Structure for Command patter of actions executable by a player
/// </summary>
public abstract class IAction
{
    public void Execute(GameState gameState)
    {
        DoExecute(gameState);

        gameState.PlayedActions.Add(this);
        gameState.UpdatePhase(this);
    }

    protected abstract void DoExecute(GameState gameState);
}