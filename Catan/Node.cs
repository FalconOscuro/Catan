using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Catan;

class Node
{
    public Node()
    {
        Position = Vector2.Zero;
        Owner = null;
        IsCity = false;
        m_Hovered = false;
        Selected = false;
    }

    public Vector2 Position;

    public Player Owner;

    public bool IsCity;

    public bool Selected;

    public Edge[] Edges = new Edge[] {null, null, null};

    public Tile[] Tiles = new Tile[3];

    private bool m_Hovered;

    private static readonly float RADIUS = 5f;

    public List<Edge> StartRecurse(Player player)
    {
        if (Owner != player && Owner != null)
            return new List<Edge>();

        List<Edge>[] paths = new List<Edge>[3];

        for (int i = 0; i < 3; i++)
        {
            if (Edges[i] != null)
                paths[i] = Edges[i].Recurse(player, this);

            else
                paths[i] = new List<Edge>();
        }
        
        List<Edge> longest = paths[0];

        for (int i = 0; i < 3; i++)
        {
            if (paths[i].Count > longest.Count)
                longest = paths[i];

            for (int j = i + 1; j < 3; j++)
            {
                bool overlap = false;
                for (int k = 0; k < paths[i].Count && !overlap; k++)
                {
                    if (paths[j].Contains(paths[i][k]))
                        overlap = true;
                }

                if (overlap)
                    continue;
                
                List<Edge> path = new List<Edge>(paths[i]);
                path.AddRange(paths[j]);

                if (path.Count > longest.Count)
                    longest = path;
            }
        }
        return longest;
    }

    public List<Edge> Recurse(Player player)
    {
        if (Owner != player && Owner != null)
            return new List<Edge>();
        
        List<Edge>[] paths = new List<Edge>[3];

        for (int i = 0; i < 3; i++)
        {
            if (Edges[i] != null)
                paths[i] = Edges[i].Recurse(player, this);

            else
                paths[i] = new List<Edge>();   
        }

        List<Edge> longest = paths[0];
        for (int i = 1; i < 3; i++)
            if (paths[i].Count > longest.Count)
                longest = paths[i];

        return longest;
    }

    public bool IsAvailable(Player player = null)
    {
        if (Owner != null)
            return false;

        bool available = false;
        foreach (Edge edge in Edges)
        {
            if (edge == null)
                continue;
                
            int n = 0;
            // Avoid checking self
            if (edge.Nodes[n] == this)
                n++;
                
            if (edge.Nodes[n].Owner != null)
                return false;
            
            else if (edge.Owner == player)
                available = true;
        }

        return available;
    }

    public bool TestCollision(Vector2 point)
    {
        m_Hovered = Vector2.DistanceSquared(Position, point) < RADIUS * RADIUS;
        return m_Hovered;
    }

    public void Draw(ShapeBatcher shapeBatcher)
    {
        float radius = RADIUS + (m_Hovered || Selected ? 1f : 0f);
        Color colour = Owner != null ? Owner.Colour : Color.Black;
        const int VERTEX_NUM = 10;

        if (IsCity)
            shapeBatcher.DrawFilledCircle(Position, radius, VERTEX_NUM, colour);
        
        else
            shapeBatcher.DrawCircle(Position, radius, VERTEX_NUM, 1f, colour);
        
        m_Hovered = false;
    }
}