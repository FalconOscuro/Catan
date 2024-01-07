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