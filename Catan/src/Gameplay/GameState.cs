using System;
using System.Collections.Generic;
using System.Numerics;
using Grid.Hexagonal;

namespace Catan;

/// <summary>
/// A model describing the current game state,
/// alongside basic action functions
/// </summary>
public class GameState
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
    public readonly Player[] Players = new Player[]{new(), new(), new(), new()}; // TEMP, CHANGE!!!!!

    /// <summary>
    /// Offset from <see cref="CurrentTurnIndex"/>, used for out of turn actions.
    /// </summary>
    public int CurrentPlayerOffset = 0;

    /// <summary>
    /// Absolute current player turn
    /// </summary>
    /// <remarks>
    /// For current player accounting for <see cref="CurrentPlayerOffset"/> use <see cref="GetCurrentPlayer"/>.
    /// </remarks>
    public int CurrentTurnIndex = 0;

    /// <summary>
    /// Sum result of the last dice roll
    /// </summary>
    public int LastRoll = 0;

    /// <summary>
    /// FSM
    /// </summary>
    public State.GamePhaseManager PhaseManager = new();

    // TODO: Init with seed
    public Random Random = new();

    /// <summary>
    /// Log of all executed actions
    /// </summary>
    public List<Action.IAction> PlayedActions = new();
    
    public GameState()
    {}

    /// <summary>
    /// Creates a deep clone.
    /// </summary>
    /// <remarks>
    /// Useful for simulation.
    /// </remarks>
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

    /// <summary>
    /// Execute update tick for current player, utilizing <see cref="DMM"/>.
    /// </summary>
    public void Update()
    {
        Players[GetCurrentPlayer()].DMM.Update(this);
    }

    /// <summary>
    /// Called on <see cref="IAction.Execute(GameState)"/>.
    /// </summary>
    /// <param name="action">Triggering action.</param>
    public void UpdatePhase(Action.IAction action)
    {
        // Add action to played action list
        PlayedActions.Add(action);

        // Update FSM & retrieve valid actions
        PhaseManager.Update(this, action);
        Players[GetCurrentPlayer()].DMM.Actions = PhaseManager.GetValidActions(this);
    }

    /// <summary>
    /// Increments <see cref="CurrentTurnIndex"/> whilst avoiding IndexOutOfRange error.
    /// </summary>
    public void AdvanceTurn()
    {
        CurrentTurnIndex = (CurrentTurnIndex + 1) % Rules.NUM_PLAYERS;
    }

    // Should be able to specify dice roll for simulation
    // TODO: Overhaul
    public (int, int) RollDice()
    {
        int d1 = Random.Next(1, 7);
        int d2 = Random.Next(1, 7);

        return (d1, d2);
    }

    /// <summary>
    /// Trading logic called by <see cref="Trade"/> Command.
    /// </summary>
    /// <remarks>
    /// Does not check if trade is valid!
    /// </remarks>
    /// <param name="targetID">If set to -1 targets <see cref="Bank"/></param>
    public void DoTrade(int ownerID, int targetID, Resources.Collection giving, Resources.Collection recieving)
    {
        Players[ownerID].Hand += recieving - giving;

        if (targetID == -1)
            Bank += giving - recieving;
        
        else
            Players[targetID].Hand += giving - recieving;
    }

    /// <summary>
    /// Settlement building logic called by <see cref="BuildSettlementAction"/>.
    /// </summary>
    /// <remarks>
    /// WARNING: Does not check if valid!
    /// </remarks>
    /// <param name="free">Used during <see cref="PreGameSettlement"/> phase</param>
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

    /// <summary>
    /// Road building logic called by <see cref="BuildRoadAction"/>.
    /// </summary>
    /// <remarks>
    /// WARNING: Does not check if valid!
    /// </remarks>
    /// <param name="free">Used during <see cref="PreGameRoad"/> and RoadBuilding</param>
    public void BuildRoad(int ownerID, Edge.Key position, bool free)
    {
        Players[ownerID].Roads--;
        
        if (!Board.TryGetEdge(position, out Path path))
            throw new Exception();
        
        path.OwnerID = ownerID;

        if (!free)
            DoTrade(ownerID, -1, Rules.ROAD_COST, new());
    }

    /// <summary>
    /// City building logic called by <see cref="BuildCityAction"/>.
    /// </summary>
    /// <remarks>
    /// WARNING: Does not check if valid!
    /// </remarks>
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

    public void MoveRobber(Axial position, int targetID, int ownerID)
    {
        // Remove from old tile
        Board.TryGetHex(RobberPos, out Tile tile);
        tile.Robber = false;

        // Add to new tile
        RobberPos = position;
        Board.TryGetHex(RobberPos, out tile);
        tile.Robber = true;

        // No resource stolen
        if (targetID == -1 || ownerID == -1)
            return;

        // Steal random resource
        int targetHandSize = Players[targetID].Hand.Count();

        // No cards to steal
        if (targetHandSize < 1)
            return;
        
        int stolenCard = Random.Next(targetHandSize) + 1;

        Resources.Type type = Resources.Type.Empty;
        int count = 0;

        // Find type of stolen card
        while (stolenCard > count)
        {
            type ++;
            count += Players[targetID].Hand[type];
        }

        Players[targetID].Hand[type] -= 1;
        Players[ownerID].Hand[type] += 1;
    }

    // buy dev card
    // play dev card
    // Steal

    /// <summary>
    /// Resource distribution after a non-robber <see cref="RollDice"/>.
    /// </summary>
    public void DistributeResources()
    {
        Resources.Collection[] playerTrades = new Resources.Collection[Rules.NUM_PLAYERS];

        for (int i = 0; i < Rules.NUM_PLAYERS; i++)
            playerTrades[i] = new();

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
            {
                DoTrade(i, -1, new(), playerTrades[i]);

                // Create dummy action for log
                Action.Trade trade = new(){
                    OwnerID = i,
                    TargetID = -1,
                    Recieving = playerTrades[i].Clone()
                };
                PlayedActions.Add(trade);
            }
    }

    /// <summary>
    /// Get current active player, accounting for offset.
    /// </summary>
    public int GetCurrentPlayer()
    {
        return (CurrentTurnIndex + CurrentPlayerOffset) % Rules.NUM_PLAYERS;
    }

    /// <summary>
    /// Add all valid settlement actions for the current player to the valid action list
    /// </summary>
    /// <param name="pregame"></param>
    public void GetValidSettlementActions(int playerID, List<Action.IAction> validActions, bool pregame = false)
    {
        // Does player have settlements remaining
        if (Players[playerID].Settlements == 0)
            return;

        // Can player afford it, ignored with pregame flag
        else if (!(Rules.SETTLEMENT_COST <= Players[playerID].Hand || pregame))
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
                Edge.Key[] edges = nodePos.GetProtrudingEdges();
                
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
            
            validActions.Add(new Action.BuildSettlementAction(playerID, nodePos));
        }
    }

    /// <summary>
    /// Evaluate eligibility for single settlement
    /// </summary>
    /// <param name="pregame"><see cref="PreGameSettlement"/> has no cost and looser placement restrictions.</param>
    public bool CheckSettlementPos(int playerID, Vertex.Key pos, bool pregame = false)
    {
        // Could not find node
        if (!Board.TryGetVertex(pos, out Node node))
            return false;
        
        // Already owned
        else if (node.OwnerID != -1)
            return false;
        
        // Connection check
        if (!pregame)
        {
            // Check if connected by road
            Edge.Key[] edges = pos.GetProtrudingEdges();
                
            bool connected = false;
            foreach (Edge.Key edgePos in edges)
            {
                if (Board.TryGetEdge(edgePos, out Path path))
                    connected |= path.OwnerID == playerID;
            }

            if (!connected)
                return false;
        }

        // Check if too close to other settlements
        Vertex.Key[] adjNodes = pos.GetAdjacentVertices();

        bool isValid = true;
        foreach(Vertex.Key adjNodePos in adjNodes)
        {
            if (Board.TryGetVertex(adjNodePos, out Node adjNode))
                isValid &= adjNode.OwnerID == -1;
        }

        return isValid;
    }

    /// <summary>
    /// Add all valid city actions for current player to the valid action list
    /// </summary>
    public void GetValidCityActions(int playerID, List<Action.IAction> validActions)
    {
        // Check if player has remaining cities, or replaceable settlements
        if (Players[playerID].Cities == 0 || Players[playerID].Settlements == Rules.MAX_SETTLEMENTS)
            return;
        
        // Check if can afford
        else if (!(Rules.CITY_COST <= Players[playerID].Hand))
            return;
        
        List<Vertex.Key> nodes = Board.GetAllVertices();
        foreach (Vertex.Key nodePos in nodes)
        {
            if (!Board.TryGetVertex(nodePos, out Node node))
                continue; // Should be impossible, throw error?
            
            else if (node.OwnerID == playerID && !node.City)
                validActions.Add(new Action.BuildCityAction(playerID, nodePos));
        }
    }

    public void GetValidRoadActions(int playerID, List<Action.IAction> validActions, bool free = false)
    {
        // Check for remaining roads
        if (Players[playerID].Roads == 0)
            return;
        
        // Check if player can afford, ignored with free flag
        else if (!(Rules.ROAD_COST <= Players[playerID].Hand || free))
            return;
        
        List<Edge.Key> edges = Board.GetAllEdges();

        foreach (Edge.Key edgePos in edges)
        {
            if (CheckRoadPos(edgePos, playerID))
                validActions.Add(new Action.BuildRoadAction(playerID, edgePos));
        }
    }

    /// <summary>
    /// Evaluates eligibility for a single road
    /// </summary>
    public bool CheckRoadPos(Edge.Key pos, int playerID)
    {
        if (!Board.TryGetEdge(pos, out Path edge))
            return false; // Should be impossible, throw error?

        // Path already owned
        else if (edge.OwnerID != -1)
            return false;
        
        Vertex.Key[] nodes = pos.GetEndpoints();

        foreach(Vertex.Key nodePos in nodes)
        {
            if (!Board.TryGetVertex(nodePos, out Node node))
                continue; // Should be impossible, throw error?
            
            // Path connects to owned node
            else if (node.OwnerID == playerID)
                return true;
            
            // Owned by other player, cannot build through
            else if (node.OwnerID != -1)
                continue;
            
            // Loop protruding edges from current node
            // Searching for connected roads
            Edge.Key[] adjEdges = nodePos.GetProtrudingEdges();
            foreach(Edge.Key adjEdgePos in adjEdges)
            {
                // Skip target edge
                if (adjEdgePos == pos)
                    continue;
                
                if (!Board.TryGetEdge(adjEdgePos, out Path adjEdge))
                    continue;
                
                // Found connected road
                else if (adjEdge.OwnerID == playerID)
                    return true;
            }
        }

        // All possibilites checked, cannot build here
        return false;
    }

    // Potential different phases of the game
    public enum Phase{
        TURN_START,
        TURN_MAIN,
        DISCARD,
        ROBBER
    }
}