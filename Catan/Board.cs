using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Catan;

class Board
{
    //  Maximum board size as a percentage of an axis
    private static readonly float BOARD_SCREEN_FILL_PC = .9f;

    public Board(int screenWidth, int screenHeight, SpriteFont font)
    {
        m_Font = font;

        float scaleY = (screenHeight * BOARD_SCREEN_FILL_PC) / 8f;
        float scaleX = (screenWidth * BOARD_SCREEN_FILL_PC) / 10f;

        // Use scale for smallest axis to prevent overspill
        m_Scale = MathHelper.Min(scaleX, scaleY);

        // x^2 + (.5x)^2 = x^2 + .25x^2 = 1.25x^2
        m_EdgeDist = MathF.Sqrt((m_Scale * m_Scale) * 1.25f);

        for (int i = 0; i < 19; i++)
            m_Tiles[i] = new Tile();

        PositionTiles(new Vector2(screenWidth, screenHeight) / 2f);

        GenerateBoard();
    }

    // Places tiles at starting positions
    private void PositionTiles(Vector2 centerPos)
    {
        Vector2 distTL = new Vector2(-ShapeBatcher.SIN_60, 1.5f) * m_Scale;
        Vector2 distTL2 = distTL * 2;

        Vector2 distTR = new Vector2(-distTL.X, distTL.Y);
        Vector2 distTR2 = distTR * 2;

        m_Tiles[0].Position = distTL2;
        m_Tiles[1].Position = distTL + distTR;
        m_Tiles[2].Position = distTR2;
        m_Tiles[3].Position = distTL2 - distTR;
        m_Tiles[4].Position = distTL;
        m_Tiles[5].Position = distTR;
        m_Tiles[6].Position = distTR2 - distTL;
        m_Tiles[7].Position = distTL2 - distTR2;
        m_Tiles[8].Position = distTL - distTR;
        m_Tiles[9].Position = Vector2.Zero;
        m_Tiles[10].Position = distTR - distTL;
        m_Tiles[11].Position = distTR2 - distTL2;
        m_Tiles[12].Position = distTL - distTR2;
        m_Tiles[13].Position = -distTR;
        m_Tiles[14].Position = -distTL;
        m_Tiles[15].Position = distTR - distTL2;
        m_Tiles[16].Position = -distTR2;
        m_Tiles[17].Position = -distTL - distTR;
        m_Tiles[18].Position = -distTL2;

        for(int i = 0; i < 19; i++)
            m_Tiles[i].Position += centerPos;
    }

    public void GenerateBoard(bool shuffleBoard = false)
    {
        Resource[] resourceSpread = Tile.DEFAULT_RESOURCE_SPREAD;
        int[] numSpread = Tile.DEFAULT_NUMBER_SPREAD;

        if (shuffleBoard)
        {
            Random rand = new Random();
            rand.ShuffleArray(resourceSpread, 2);
            rand.ShuffleArray(numSpread, 2);
        }

        int n = 0;
        for (int i = 0; i < 19; i++)
        {
            m_Tiles[i].Type = resourceSpread[i];

            if (resourceSpread[i] == Resource.Empty)
            {
                m_Tiles[i].Value = 7;
                continue;
            }

            m_Tiles[i].Value = numSpread[n++];
        }
    }

    public void ShapeDraw(ShapeBatcher shapeBatcher)
    {
        for (int i = 0; i < 19; i++)
            m_Tiles[i].ShapeDraw(shapeBatcher, m_Scale);
    }

    public void SpriteDraw(SpriteBatch spriteBatch, float windowHeight)
    {
        for (int i = 0; i < 19; i++)
            m_Tiles[i].SpriteDraw(spriteBatch, m_Font, windowHeight);
    }

    // TODO: Change to class, integrate draw both hex and num
    private class Tile
    {
        public Tile()
        {
            Position = Vector2.Zero;
            Type = Resource.Empty;
            Value = 0;
        }

        public Tile(Vector2 position, Resource type)
        {
            Position = position;
            Type = type;
        }

        public Vector2 Position;

        public Resource Type;

        public int Value;

        public void ShapeDraw(ShapeBatcher shapeBatcher, float scale)
        {
            shapeBatcher.DrawHex(Position, scale * .9f, GetResourceColour(Type));
        }

        public void SpriteDraw(SpriteBatch spriteBatch, SpriteFont font, float windowHeight)
        {
            spriteBatch.DrawString(font, Value.ToString(), Position.FlipY(windowHeight), Color.Black);
        }

        // Default resource layout defined by rulebook
        public static readonly Resource[] DEFAULT_RESOURCE_SPREAD = {
                    Resource.Ore, Resource.Wool, Resource.Lumber,
                Resource.Grain, Resource.Brick, Resource.Wool, Resource.Brick,
            Resource.Grain, Resource.Lumber, Resource.Empty, Resource.Lumber, Resource.Ore,
                Resource.Lumber, Resource.Ore, Resource.Grain, Resource.Wool,
                    Resource.Brick, Resource.Grain, Resource.Wool
            };

        public static readonly int[] DEFAULT_NUMBER_SPREAD = {
            10, 2, 9,
            12, 6, 4, 10,
            9, 11, 3, 8,
            8, 3, 4, 5,
            5, 6, 11
        };

        private static Color GetResourceColour(Resource resource)
        {
            switch(resource)
            {
                case Resource.Empty:
                    return Color.Wheat;

                case Resource.Lumber:
                    return Color.DarkGreen;

                case Resource.Brick:
                    return Color.OrangeRed;

                case Resource.Grain:
                    return Color.Goldenrod;
            
                case Resource.Wool:
                    return Color.LightGreen;
            
                case Resource.Ore:
                    return Color.Gray;
            }

            return Color.Black;
        }
    }

    public enum Resource
    {
        Empty,
        Lumber,
        Brick,
        Grain,
        Wool,
        Ore
    }

    private Tile[] m_Tiles = new Tile[19];

    private float m_Scale;

    private float m_EdgeDist;

    private SpriteFont m_Font;
}