namespace Catan;

/// <summary>
/// Trade between 2 players
/// or a player and the bank
/// </summary>
public class Trade : IAction
{
    /// <summary>
    /// Owner / Initiator of the trade
    /// </summary>
    public int OwnerID;

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
    public Resources.Collection Recieving = new();

    public Trade()
    {}

    public bool CanExecute(GameState gameState)
    {
        bool canOwnerTrade = gameState.Players[OwnerID].Hand >= Giving;
        bool canTargetTrade = (TargetID == -1 ? gameState.Bank : gameState.Players[TargetID].Hand) >= Recieving;

        return canOwnerTrade && canTargetTrade;
    }

    protected override void DoExecute(GameState gameState)
    {
        gameState.DoTrade(OwnerID, TargetID, Giving, Recieving);
    }
}