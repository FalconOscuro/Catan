using System.Diagnostics.CodeAnalysis;

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

    public override IAction Clone()
    {
        Trade clone = new(){
            OwnerID = OwnerID,
            TargetID = TargetID,
            Giving = Giving.Clone(),
            Receiving = Receiving.Clone()
        };

        return clone;
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

    public override bool Equals([NotNullWhen(true)] object obj)
    {
        if (obj is not Trade action)
            return false;

        return base.Equals(obj) && action.TargetID == TargetID && action.Giving.Equals(Giving) && action.Receiving.Equals(Receiving);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
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