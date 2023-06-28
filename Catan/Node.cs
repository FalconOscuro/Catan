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

    public void Distribute(Resources.Type resource)
    {
        if (Owner != null)
            Owner.GiveResource(resource, IsCity ? 2 : 1);
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