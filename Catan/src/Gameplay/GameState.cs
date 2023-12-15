using System;
using System.Collections.Generic;
using Grid.Hexagonal;

namespace Catan;

/// <summary>
/// A model describing the current game state,
/// alongside basic action functions
/// </summary>
public struct GameState
{
    // Dev Card Deck
    // Progress Cards
    // Setup method

    /// <summary>
    /// Resources available for distribution
    /// </summary>
    public Resources.Collection Bank = new();

    /// <summary>
    /// Current board layout
    /// </summary>
    public HexGrid Board = new();

    public Dictionary<int, List<Axial>> TileValueMap = new();

    public Axial RobberPos = new();
    
    /// <summary>
    /// Players
    /// </summary>
    public readonly Player[] Players = new Player[Rules.NUM_PLAYERS];

    public int CurrentPlayerOffset = 0;
    public int CurrentTurnIndex = 0;

    /// <summary>
    /// Sum result of the last dice roll
    /// </summary>
    public int LastRoll = 0;

    public Phase CurrentPhase { get; private set; }

    // TODO: Init with seed
    public Random Random = new();

    public List<IAction> ValidActions = new();
    
    public GameState()
    {
        CurrentPhase = 0;
    }

    public GameState Clone()
    {
        GameState clone = new(){
            LastRoll = LastRoll,
            Board = Board.Clone(),
            Bank = Bank.Clone(),
            CurrentTurnIndex = CurrentTurnIndex,
            RobberPos = RobberPos,
            TileValueMap = TileValueMap
        };
        Players.CopyTo(clone.Players, 0);

        return clone;
    }

    public void UpdatePhase(Phase phase)
    {
        if (phase == Phase.DISCARD)
        {
            while (Players[GetCurrentPlayer()].Hand.Count() <= Rules.MAX_HAND_SIZE && 
                    CurrentPlayerOffset != Rules.NUM_PLAYERS)
                CurrentPlayerOffset++;
            
            if (CurrentPlayerOffset == Rules.NUM_PLAYERS)
            {
                CurrentPlayerOffset = 0;
                phase = Phase.ROBBER;
            }
        }

        CurrentPhase = phase;

        // Check victory first
        // Recompute possible actions
    }

    // Should be able to specify dice roll for simulation
    public (int, int) RollDice()
    {
        int d1 = Random.Next(1, 7);
        int d2 = Random.Next(1, 7);

        return (d1, d2);
    }

    public void DoTrade(int ownerID, int targetID, Resources.Collection giving, Resources.Collection recieving)
    {
        Players[ownerID].Hand += recieving - giving;

        if (targetID == -1)
            Bank += giving - recieving;
        
        else
            Players[targetID].Hand += giving - recieving;
    }

    public void BuildSettlement(int ownerID, Vertex.Key position, bool free = false)
    {
        Players[ownerID].Settlements--;
        Players[ownerID].VictoryPoints++;

        if (!Board.TryGetVertex(position, out Node corner))
            throw new Exception();
        
        corner.OwnerID = ownerID;

        if (!free)
            DoTrade(ownerID, -1, Rules.SETTLEMENT_COST, new());
    }

    public void BuildRoad(int ownerID, Edge.Key position, bool free)
    {
        Players[ownerID].Roads--;
        
        if (!Board.TryGetEdge(position, out Path path))
            throw new Exception();
        
        path.OwnerID = ownerID;

        if (!free)
            DoTrade(ownerID, -1, Rules.ROAD_COST, new());
    }

    public void BuildCity(int ownerID, Vertex.Key position)
    {
        Players[ownerID].Settlements++;
        Players[ownerID].Cities--;
        Players[ownerID].VictoryPoints++;

        if (!Board.TryGetVertex(position, out Node corner))
            throw new Exception();
        
        corner.City = true;

        DoTrade(ownerID, -1, Rules.CITY_COST, new());
    }

    public void MoveRobber(Axial position)
    {
        // Remove from old tile
        Board.TryGetHex(RobberPos, out Tile tile);
        tile.Robber = false;

        // Add to new tile
        RobberPos = position;
        Board.TryGetHex(RobberPos, out tile);
        tile.Robber = true;
    }

    // buy dev card
    // play dev card
    // Steal

