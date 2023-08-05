using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Catan;

class Edge
{
    public Edge()
    {
        Start = Vector2.Zero;
        End = Vector2.Zero;
        Owner = null;

        m_Hovered = false;
        Selected = false;
    }

    public void CalculatePosition()
    {
        Vector2 centre = (Nodes[0].Position + Nodes[1].Position) / 2;

        Start = ((Nodes[0].Position - centre) * .8f) + centre;
        End = ((Nodes[1].Position - centre) * .8f) + centre;
    }

    // Connections ordered N->S & E->W
    public Vector2 Start;
    public Vector2 End;

    public Player Owner;

    public Node[] Nodes = new Node[2];

    public bool Selected;

    private bool m_Hovered;

    public bool IsAvailable()
    {
        return Owner == null;
    }

    public bool IsAvailable(Player player)
    {
        if (!IsAvailable())
            return false;

        foreach (Node node in Nodes)
        {
            if (node.Owner == player)
                return true;
                
            if (node.Owner != null)
                continue;

            foreach (Edge edge in node.Edges)
                if (edge != null && edge != this)
                    if (edge.Owner == player)
                        return true;
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
        shapeBatcher.DrawLine((Start * scale) + offset, (End * scale) + offset, LINE_WIDTH + ((m_Hovered || Selected) ? LINE_WIDTH * 2.5f : 0f), Owner != null ? Owner.Colour : Color.Black);
        m_Hovered = false;
    }

    private static readonly float LINE_WIDTH = 1f;
}