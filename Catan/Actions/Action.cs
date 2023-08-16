namespace Catan;

// Grouped actions?
abstract class Action
{
    protected Action(int playerID)
    {
        PlayerID = playerID;
    }

    public abstract bool TryCheckPreConditions(Catan.GameState gameState);

    public abstract bool CheckPreConditions(Catan.GameState gameState);

    public virtual Catan.GameState TryCheckResult(Catan.GameState gameState)
    {
        if (gameState.PlayerStates[PlayerID].VictoryPoints >= 10)
            gameState.Phase = Catan.State.End;
        
        return gameState;
    }

    public abstract Catan.GameState CheckResult(Catan.GameState gameState);

    public int PlayerID { get; private set; }
}