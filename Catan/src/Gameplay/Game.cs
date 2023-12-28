using System;
using System.Collections.Generic;
using System.Numerics;
using Grid.Hexagonal;
using ImGuiNET;
using Utility.Graphics;

namespace Catan;
using Type = Resources.Type;

/// <summary>
/// Controls a singular <see cref="GameState"/>
/// </summary>
/// <remarks>
/// For use with monogame.
/// </remarks>
public class Game
{
    public GameState GameState;

    private Game()
    {}

    /// <summary>
    /// Execute single update tick
    /// </summary>
    public void Update()
    {
        GameState.Update();
    }

    /// <summary>
    /// Draw all game elements
    /// </summary>
    public void Draw(Canvas canvas) {
        GameState.Board.Draw(canvas);
    }

    /// <summary>
    /// Draw debug elements with ImGui
    /// </summary>
    public void ImDraw()
    {
        // Move to Gamestate class?

        ImGui.Text(string.Format("Dice Roll: {0}", GameState.LastRoll));
        ImGui.Text(string.Format("Turn: {0} - {1}", GameState.GetCurrentPlayer(), GameState.PhaseManager.CurrentPhase));

        if (ImGui.CollapsingHeader("Actions"))
            IAction.ImDrawActList(GameState.PlayedActions, "PlayedActions");

        if (ImGui.CollapsingHeader("Bank"))
            GameState.Bank.ImDraw();

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
                        // Move to player class
                        ImGui.TextColored(Rules.GetPlayerIDColour(i).ToVector4().ToNumerics(), "Colour");

                        GameState.Players[i].ImDraw();
                        ImGui.EndTabItem();
                    }
                }

                ImGui.EndTabBar();
            }
        }
    }

    // Default start
    // Random map

    /// <summary>
    /// Create new game using default map
    /// </summary>
    public static Game NewDefaultMapGame() {
        return NewGame(Rules.DEFAULT_RESOURCE_SPREAD, Rules.DEFAULT_VALUE_SPREAD);
    }

    /// <summary>
    /// Create new game specifying resource and value layout
    /// </summary>
    public static Game NewGame(Type[] resourceMap, int[] valueMap)
    {
        GameState gameState = new(){
            Bank = Rules.BANK_START.Clone()
        };

        // Need offset and rot?
        int resourceIndex = 0;
        int valueIndex = 0;

        int qStart = -Rules.BOARD_WIDTH / 2;
        int qEnd = qStart + Rules.BOARD_WIDTH;

        // iterate across columns
        for (Axial pos = new(){Q = qStart}; pos.Q < qEnd; pos.Q++)
        {
            int rStart = Math.Max(qStart, qStart - pos.Q);
            int rEnd = rStart + Rules.BOARD_WIDTH - Math.Abs(pos.Q);

            // iterate across rows
            for (pos.R = rStart; pos.R < rEnd; pos.R++)
            {
                // Create tile
                Tile tile = new(){
                    Resource = resourceMap[resourceIndex++]
                };

                // If desert make robber
                if (tile.Resource == Type.Empty)
                {
                    tile.Robber = true;
                    gameState.RobberPos = pos;
                }

                // Else assign value
                else
                {
                    tile.Value = valueMap[valueIndex++];

                    // Map value to tile pos
                    if (!gameState.TileValueMap.ContainsKey(tile.Value))
                        gameState.TileValueMap[tile.Value] = new();

                    gameState.TileValueMap[tile.Value].Add(pos);
                }

                gameState.Board.InsertHex(pos, tile);

                // Ensure surrounding paths are created
                for (Edge.Key key = new(){Position = pos}; key.Side < Edge.Side.SW + 1; key.Side++)
                    if (!gameState.Board.TryGetEdge<Edge>(key, out _))
                        gameState.Board.InsertEdge(key, new Path());

                // Ensure surrounding intersections are created
                for (Vertex.Key key = new(){Position = pos}; key.Side < Vertex.Side.SW + 1; key.Side++)
                    if (!gameState.Board.TryGetVertex<Vertex>(key, out _))
                        gameState.Board.InsertVertex(key, new Node());
            }

            // TODO: Ports
            // TODO: Players
            // TODO: Dev Cards
        }

        // Should be a part of gamestate
        gameState.Players[gameState.GetCurrentPlayer()].DMM.Actions = gameState.PhaseManager.GetValidActions(gameState);

        return new Game(){
            GameState = gameState
        };
    }
}