using Grid.Hexagonal;

namespace Catan.Action;

public class RoadBuilding : IAction
{
    public Edge.Key Road1Pos;
    public Edge.Key Road2Pos;

    public override string ToString()
    {
        return $"{OwnerID} play Road Building";
    }

    public override string GetDescription()
    {
        return string.Format(
            "Player: {0}\n" +
            "Road 1 Position: {1}\n" +
            "Road 2 Position: {2}",
            OwnerID, Road1Pos, Road2Pos
        );
    }

    protected override GameState DoExecute(GameState gameState)
    {
        BuildRoadAction road1 = new(OwnerID, Road1Pos, true){
            IsHidden = true
        };

        road1.Execute(gameState);

        BuildRoadAction road2 = new(OwnerID, Road2Pos, true){
            IsHidden = true
        };

        road2.Execute(gameState);

        gameState.SetDevCardToPlayed(DevCards.Type.RoadBuilding, OwnerID);
        return gameState;
    }
}