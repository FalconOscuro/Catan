using System;
using System.Diagnostics.CodeAnalysis;
using Grid.Hexagonal;

namespace Catan.Action;

/// <summary>
/// Build a new settlement
/// </summary>
/// <remarks>
/// Logic: <see cref="GameState.BuildSettlement(int, Vertex.Key, bool)"/><br/>
/// Phases: <see cref="PreGameSettlement"/>, <see cref="TurnMain"/>
/// </remarks>
public class BuildSettlementAction : IAction
{
    /// <summary>
    /// Position for built settlement
    /// </summary>
    public Vertex.Key Position;

    /// <summary>
    /// Does this action cost resources
    /// </summary>
    /// <remarks>
    /// Used by <see cref="PreGameSettlement"/>
    /// </remarks>
    public bool Free;

    public bool DistributeResources;

    public BuildSettlementAction(int ownerID, Vertex.Key position, bool free = false)
    {
        OwnerID = ownerID;
        Position = position;
        Free = free;
    }

    public override string ToString()
    {
        return string.Format("{0} build settlement", OwnerID);
    }

    public override string GetDescription()
    {
        return string.Format(
            "Player: {0}\n" +
            "Position: {1}\n" +
            "Free: {2}",
            OwnerID, Position.ToString(), Free
        );
    }

    public override bool Equals([NotNullWhen(true)] object obj)
    {
        if (obj is not BuildSettlementAction action)
            return false;

        return base.Equals(obj) && action.Position == Position;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    /// <summary>
    /// Executes <see cref="GameState.BuildSettlement(int, Vertex.Key, bool)"/>.
    /// </summary>
    protected override GameState DoExecute(GameState gameState)
    {
        Player player = gameState.Players[OwnerID];
        player.Settlements--;
        player.VictoryPoints++;

        if (!gameState.Board.TryGetVertex(Position, out Node corner))
            throw new Exception();
        
        corner.OwnerID = OwnerID;
        gameState.CheckRoadBreak(Position, OwnerID);

        if (!Free)
        {
            IAction trade = new Trade(){
                OwnerID = OwnerID,
                TargetID = -1,
                Giving = Rules.SETTLEMENT_COST,
                IsHidden = true
            };

            gameState = trade.Execute(gameState);
        }

        if (DistributeResources)
        {
            Axial[] tiles = Position.GetAdjacentHexes();
            Resources.Collection receiving = new();

            foreach (Axial pos in tiles)
                if (gameState.Board.TryGetHex(pos, out Tile tile))
                    receiving[tile.Resource] += 1;
            
            IAction trade = new Trade(){
                OwnerID = OwnerID,
                TargetID = -1,
                Receiving = receiving,
                IsHidden = true
            };

            gameState = trade.Execute(gameState);
        }

        return gameState;
    }
}