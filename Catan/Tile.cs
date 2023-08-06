using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Catan;

class Tile
{
    public Tile(int id, Board board)
    {
        Position = Vector2.Zero;
        Type = Resources.Type.Empty;
        Value = 0;

        Selected = false;
        Robber = false;
        ID = id;

        m_Board = board;
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    /// <param name="tile"> old tile </param>
    /// <param name="board"> new board parent </param>
    public Tile(Tile tile, Board board)
    {
        Position = tile.Position;
        Type = tile.Type;
        Value = tile.Value;

        Selected = false;
        Robber = tile.Robber;
        ID = tile.ID;

        Array.Copy(tile.m_Nodes, m_Nodes, 6);

        m_Board = board;
    }

    public Node GetNode(int index)
    {
        if (index < 0 || index >= 6 || m_Board == null)
            return null;
        
        int nodeID = m_Nodes[index];        
        return nodeID == -1 ? null : m_Board.Nodes[nodeID];
    }

    public void SetNodeID(int index, int nodeID)
    {
        if (index < 0 || index >= 6 || nodeID < -1 || nodeID > 53)
            return;
        
        m_Nodes[index] = nodeID;
    }

    public Vector2 Position;

    public Resources.Type Type;

    public int ID { get; private set; }

    public int Value;

    private readonly int[] m_Nodes = new int[] { -1, -1, -1, -1, -1, -1 };

    private readonly Board m_Board;

    public bool Selected;

    public bool Robber;

    public bool TestCollision(Vector2 point, Vector2 offset, float scale)
    {
        // Heavily simplified, using approximation of inner circle for collision
        // .75f is magic number, actual scale is .9f of input scale & shortest edge distane is ~= .87 of scale
        // .87 * .9f squared is then ~= to magic number .75f
        return Vector2.DistanceSquared(point, (Position * scale) + offset) < scale * scale * .75f;
    }

    public List<Trade> Distribute(Catan board)
    {
        List<Trade> trades = new();

        if (Robber)
            return trades;

        for (int i = 0; i < 6; i++)
        {
            Node node = GetNode(i);

            if (node == null)
                continue;

            else if (node.OwnerID != -1)
            {
                Trade trade = new(board);
                trade.Giving.AddType(Type, node.IsCity ? 2 : 1);
                trade.ToID = node.OwnerID;

                trades.Add(trade);
            }
        }

        return trades;
    }

    public void ShapeDraw(ShapeBatcher shapeBatcher, Vector2 offset, float scale)
    {
        // Hexagon is basically a 6 sided circle ¯\_(ツ)_/¯
        shapeBatcher.DrawFilledCircle((Position * scale) + offset, (scale + (Selected ? 2f : 0f)) * .9f, 6, Resources.GetResourceColour(Type));
    }

    public void SpriteDraw(SpriteBatch spriteBatch, Vector2 offset, float scale, SpriteFont font, float windowHeight, int active)
    {
        string text;
        if (Robber)
            text = "R";

        else if (Value == 0)
            return;

        else
            text = Value.ToString();

        Color colour;
        if (Value == active)
        {
            if (Robber)
                colour = Color.Gray;

            else
                colour = Color.Red;
        }

        else
            colour = Color.Black;

        spriteBatch.DrawString(font, text, ((Position * scale) + offset).FlipY(windowHeight), colour);
    }

    public int GetProbability()
    {
        if (Value == 0)
            return 0;

        return 6 - Math.Abs(7 - Value);
    }

    // Default resource layout defined by rulebook
    public static readonly Resources.Type[] DEFAULT_RESOURCE_SPREAD = {
                    Resources.Type.Ore, Resources.Type.Wool, Resources.Type.Lumber,
                Resources.Type.Grain, Resources.Type.Brick, Resources.Type.Wool, Resources.Type.Brick,
            Resources.Type.Grain, Resources.Type.Lumber, Resources.Type.Empty, Resources.Type.Lumber, Resources.Type.Ore,
                Resources.Type.Lumber, Resources.Type.Ore, Resources.Type.Grain, Resources.Type.Wool,
                    Resources.Type.Brick, Resources.Type.Grain, Resources.Type.Wool
            };

    public static readonly int[] DEFAULT_NUMBER_SPREAD = {
            10, 2, 9,
            12, 6, 4, 10,
            9, 11, 3, 8,
            8, 3, 4, 5,
            5, 6, 11
        };
}