using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Catan;

/// <summary>
/// Single board node
/// </summary>
class Node
{
    public Node(int id)
    {
        Position = Vector2.Zero;
        Owner = null;
        IsCity = false;
        m_Hovered = false;
        Selected = false;
        PortType = Port.TradeType.Empty;
        ID = id;
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
    public Player Owner;

    public bool IsCity;

    public bool Selected;

    /// <summary>
    /// Connect port type
    /// </summary>
    public Port.TradeType PortType;

    /// <summary>
    /// Connected edges
    /// </summary>
    public Edge[] Edges = new Edge[] {null, null, null};

    /// <summary>
    /// Adjacent tiles
    /// </summary>
    public Tile[] Tiles = new Tile[3];

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
        // Out of range
        if (index < 0 || index >= 3)
            return null;
        
        else if (Edges[index] == null)
            return null;
        
        int n = 0;
        if (Edges[index].Nodes[n] == this)
            n++;
        
        return Edges[index].Nodes[n];
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
            
            else if (edge.Owner == player || player == null)
                available = true;
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
        Color colour = Owner != null ? Owner.Colour : Color.Black;
        const int VERTEX_NUM = 10;

        Vector2 drawPos = (Position * scale) + offset;

        if (IsCity)
            shapeBatcher.DrawFilledCircle(drawPos, radius, VERTEX_NUM, colour);
        
        else
            shapeBatcher.DrawCircle(drawPos, radius, VERTEX_NUM, 1f, colour);
        
        m_Hovered = false;
    }
}