namespace Catan.Action;

public class KnightAction : RobberAction
{
    public override string ToString()
    {
        return $"{OwnerID} plays knight";
    }

    protected override GameState DoExecute(GameState gameState)
    {
        gameState.SetDevCardToPlayed(DevCards.Type.Knight, OwnerID);
        gameState.Players[OwnerID].KnightsPlayed++;
        gameState.UpdateLargestArmy(OwnerID);

        return base.DoExecute(gameState);
    }
};