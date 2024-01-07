namespace Catan.Action;

public class YearOfPlenty : Trade
{
    public YearOfPlenty()
    {
        TargetID = -1;
    }

    public override string ToString()
    {
        return $"{OwnerID} play YoP";
    }

    public override string GetDescription()
    {
        return string.Format(
            "Player: {0}\n" +
            "Recieved:\n{1}",
            OwnerID, Receiving.ToString()
        );
    }

    protected override GameState DoExecute(GameState gameState)
    {
        gameState.SetDevCardToPlayed(DevCards.Type.YearOfPlenty, OwnerID);

        return base.DoExecute(gameState);
    }
}