using System;
using System.Collections.Generic;
using Grid.Hexagonal;
using Utility.Graphics;

namespace Catan;
using Type = Resources.Type;

public class Game
{
    public GameState GameState;

    private Game()
    {}

    public void Draw(Canvas canvas) {
        GameState.Board.Draw(canvas);
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

        return new Game(){
            GameState = gameState
        };
    }
}