using System;
using System.Collections.Generic;
using Catan.Action;
using Grid.Hexagonal;

namespace Catan.State;
using Type = Resources.Type;

public class Robber : IGamePhase
{
    public void OnEnter(GameState gameState, params object[] argn)
    {}

    public void OnExit()
    {}

    public List<IAction> GetValidActions(GameState gameState)
    {
        List<IAction> actions = new();

        int playerID = gameState.GetCurrentPlayerID();
        foreach ((Axial targetPos, int targetID) in GetAllRobberMoves(gameState, playerID))
            actions.Add(new RobberAction(){
                OwnerID = playerID,
                TargetID = targetID,
                TargetPos = targetPos,
                TriggerStateChange = true
            });

        return actions;
    }

    public void Update(GameState gameState, IAction lastAction)
    {
        gameState.PhaseManager.ChangePhase(TurnMain.NAME, gameState);
    }

    public static IEnumerable<(Axial, int)> GetAllRobberMoves(GameState gameState, int playerID)
    {
        List<Axial> tiles = gameState.Board.GetAllHexes();

        foreach (Axial tilePos in tiles)
        {
            // Tile already has robber
            if (tilePos == gameState.RobberPos)
                continue;
            
            // Check adj nodes for targetable players
            int adjPlayerCount = 0;
            for (Vertex.Key nodePos = new(){Position = tilePos, Side = Vertex.Side.W}; nodePos.Side < Vertex.Side.SW + 1; nodePos.Side++)
            {
                gameState.Board.TryGetVertex(nodePos, out Node node);

                if (node.OwnerID != playerID && node.OwnerID != -1)
                {
                    yield return (tilePos, node.OwnerID);

                    adjPlayerCount++;
                }
            }

            // No targetable players found
            if (adjPlayerCount == 0)
            {
                IAction action = new Action.RobberAction(){
                    OwnerID = playerID,
                    TargetID = -1,
                    TargetPos = tilePos
                };

                yield return (tilePos, -1);
            }
        }
    }

    public const string NAME = "Robber";
}

public class Discard : IGamePhase
{
    public void OnEnter(GameState gameState, params object[] argn)
    {
        FindTarget(gameState);
    }

    public void OnExit()
    {}

    public List<IAction> GetValidActions(GameState gameState)
    {
        List<IAction> actions = new();
        int playerID = gameState.GetCurrentPlayerID();

        Resources.Collection hand = gameState.Players[playerID].Hand;

        // Half hand size rounded down
        int discardNum = Math.DivRem(hand.Count(), 2).Quotient;
        
        foreach (var giving in Resources.RecurseOptions(hand, new Resources.Collection(), discardNum))
        {
            Trade trade = new(){
                OwnerID = playerID,
                TargetID = -1,
                Giving = giving,
                TriggerStateChange = true
            };

            actions.Add(trade);
        }

        return actions;
    }

    public void Update(GameState gameState, IAction lastAction)
    {
        gameState.CurrentPlayerOffset++;
        FindTarget(gameState);
    }

    private static void FindTarget(GameState gameState)
    {
        for (; gameState.CurrentPlayerOffset < Rules.NUM_PLAYERS; gameState.CurrentPlayerOffset++)
            if (gameState.Players[gameState.GetCurrentPlayerID()].Hand.Count() > Rules.MAX_HAND_SIZE)
                return;
        
        gameState.CurrentPlayerOffset = 0;
        gameState.PhaseManager.ChangePhase(Robber.NAME, gameState);
    }

    public const string NAME = "Discard";
}