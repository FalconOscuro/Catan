using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Numerics;
using Grid.Hexagonal;
using ImGuiNET;

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
    public readonly Player[] Players; // TEMP, CHANGE!!!!!

    // Should be hidden
    public Stack<DevCards.Type> DevCardDeck = new();

    public List<DevCards.Type> PlayedDevCards = new();

    public int LargestArmyOwnerID { get; private set; }

    /// <summary>
    /// Offset from <see cref="CurrentTurnIndex"/>, used for out of turn actions.
    /// </summary>
    public int CurrentPlayerOffset = 0;

    /// <summary>
    /// Absolute current player turn
    /// </summary>
    /// <remarks>
    /// For current player accounting for <see cref="CurrentPlayerOffset"/> use <see cref="GetCurrentPlayerID"/>.
    /// </remarks>
    public int CurrentTurnIndex = 0;

    /// <summary>
    /// Sum result of the last dice roll
    /// </summary>
    public (int, int) LastRoll { get; private set; }

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
    
    private List<Action.IAction> m_AllActions = new();

    public GameState()
    {
        Players = new Player[Rules.NUM_PLAYERS];
        for (int i = 0; i < Rules.NUM_PLAYERS; i++)
            Players[i] = new(i);
        
        LargestArmyOwnerID = -1;
    }

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
        Players[GetCurrentPlayerID()].DMM.Update(this);
    }

    public bool HasGameEnded()
    {
        return Players[CurrentTurnIndex].GetTotalVP() >= Rules.MAX_VICTORY_POINTS;
    }

    /// <summary>
    /// Called on <see cref="IAction.Execute(GameState)"/>.
    /// </summary>
    /// <param name="action">Triggering action.</param>
    public void UpdatePhase(Action.IAction action)
    {
        // Add action to played action list
        if (!action.IsSilent)
        {
            m_AllActions.Add(action);

            if (!action.IsHidden)
                PlayedActions.Add(action);
        }

        // Update FSM & retrieve valid actions
        PhaseManager.Update(this, action);
        Players[GetCurrentPlayerID()].DMM.Actions = PhaseManager.GetValidActions(this);
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
        LastRoll = (d1, d2);

        return LastRoll;
    }

    public DevCards.Type GetNextDevCard()
    {
        if (DevCardDeck.Count < 1)
            throw new Exception("Attempted to buy dev card, but deck was empty!");

        // Should not be immediately playable
        return DevCardDeck.Pop();
    }

    public void SetDevCardToPlayed(DevCards.Type type, int ownerID)
    {
        Player player = Players[ownerID];

        if (!player.HeldDevCards.Remove(type))
            throw new Exception("Failed to find specified dev card in player hand");

        PlayedDevCards.Add(type);
        player.HasPlayedDevCard = true;
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

    public void UpdateLargestArmy(int playerID)
    {
        if (playerID == LargestArmyOwnerID)
            return;
        
        bool unOwned = LargestArmyOwnerID == -1;
        int largestArmy = unOwned ? 2 : Players[LargestArmyOwnerID].KnightsPlayed;

        if (Players[playerID].KnightsPlayed > largestArmy)
        {
            if (!unOwned)
                Players[LargestArmyOwnerID].LargestArmy = false;
            
            Players[playerID].LargestArmy = true;
            LargestArmyOwnerID = playerID;
        }
    }

    /// <summary>
    /// Get current active player, accounting for offset.
    /// </summary>
    public int GetCurrentPlayerID()
    {
        return (CurrentTurnIndex + CurrentPlayerOffset) % Rules.NUM_PLAYERS;
    }

    public Player GetCurrentPlayer()
    {
        return Players[GetCurrentPlayerID()];
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

    public void ImDraw()
    {
        string phaseMsg;
        if (!HasGameEnded())
            phaseMsg = string.Format("Turn: {0} - {1}", GetCurrentPlayerID(), PhaseManager.CurrentPhase);
        
        else
            phaseMsg = string.Format("Player {0} wins!", GetCurrentPlayerID());


        ImGui.Text(string.Format("Dice Roll: {0}", LastRoll));
        ImGui.Text(phaseMsg);

        if (ImGui.CollapsingHeader("Actions"))
            Action.IAction.ImDrawActList(m_AllActions, "PlayedActions");

        if (ImGui.CollapsingHeader("Resources"))
        {
            if (ImGui.TreeNode("Bank"))
            {
                Bank.ImDraw();
                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Dev Cards"))
            {
                Vector2 boxSize = new(
                    ImGui.GetContentRegionAvail().X / 3, ImGui.GetTextLineHeightWithSpacing() * 5
                );

                if (ImGui.TreeNode("Unplayed"))
                {
                    ImGui.BeginListBox("##Unplayed Dev Cards", boxSize);

                    for (int i = 0; i < DevCardDeck.Count; i++)
                        ImGui.Selectable($"{i}: {DevCardDeck.ElementAt(i)}");

                    ImGui.EndListBox();
                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Played"))
                {
                    ImGui.BeginListBox("##Played Dev Cards", boxSize);

                    for (int i = 0; i < PlayedDevCards.Count; i++)
                        ImGui.Selectable($"{i}: {PlayedDevCards[i]}");

                    ImGui.EndListBox();
                    ImGui.TreePop();
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Special Cards"))
            {
                ImGui.Text("Largest Army Owner:");
                ImGui.SameLine();
                
                if (LargestArmyOwnerID == -1)
                    ImGui.Text("None");
                
                else
                    ImGui.TextColored(Rules.GetPlayerIDColour(LargestArmyOwnerID).ToVector4().ToNumerics(), $"Player {LargestArmyOwnerID}");

                ImGui.TreePop();
            }
        }

        // Players arranged as tabs
        // NOTE: Current method does not allow for switching of tabs to left if viewing a players valid actions FIX
        if (ImGui.CollapsingHeader("Players"))
        {
            if (ImGui.BeginTabBar("Players"))
            {
                for (int i = 0; i < Rules.NUM_PLAYERS; i++)
                {
                    if(ImGui.BeginTabItem(i.ToString()))
                    {
                        Players[i].ImDraw();
                        ImGui.EndTabItem();
                    }
                }

                ImGui.EndTabBar();
            }
        }
    }
}