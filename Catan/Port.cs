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

    public void Draw(ShapeBatcher shapeBatcher)
    {
        Vector2 pos = ((Nodes[0].Position + Nodes[1].Position) / 2) - Nodes[0].Position;
        Vector2 offset = new Vector2(pos.Y, -pos.X);
        
        pos += Nodes[0].Position + offset;

        shapeBatcher.DrawFilledCircle(pos, 4f, 10, GetPortColour(Type));

        shapeBatcher.DrawLine(Nodes[0].Position, (Nodes[0].Position + pos) / 2, 2f, Color.Brown);
        shapeBatcher.DrawLine(Nodes[1].Position, (Nodes[1].Position + pos) / 2, 2f, Color.Brown);
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