    public void DistributeResources()
    {
        Resources.Collection[] playerTrades = new Resources.Collection[Rules.NUM_PLAYERS];

        // Get all distribution trades for role
        foreach (var pos in TileValueMap[LastRoll])
        {
            // Skip robber tile
            if (pos == RobberPos)
                continue;

            if (!Board.TryGetHex(pos, out Tile tile))
                throw new Exception(string.Format("Expected tile at (q={0}, r={1}), but found none!", pos.Q, pos.R));

            Resources.Type type = tile.Resource;

            // Skip if none of resource type available
            if (Bank[type] == 0)
                continue;

            for (Vertex.Key key = new(){Position = pos}; key.Side < Vertex.Side.SW + 1; key.Side++)
            {
                if (!Board.TryGetVertex(key, out Node corner))
                    throw new Exception(string.Format("Expected intersection at (q={0}, r{1}, side={2}), but found none!", pos.Q, pos.R, key.Side.ToString()));
                
                if (corner.OwnerID == -1)
                    continue;

                playerTrades[corner.OwnerID][type] += corner.City ? 2 : 1;
            }
        }

        // Get sum total
        Resources.Collection total = new();
        for (int i = 0; i < Rules.NUM_PLAYERS; i++)
            total += playerTrades[i];
        
        // There are no trades, exit function
        if (total.Count() == 0)
            return;

        // Ensure trades can be executed
        for (Resources.Type type = Resources.Type.Brick; type < Resources.Type.Wool + 1; type++)
        {
            if (Bank[type] >= total[type])
                continue;
            
            // Check number of requesting players
            int firstPlayerIndex = -1;
            bool cannotSupply = false;
            for (int i = 0; i < Rules.NUM_PLAYERS; i++)
            {
                if(playerTrades[i][type] == 0)
                    continue;
                
                else if (firstPlayerIndex == -1)
                    firstPlayerIndex = i;
                
                else
                {
                    cannotSupply = true;
                    break;
                }
            }

            // A trade is cancelled if there is not enough supply,
            // unless only 1 player is requesting
            if (cannotSupply)
                for (int i = firstPlayerIndex; i < Rules.NUM_PLAYERS; i++)
                    playerTrades[i][type] = 0;
            
            else
                playerTrades[firstPlayerIndex][type] = Bank[type];
        }

        // Execute all trades
        for (int i = 0; i < Rules.NUM_PLAYERS; i++)
            if (playerTrades[i].Count() != 0)
                DoTrade(i, -1, new(), playerTrades[i]);
    }

    private readonly int GetCurrentPlayer()
    {
        return (CurrentTurnIndex + CurrentPlayerOffset) % Rules.NUM_PLAYERS;
    }

    private void GenerateValidActions()
    {
        ValidActions.Clear();

        switch (CurrentPhase)
        {
        case Phase.TURN_START:
            ValidActions.Add(new RollDice());
            // Dev cards
            break;

        case Phase.TURN_MAIN:
            GetValidSettlementActions();
            break;

        case Phase.DISCARD:
            break;

        case Phase.ROBBER:
            break;
        }
    }

    /// <summary>
    /// Add all valid settlement actions for the current player to the valid action list
    /// </summary>
    /// <param name="pregame"></param>
    private readonly void GetValidSettlementActions(bool pregame = false)
    {
        int playerID = GetCurrentPlayer();

        // Does player have settlements remaining
        if (Players[playerID].Settlements == 0)
            return;

        // Can player afford it, ignored with pregame flag
        else if (Players[playerID].Hand < Rules.SETTLEMENT_COST && !pregame)
            return;
        
        List<Vertex.Key> nodes = Board.GetAllVertices();

        foreach(Vertex.Key nodePos in nodes)
        {
            if (!Board.TryGetVertex(nodePos, out Node node))
                continue;
            // Throw error?

            // node is already occupied
            else if (node.OwnerID != -1)
                continue;
            
            // These checks are ignored in pregame phase
            if (!pregame)
            {
                // Check if connected by road
                Edge.Key[] edges = nodePos.GetAdjacentEdges();
                
                bool connected = false;
                foreach (Edge.Key edgePos in edges)
                {
                    if (Board.TryGetEdge(edgePos, out Path path))
                        connected |= path.OwnerID == playerID;
                }

                if (!connected)
                    continue;
            }

            // Check if too close to other settlements
            Vertex.Key[] adjNodes = nodePos.GetAdjacentVertices();

            bool isValid = true;
            foreach(Vertex.Key adjNodePos in adjNodes)
            {
                if (Board.TryGetVertex(adjNodePos, out Node adjNode))
                    isValid &= adjNode.OwnerID == -1;
            }

            if (!isValid)
                continue;
            
            ValidActions.Add(new BuildSettlementAction(){
                OwnerID = playerID,
                Position = nodePos
            });
        }
    }

    private readonly void GetValidRoadActions(bool free = false)
    {
        int playerID = GetCurrentPlayer();

        // Check for remaining roads
        if (Players[playerID].Roads == 0)
            return;
        
        // Check if player can afford, ignored with free flag
        else if (Players[playerID].Hand < Rules.ROAD_COST && !free)
            return;
        
        List<Edge.Key> edges = Board.GetAllEdges();

        foreach (Edge.Key edgePos in edges)
        {
            if (!Board.TryGetEdge(edgePos, out Path edge))
                continue; // Throw error?
            
            // Check if already occupied
            else if (edge.OwnerID != -1)
                continue;
            
            // Check opposing nodes
            Vertex.Key nodes;
        }
    }

    public enum Phase{
        TURN_START,
        TURN_MAIN,
        DISCARD,
        ROBBER
    }
}