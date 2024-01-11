using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Numerics;
using Catan.Action;
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

    public int LongestRoadOwnerID { get; private set; }

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

    public int LastRollSum {
        get {
            return LastRoll.Item1 + LastRoll.Item2;
        }
    }

    /// <summary>
    /// FSM
    /// </summary>
    public State.GamePhaseManager PhaseManager = new();

    // TODO: Init with seed
    public Random Random = new();

    /// <summary>
    /// Log of all executed actions
    /// </summary>
    public List<IAction> PlayedActions = new();
    
    private List<IAction> m_AllActions = new();

    public GameState()
    {
        Players = new Player[Rules.NUM_PLAYERS];
        for (int i = 0; i < Rules.NUM_PLAYERS; i++)
            Players[i] = new(i);
        
        LargestArmyOwnerID = -1;
        LongestRoadOwnerID = -1;
    }

    /// <summary>
    /// Creates a deep clone.
    /// </summary>
    /// <remarks>
    /// Useful for simulation.
    /// WARNING: INCOMPLETE
    /// </remarks>
    public GameState Clone()
    {
        GameState clone = new(){
            LastRoll = LastRoll,
            Board = Board.Clone(),
            Bank = Bank.Clone(),
            CurrentTurnIndex = CurrentTurnIndex,
            CurrentPlayerOffset = CurrentPlayerOffset,
            RobberPos = RobberPos,
            TileValueMap = new(TileValueMap),
            m_AllActions =  new(m_AllActions),
            PlayedActions = new(PlayedActions),
            PhaseManager = PhaseManager.Clone(),
            LongestRoadOwnerID = LongestRoadOwnerID,
            LargestArmyOwnerID = LargestArmyOwnerID,
            DevCardDeck = new(DevCardDeck),
            PlayedDevCards = new(PlayedDevCards)
        };

        for (int i = 0; i < Rules.NUM_PLAYERS; i++)
            clone.Players[i] = Players[i].Clone();

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
        int largestArmy = unOwned ? Rules.MIN_LARGEST_ARMY : Players[LargestArmyOwnerID].KnightsPlayed;

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

    private void ChangeLongestRoadHolder(int playerID)
    {
        if (LongestRoadOwnerID != -1)
            Players[LongestRoadOwnerID].LongestRoad = false;
        
        LongestRoadOwnerID = playerID;

        if (playerID != -1)
            Players[playerID].LongestRoad = true;
    }

    public void CheckRoadBreak(Vertex.Key nodePos, int playerID)
    {
        List<Edge.Key> edges = new();
        int targetID = -1;
        bool breakFound = false;

        foreach (Edge.Key edgePos in nodePos.GetProtrudingEdges())
        {
            if (!Board.TryGetEdge(edgePos, out Path edge))
                return;
            
            else if (edge.OwnerID == -1)
                return;
            
            else if (edge.OwnerID == playerID)
                continue;
            
            edges.Add(edgePos);

            breakFound = targetID == edge.OwnerID;
            targetID = edge.OwnerID;
        }

        // No break found
        if (!breakFound)
            return;

        // Possible break, check if both edges included in longest road
        foreach (Edge.Key edgePos in edges)
            if (!Players[targetID].LongestRoadPath.Contains(edgePos))
                return;
        
        // Find new longest road
        int oldLen = Players[targetID].LongestRoadPath.Count;
        Players[targetID].LongestRoadPath.Clear();

        // Find new longest road
        foreach (Edge.Key edgePos in Board.GetAllEdges())
            UpdateLongestRoad(edgePos, targetID);
        
        // check if length changed and was longest road holder
        int newLen = Players[targetID].LongestRoadPath.Count;
        if (newLen >= oldLen || targetID != LongestRoadOwnerID) // newLen > oldLen should be impossible
            return;
        
        // Find new holder
        int holder = targetID;
        int bestLen = Players[targetID].LongestRoadPath.Count;
        bool contested = false;

        for (int i = 0; i < Rules.NUM_PLAYERS; i++)
        {
            int current = Players[i].LongestRoadPath.Count;

            if (current > bestLen)
            {
                holder = i;
                contested = false;
                bestLen = Players[holder].LongestRoadPath.Count;
            }

            else if (current == bestLen)
                contested = true;
        }

        // Prioritizes previous holder
        if ((!contested || holder == targetID) && bestLen > Rules.MIN_LONGEST_ROAD)
            ChangeLongestRoadHolder(holder);
        
        // No viable players, goes unowned
        else
            ChangeLongestRoadHolder(-1);
    }

    public void UpdateLongestRoad(Edge.Key start, int playerID)
    {
        if (!Board.TryGetEdge(start, out Path edge))
            return;
        
        else if (edge.OwnerID != playerID)
            return;

        Player player = Players[playerID];

        Vertex.Key[] nodes = start.GetEndpoints();
        IEnumerable<Edge.Key> longest = new Edge.Key[]{start};

        foreach (Vertex.Key nodePos in nodes)
            longest = RecurseFindLongestRoute(Board, nodePos, playerID, longest);

        int currentLength = player.LongestRoadPath.Count;
        int newLength = longest.Count();

        if (newLength > currentLength)
        {
            player.LongestRoadPath = longest.ToList();

            if (LongestRoadOwnerID == -1)
            {
                if (newLength <= Rules.MIN_LONGEST_ROAD)
                    return;

                bool contested = false;

                for (int i = 0; i < Rules.NUM_PLAYERS; i++)
                    contested |= Players[i].LongestRoadPath.Count >= newLength && i != playerID;

                if (!contested)
                    ChangeLongestRoadHolder(playerID);
            }

            else if (newLength > Players[LongestRoadOwnerID].LongestRoadPath.Count)
                ChangeLongestRoadHolder(playerID);
        }
    }

    private static IEnumerable<Edge.Key> RecurseFindLongestRoute(HexGrid board, Vertex.Key currentNode, int playerID, IEnumerable<Edge.Key> route)
    {
        if (!board.TryGetVertex(currentNode, out Node node))
            return route;

        else if (node.OwnerID != playerID && node.OwnerID != -1)
            return route;

        IEnumerable<Edge.Key> longest = route;

        Edge.Key[] edges = currentNode.GetProtrudingEdges();
        foreach (Edge.Key edgePos in edges)
        {
            if (!board.TryGetEdge(edgePos, out Path edge) || route.Contains(edgePos))
                continue;

            else if (edge.OwnerID != playerID)
                continue;
            
            Vertex.Key[] nodes = edgePos.GetEndpoints();
            Vertex.Key nextNode = nodes[0] == currentNode ? nodes[1] : nodes[0];

            IEnumerable<Edge.Key> newRoute = RecurseFindLongestRoute(board, nextNode, playerID, route.Append(edgePos));
            if (newRoute.Count() > longest.Count())
                longest = newRoute;
        }

        return longest;
    }

    public void ImDraw()
    {
        string phaseMsg;
        if (!HasGameEnded())
            phaseMsg = string.Format("Turn: {0} - {1}", GetCurrentPlayerID(), PhaseManager.CurrentPhase);
        
        else
            phaseMsg = string.Format("Player {0} wins!", GetCurrentPlayerID());


        ImGui.Text($"Dice Roll: {LastRoll} = {LastRollSum}");
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
                    ImGui.TextColored(Rules.GetPlayerIDColour(LargestArmyOwnerID).ToVector4().ToNumerics(), $"Player {LargestArmyOwnerID} ({Players[LargestArmyOwnerID].KnightsPlayed})");

                ImGui.Text("Longest Road Owner:");
                ImGui.SameLine();

                if (LongestRoadOwnerID == -1)
                    ImGui.Text("None");
                
                else
                    ImGui.TextColored(Rules.GetPlayerIDColour(LongestRoadOwnerID).ToVector4().ToNumerics(), $"Player {LongestRoadOwnerID} ({Players[LongestRoadOwnerID].LongestRoadPath.Count})");

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