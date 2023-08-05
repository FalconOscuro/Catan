using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Catan;

struct Port
{ 
    public Port()
    {
        Type = TradeType.Empty;
        Nodes = new Node[2];
    }

    public Port(Node a, Node b, TradeType type)
    {
        Type = type;
        Nodes = new Node[2]{a, b};

        a.PortType = type;
        b.PortType = type;
    }

    public readonly void Draw(ShapeBatcher shapeBatcher, Vector2 offset, float scale)
    {
        Vector2 pos = ((Nodes[0].Position + Nodes[1].Position) * scale / 2) - (Nodes[0].Position * scale);
        Vector2 portEnd = new(pos.Y, -pos.X);
        
        pos += (Nodes[0].Position * scale) + offset + portEnd;

        shapeBatcher.DrawFilledCircle(pos, 4f, 10, GetPortColour(Type));

        for (int i = 0; i < 2; i++)
        {
            Vector2 start = (Nodes[i].Position * scale) + offset;

            shapeBatcher.DrawLine(start, (start + pos) / 2, 2f, Color.Brown);
        }
    }

    public static Color GetPortColour(TradeType type)
    {
        if (type == TradeType.Versatile)
            return Color.Gray;
        
        return Resources.GetResourceColour((Resources.Type)type);
    }

    public TradeType Type;

    public Node[] Nodes;

   public enum TradeType 
   {
    Empty = -1,
    Lumber,
    Brick,
    Grain,
    Wool,
    Ore,
    Versatile
   }
}