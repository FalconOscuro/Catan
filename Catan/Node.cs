using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Catan;

/// <summary>
/// Single board node
/// </summary>
class Node
{
    public Node(int id, Board board)
    {
        Position = Vector2.Zero;
        OwnerID = -1;
        IsCity = false;
        m_Hovered = false;
        Selected = false;
        PortType = Port.TradeType.Empty;
        ID = id;

        m_Board = board;
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    /// <param name="node">old node</param>
    /// <param name="board">new board parent</param>
    public Node(Node node, Board board)
    {
        Position = node.Position;
        OwnerID = node.OwnerID;
        IsCity = node.IsCity;

        m_Hovered = false;
        Selected = false;

        PortType = node.PortType;
        ID = node.ID;

        Array.Copy(node.m_Edges, m_Edges, 3);
        Array.Copy(node.m_Tiles, m_Tiles, 3);

        m_Board = board;
    }

    public Tile GetTile(int index)
    {
        if (index < 0 || index > 2 || m_Board == null)
            return null;
        
        int tileID = m_Tiles[index];
        return tileID == -1 ? null : m_Board.Tiles[tileID];
    }

    public Edge GetEdge(int index)
    {
        if (index < 0 || index > 2 || m_Board == null)
            return null;
        
        int edgeID = m_Edges[index];        
        return edgeID == -1 ? null : m_Board.Edges[edgeID];
    }

    public void SetTileID(int index, int tileID)
    {
        if (index < 0 || index > 2 || tileID < -1 || tileID > 18)
            return;
        
        m_Tiles[index] = tileID;
    }

    public void SetEdgeID(int index, int edgeID)
    {
        if (index < 0 || index > 2 || edgeID < -1 || edgeID > 71)
            return;
        
        m_Edges[index] = edgeID;
    }

    /// <summary>
    /// ID equal to position in node array
    /// </summary>
    public int ID { get; private set; }

    /// <summary>
    /// Worldspace position for rendering
    /// </summary>
    public Vector2 Position;

    /// <summary>
    /// Current owner, null if un-owned
    /// </summary>
    public int OwnerID;

    public bool IsCity;

    public bool Selected;

    /// <summary>
    /// Connect port type
    /// </summary>
    public Port.TradeType PortType;

    /// <summary>
    /// Connected edges
    /// </summary>
    private readonly int[] m_Edges = new int[] { -1, -1, -1 };

    /// <summary>
    /// Adjacent tiles
    /// </summary>
    private readonly int[] m_Tiles = new int[] { -1, -1, -1 };

    private readonly Board m_Board;

    /// <summary>
    /// Similar to selected, set to active if collision is detected
    /// Regardless of mouse-press
    /// </summary>
    private bool m_Hovered;

    /// <summary>
    /// Radius of drawn circle
    /// </summary>
    private static readonly float RADIUS = 5f;

    /// <summary>
    /// Get neighbouring node
    /// </summary>
    /// <param name="index">index of edge to traverse</param>
    /// <returns>null if nonexistant edge</returns>
    public Node GetNeighbourNode(int index)
    {
        Edge edge = GetEdge(index);

        if (edge == null)
            return null;

        Node node = edge.GetNode(0);
        if (node == this)
            node = edge.GetNode(1);

        return node;
    }

    /// <summary>
    /// Fetch 2nd order node
    /// </summary>
    /// <param name="x">path from root</param>
    /// <param name="y">path from branch</param>
    public Node GetSecondOrderNode(int x, int y)
    {
        Node branchNode = GetNeighbourNode(x);

        if (branchNode == null)
            return null;

        Node leafNode = branchNode.GetNeighbourNode(y);

        if (leafNode == this)
            return null;

        return leafNode;
    }

    /// <summary>
    /// Is this a valid position to build a settlement
    /// </summary>
    public bool IsAvailable(int playerID = -1)
    {
        if (OwnerID != -1)
            return false;

        bool available = false;
        for (int i = 0; i < 3; i++)
        {
            Edge edge = GetEdge(i);
            Node node = GetNeighbourNode(i);

            if (edge == null || node == null)
                continue;
            
            else if (node.OwnerID != -1)
                return false;

            available |= edge.OwnerID == playerID;
        }

        return available;
    }

    public bool TestCollision(Vector2 point, Vector2 offset, float scale)
    {
        m_Hovered = Vector2.DistanceSquared((Position * scale) + offset, point) < RADIUS * RADIUS;
        return m_Hovered;
    }

    public void Draw(ShapeBatcher shapeBatcher, Vector2 offset, float scale)
    {
        float radius = RADIUS + (m_Hovered || Selected ? 1f : 0f);
        Color colour = Player.GetColourFromID(OwnerID);
        const int VERTEX_NUM = 10;

        Vector2 drawPos = (Position * scale) + offset;

        if (IsCity)
            shapeBatcher.DrawFilledCircle(drawPos, radius, VERTEX_NUM, colour);

        else
            shapeBatcher.DrawCircle(drawPos, radius, VERTEX_NUM, 1f, colour);

        m_Hovered = false;
    }
}