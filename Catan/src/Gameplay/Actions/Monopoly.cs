namespace Catan.Action;

public class Monopoly : IAction
{
    public Resources.Type TargetResource;

    public override string ToString()
    {
        return $"{OwnerID} play Monopoly";
    }

    public override string GetDescription()
    {
        return string.Format(
            "Player: {0}\n" +
            "Resource: {1}",
            OwnerID, TargetResource
        );
    }

    protected override GameState DoExecute(GameState gameState)
    {
        gameState.SetDevCardToPlayed(DevCards.Type.Monopoly, OwnerID);

        for (int i = 0; i < Rules.NUM_PLAYERS; i++)
        {
            if (i == OwnerID)
                continue;
            
            Player player = gameState.Players[i];
            if (player.Hand[TargetResource] < 1)
                continue;
            
            Resources.Collection receiving = new();
            receiving[TargetResource] = player.Hand[TargetResource];

            IAction trade = new Trade(){
                OwnerID = OwnerID,
                TargetID = i,
                Receiving = receiving
            };

            gameState = trade.Execute(gameState);
        }

        return gameState;
    }
}