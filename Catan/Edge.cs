using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Catan;

class Edge
{
    public Edge(int id, Board board)
    {
        ID = id;

        Start = Vector2.Zero;
        End = Vector2.Zero;
        OwnerID = -1;

        m_Hovered = false;
        Selected = false;

        m_Board = board;
    }

    public Edge(Edge edge, Board board)
    {
        ID = edge.ID;

        Start = edge.Start;
        End = edge.End;
        OwnerID = edge.OwnerID;

        m_Hovered = false;
        Selected = false;

        Array.Copy(edge.m_Nodes, m_Nodes, 2);

        m_Board = board;
    }

    public void CalculatePosition()
    {
        Vector2 pos1;
        {
            Node node = GetNode(0);
            if (node == null)
                return;

            pos1 = node.Position;
        }
        Vector2 pos2;
        {
            Node node = GetNode(1);
            if (node == null)
                return;

            pos2 = node.Position;
        }

        Vector2 centre = (pos1 + pos2) / 2;
        Start = ((pos1 - centre) * .8f) + centre;
        End = ((pos2 - centre) * .8f) + centre;
    }

    public Node GetNode(int index)
    {
        if (index < 0 || index > 1 || m_Board == null)
            return null;
        
        int nodeID = m_Nodes[index];
        return nodeID == -1 ? null : m_Board.Nodes[nodeID];
    }

    public void SetNodeID(int index, int nodeID)
    {
        if (index < 0 || index > 1 || nodeID < -1 || nodeID > 53)
            return;
        
        m_Nodes[index] = nodeID;
    }

    public int ID { get; private set; }

    // Connections ordered N->S & E->W
    public Vector2 Start;
    public Vector2 End;

    public int OwnerID;

    private readonly int[] m_Nodes = new int[]{-1, -1};

    private readonly Board m_Board;

    public bool Selected;

    private bool m_Hovered;

    public bool IsAvailable()
    {
        return OwnerID == -1;
    }

    public bool IsAvailable(int playerID)
    {
        if (!IsAvailable())
            return false;

        for (int i = 0; i < 2; i++)
        {
            Node node = GetNode(i);

            if (node == null)
                continue;

            else if (node.OwnerID == playerID)
                return true;
                
            else if (node.OwnerID != -1)
                continue;

            for (int j = 0; j < 3; j++)
            {
                Edge edge = node.GetEdge(j);

                if (edge != null && edge != this)
                    if (edge.OwnerID == playerID)
                        return true;
            }
        }

        return false;
    }

    public bool TestCollision(Vector2 point, Vector2 offset, float scale)
    {
        Vector2 v1 = (End - Start) * scale;
        Vector2 v2 = point - ((Start * scale) + offset);
        Vector2 v3 = Vector2.Normalize(v1);

        float d  = Vector2.Dot(v3, v2);

        if (d < 0 || d > v1.Length())
            return false;
            
        m_Hovered = ((v3 * d) - v2).Length() <= LINE_WIDTH;

        return m_Hovered;
    }

    public void Draw(ShapeBatcher shapeBatcher, Vector2 offset, float scale)
    {
        shapeBatcher.DrawLine(
            (Start * scale) + offset, (End * scale) + offset, 
            LINE_WIDTH + ((m_Hovered || Selected) ? LINE_WIDTH * 2.5f : 0f), 
            Player.GetColourFromID(OwnerID));
        m_Hovered = false;
    }

    private static readonly float LINE_WIDTH = 1f;
}