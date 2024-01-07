namespace Catan.Action;

/// <summary>
/// Trade between 2 players, or a player and the bank
/// </summary>
/// <remarks>
/// Logic: <see cref="GameState.DoTrade(int, int, Catan.Resources.Collection, Catan.Resources.Collection)"/><br/>
/// Phases: 
/// </remarks>
public class Trade : IAction
{
    /// <summary>
    /// Target to be traded with,
    /// if set to -1, trade will be executed with the bank
    /// </summary>
    public int TargetID;

    /// <summary>
    /// Resource going from owner to target
    /// </summary>
    /// <returns></returns>
    public Resources.Collection Giving = new();

    /// <summary>
    /// Resources going from target to owner
    /// </summary>
    /// <returns></returns>
    public Resources.Collection Receiving = new();

    public Trade()
    {}

    /// <summary>
    /// Is this trade possible
    /// </summary>
    /// <remarks>
    /// Deprecated?
    /// </remarks>
    public bool CanExecute(GameState gameState)
    {
        bool canOwnerTrade = gameState.Players[OwnerID].Hand >= Giving;
        bool canTargetTrade = (TargetID == -1 ? gameState.Bank : gameState.Players[TargetID].Hand) >= Receiving;

        return canOwnerTrade && canTargetTrade;
    }

    public override string ToString()
    {
        return string.Format("{0} trade {1}", OwnerID, TargetID);// Need long version
    }

    public override string GetDescription()
    {
        return string.Format(
            "Owner: {0}\n" +
            "Trading:\n{1}\n\n" +

            "Target: {2}\n" +
            "Trading:\n{3}",
            OwnerID, Giving.ToString(),
            TargetID, Receiving.ToString()
        );
    }

    /// <summary>
    /// Executes <see cref="GameState.DoTrade(int, int, Resources.Collection, Resources.Collection)"/>.
    /// </summary>
    protected override GameState DoExecute(GameState gameState)
    {
        gameState.Players[OwnerID].Hand += Receiving - Giving;

        if (TargetID == -1)
            gameState.Bank += Giving - Receiving;
        
        else
            gameState.Players[TargetID].Hand += Giving - Receiving;
        
        return gameState;
    }
}