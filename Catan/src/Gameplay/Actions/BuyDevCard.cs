using System.Diagnostics.CodeAnalysis;

namespace Catan.Action;

public class BuyDevCardAction : IAction
{
    public override string ToString()
    {
        return $"{OwnerID} buys DevCard";
    }

    public override string GetDescription()
    {
        return ToString();
    }

    public override bool Equals([NotNullWhen(true)] object obj)
    {
        return base.Equals(obj) && obj is BuyDevCardAction;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    protected override GameState DoExecute(GameState gameState)
    {
        Player player = gameState.Players[OwnerID];

        player.HeldDevCards.Add(gameState.GetNextDevCard());
        
        IAction trade = new Trade(){
            OwnerID = OwnerID,
            TargetID = -1,
            Giving = Rules.DEVELOPMENT_CARD_COST
        };

        return trade.Execute(gameState);
    }
}