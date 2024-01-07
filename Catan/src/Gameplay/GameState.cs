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