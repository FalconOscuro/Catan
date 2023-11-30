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

    private static readonly float SCREEN_FILL_PERCENT = 0.8f;

    public Board(Game game)
    {
        m_Game = game;

        InitTiles();
    }

    private void InitTiles()
    {
        int tileCount = 0;
        m_Tiles[tileCount++].Position = Vector2.Zero;
        float deltaAngle = MathF.PI / 3;

        for (int depth = 1; depth < 3; depth++)
        {
            float angle = deltaAngle / 2;
            float sepAngle = angle + deltaAngle * 2;
            for (int side = 0; side < 6; side++)
            {
                Vector2 pos = new Vector2(MathF.Sin(angle), MathF.Cos(angle)) * depth;
                Vector2 separation = new(MathF.Sin(sepAngle), MathF.Cos(sepAngle));

                for (int tile = 0; tile < depth; tile++)
                {
                    m_Tiles[tileCount++].Position = pos;
                    pos += separation;
                }

                angle += deltaAngle;
                sepAngle += deltaAngle;
            }
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