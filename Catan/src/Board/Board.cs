using System;
using System.Linq;
using System.Reflection.Metadata;
using Microsoft.Xna.Framework;

namespace Catan;

/// <summary>
/// The setup and current state of the gameboard
/// </summary>
public class Board
{
    private readonly Tile[] m_Tiles = new Tile[19];

    private readonly Game m_Game;

    private static readonly float SCREEN_FILL_PERCENT = 0.9f;

    public static readonly Resources.Collection RESOURCE_TILE_COUNT = new(){
        Brick = 3,
        Grain = 4,
        Lumber = 4,
        Ore = 3,
        Wool = 4
    };

    public Board(Game game)
    {
        m_Game = game;

        InitTiles();
    }

    /// <summary>
    /// Position and assign types to all tiles
    /// </summary>
    private void InitTiles()
    {
        int tileCount = 0;

        // Position central tile
        m_Tiles[tileCount++].LocalPosition = Vector2.Zero;
        float deltaAngle = MathF.PI / 3;

        // Depth = distance from central tile
        for (int depth = 1; depth < 3; depth++)
        {
            // Angle from centre to current tile
            float angle = deltaAngle / 2;

            // Angle from current tile to next tile on same side
            float sepAngle = angle + deltaAngle * 2;

            for (int side = 0; side < 6; side++)
            {
                Vector2 pos = new Vector2(MathF.Sin(angle), MathF.Cos(angle)) * depth;
                Vector2 separation = new(MathF.Sin(sepAngle), MathF.Cos(sepAngle));

                // tiles per side = depth
                for (int tile = 0; tile < depth; tile++)
                {
                    m_Tiles[tileCount++].LocalPosition = pos;
                    pos += separation;
                }

                angle += deltaAngle;
                sepAngle += deltaAngle;
            }
        }

        Resources.Type[] resourceSpread = new Resources.Type[19];
        Tile.DEFAULT_RESOURCE_SPREAD.CopyTo(resourceSpread, 0);

        int[] valueSpread = new int[18];
        Tile.DEFAULT_NUMBER_SPREAD.CopyTo(valueSpread, 0);
        bool desertFound = false;

        for (int i = 0; i < 19; i++)
        {
            m_Tiles[i].Resource = resourceSpread[i];

            if (m_Tiles[i].Resource == Resources.Type.Empty)
            {
                desertFound = true;
                m_Tiles[i].Robber = true;
            }
            
            else
                m_Tiles[i].Value = valueSpread[i - (desertFound ? 1 : 0)];
        }
    }

    public void Draw()
    {
        Vector2 windowSize = new()
        {
            X = m_Game.GraphicsDevice.Viewport.Width,
            Y = m_Game.GraphicsDevice.Viewport.Height
        };

        Vector2 offset = windowSize * 0.5f;
        float scale = MathF.Min(windowSize.X, windowSize.Y) * SCREEN_FILL_PERCENT / 5;

        for (int i = 0; i < 19; i++)
            m_Tiles[i].Draw(offset, scale);
    }
}