using Catan;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Grid.Hexagonal;
using static Utility;

public class Hex : Tileable
{
    public Hex(Axial axial)
    {
        Position = axial;
    }

    public Color Colour = Color.Black;

    public Axial Position { get; private set; }

    public override void Draw(Transform transform, Canvas canvas)
    {
        int[] indices = new int[18];
        HEX_INDICES.CopyTo(indices, 0);

        VertexPositionColor[] vertices = new VertexPositionColor[7];

        for (int i = 0; i < 7; i++)
            vertices[i] = new VertexPositionColor(transform.Apply(UNSCALED_HEX_VERTICES[i]).ToVector3(), Colour);

        canvas.shapeBatcher.DrawPrimitives(vertices, indices);
    }
}

public abstract class HexFactory {
    public abstract Hex CreateHex(Axial pos);
}

public class DefaultHexFactory : HexFactory {
    public override Hex CreateHex(Axial pos) {
        return new Hex(pos);
    }
}