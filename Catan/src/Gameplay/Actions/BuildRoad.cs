using System;
using Grid.Hexagonal;

namespace Catan.Action;

/// <summary>
/// Build a new road
/// </summary>
/// <remarks>
/// Logic: <see cref="GameState.BuildRoad(int, Edge.Key, bool)"/><br/>
/// Phases: <see cref="PreGameRoad"/>, <see cref="TurnMain"/>
/// </remarks> 
public class BuildRoadAction : IAction
{
    /// <summary>
    /// Position for built road
    /// </summary>
    public Edge.Key Position;

    /// <summary>
    /// Does this action cost resources
    /// </summary>
    /// <remarks>
    /// Used in <see cref="PreGameRoad"/> and <see cref="RoadBuilding"/>.
    /// </remarks>
    public bool Free;

    public BuildRoadAction(int ownerID, Edge.Key position, bool free = false)
    {
        OwnerID = ownerID;
        Position = position;
        Free = free;
    }

    public override string ToString()
    {
        return string.Format("{0} build road", OwnerID);
    }

    public override string GetDescription()
    {
        return string.Format(
            "Player: {0}\n" +
            "Postion: {1}\n" +
            "Free: {2}",
            OwnerID, Position.ToString(), Free
        );
    }

    /// <summary>
    /// Executes <see cref="GameState.BuildRoad(int, Edge.Key, bool)"/>.
    /// </summary>
    protected override GameState DoExecute(GameState gameState)
    {
        Player player = gameState.Players[OwnerID];
        player.Roads--;
        
        if (!gameState.Board.TryGetEdge(Position, out Path path))
            throw new Exception();
        
        path.OwnerID = OwnerID;
        gameState.UpdateLongestRoad(OwnerID);

        if (Free)
            return gameState;
        
        IAction trade = new Trade(){
            OwnerID = OwnerID,
            TargetID = -1,
            Giving = Rules.ROAD_COST
        };

        return trade.Execute(gameState);
    }
}