namespace Catan;

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

    /// <summary>
    /// Is this trade possible
    /// </summary>
    /// <remarks>
    /// Deprecated?
    /// </remarks>
    public bool CanExecute(GameState gameState)
    {
        bool canOwnerTrade = gameState.Players[OwnerID].Hand >= Giving;
        bool canTargetTrade = (TargetID == -1 ? gameState.Bank : gameState.Players[TargetID].Hand) >= Recieving;

        return canOwnerTrade && canTargetTrade;
    }

    public override string ToString()
    {
        return string.Format("{0} trade {1}", OwnerID, TargetID);// Need long version
    }

    /// <summary>
    /// Executes <see cref="GameState.DoTrade(int, int, Resources.Collection, Resources.Collection)"/>.
    /// </summary>
    protected override void DoExecute(GameState gameState)
    {
        gameState.DoTrade(OwnerID, TargetID, Giving, Recieving);
    }
}