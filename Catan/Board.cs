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
        
        for (int i = 0; i < 57; i++)
            m_Nodes[i] = new Node();

        Vector2 centrePos = new Vector2(screenWidth, screenHeight) / 2f;
        PositionObjects(centrePos);

        GenerateBoard();
    }

    // Places tiles at starting positions
    private void PositionObjects(Vector2 centrePos)
    {
        Vector2 hexDistTL = new Vector2(-ShapeBatcher.SIN_60, 1.5f) * m_Scale;
        Vector2 hexDistTL2 = hexDistTL * 2;

        Vector2 hexDistTR = new Vector2(-hexDistTL.X, hexDistTL.Y);
        Vector2 hexDistTR2 = hexDistTR * 2;

        m_Tiles[0].Position = hexDistTL2;
        m_Tiles[1].Position = hexDistTL + hexDistTR;
        m_Tiles[2].Position = hexDistTR2;
        m_Tiles[3].Position = hexDistTL2 - hexDistTR;
        m_Tiles[4].Position = hexDistTL;
        m_Tiles[5].Position = hexDistTR;
        m_Tiles[6].Position = hexDistTR2 - hexDistTL;
        m_Tiles[7].Position = hexDistTL2 - hexDistTR2;
        m_Tiles[8].Position = hexDistTL - hexDistTR;
        m_Tiles[9].Position = Vector2.Zero;
        m_Tiles[10].Position = hexDistTR - hexDistTL;
        m_Tiles[11].Position = hexDistTR2 - hexDistTL2;
        m_Tiles[12].Position = hexDistTL - hexDistTR2;
        m_Tiles[13].Position = -hexDistTR;
        m_Tiles[14].Position = -hexDistTL;
        m_Tiles[15].Position = hexDistTR - hexDistTL2;
        m_Tiles[16].Position = -hexDistTR2;
        m_Tiles[17].Position = -hexDistTL - hexDistTR;
        m_Tiles[18].Position = -hexDistTL2;

        for(int i = 0; i < 19; i++)
            m_Tiles[i].Position += centrePos;
        
        Vector2 up = new Vector2(0, m_Scale);
        Vector2 pointDistTL = hexDistTL - up;
        Vector2 pointDistTR = hexDistTR - up;

        for (int i = 0; i < 3; i++)
        {
            m_Nodes[i * 2].Position = m_Tiles[i].Position + pointDistTL;
            m_Nodes[(i * 2) + 1].Position = m_Tiles[i].Position + up;
        }
        m_Nodes[6].Position = m_Tiles[2].Position + pointDistTR;

        for (int i = 3; i < 7; i++)
        {
            m_Nodes[(i * 2) + 1].Position = m_Tiles[i].Position + pointDistTL;
            m_Nodes[(i + 1) * 2].Position = m_Tiles[i].Position + up;
        }
        m_Nodes[15].Position = m_Tiles[6].Position + pointDistTR;

        for (int i = 7; i < 12; i++)
        {
            m_Nodes[(i + 1) * 2].Position = m_Tiles[i].Position + pointDistTL;
            m_Nodes[(i * 2) + 3].Position = m_Tiles[i].Position + up;
        }
        m_Nodes[26].Position = m_Tiles[11].Position + pointDistTR;

        m_Nodes[27].Position = m_Tiles[7].Position - pointDistTR;
        for (int i = 12; i < 16; i++)
        {
            m_Nodes[(i + 2) * 2].Position = m_Tiles[i].Position + pointDistTL;
            m_Nodes[(i * 2) + 5].Position = m_Tiles[i].Position + up;
        }
        m_Nodes[36].Position = m_Tiles[15].Position + pointDistTR;
        m_Nodes[37].Position = m_Nodes[36].Position + pointDistTR;

        m_Nodes[38].Position = m_Nodes[28].Position - up;
        for (int i = 16; i < 19; i++)
        {
            m_Nodes[(i * 2) + 7].Position = m_Tiles[i].Position + pointDistTL;
            m_Nodes[(i + 4) * 2].Position = m_Tiles[i].Position + up;
        }
        m_Nodes[45].Position = m_Nodes[44].Position - pointDistTL;
        m_Nodes[46].Position = m_Nodes[45].Position + pointDistTR;

        for (int i = 16; i < 19; i++)
        {
            m_Nodes[(i * 2) + 15].Position = m_Tiles[i].Position - pointDistTR;
            m_Nodes[(i + 8) * 2].Position = m_Tiles[i].Position - up;
        }
        m_Nodes[53].Position = m_Nodes[52].Position + pointDistTR;
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
        
        for (int i = 0; i < 57; i++)
            m_Nodes[i].Draw(shapeBatcher);
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

        public void SpriteDraw(SpriteBatch spriteBatch, SpriteFont font, float windowHeight, int active = 6)
        {
            spriteBatch.DrawString(font, Value.ToString(), Position.FlipY(windowHeight), Value == active ? Color.Red : Color.Black);
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
                    return Color.Brown;

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

    private class Node
    {
        public Node()
        {
            OwnerID = 0;
            Position = Vector2.Zero;
        }

        public Vector2 Position;

        public int OwnerID;

        public bool IsCity = false;

        public Edge[] Edges = new Edge[3];

        public Tile[] Tiles = new Tile[3];

        public void Draw(ShapeBatcher shapeBatcher)
        {
            shapeBatcher.DrawCircle(Position, 5f, 10, 1f, Color.Black);
        }
    }

    private class Edge
    {
        // Connections ordered N->S & E->W
        public Vector2 Position;

        public int OwnerID;

        public Node[] Nodes = new Node[2];
    }

    private Tile[] m_Tiles = new Tile[19];

    private Node[] m_Nodes = new Node[57];

    private float m_Scale;

    private float m_EdgeDist;

    private SpriteFont m_Font;
}