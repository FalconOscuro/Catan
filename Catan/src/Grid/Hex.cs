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

    public override void Draw(float shapeScale, float scale, float rotation, Vector2 translation, ShapeBatcher shapeBatcher)
    {
        int[] indices = new int[18];
        HEX_INDICES.CopyTo(indices, 0);

        VertexPositionColor[] vertices = new VertexPositionColor[7];

        Vector2 localPos = new(){
            X = shapeScale * INVERSE_SQRT_3 * Position.q * 1.5f,
            Y = shapeScale * (Position.r + Position.q * 0.5f)
        };

        Vector3 pos = localPos.Rotate(rotation).ToVector3();

        pos.X += translation.X;
        pos.Y += translation.Y;

        for (int i = 0; i < 7; i++)
            vertices[i] = new VertexPositionColor((UNSCALED_HEX_VERTICES[i].Rotate(rotation).ToVector3() * scale * shapeScale) + pos, Colour);

        shapeBatcher.DrawPrimitives(vertices, indices);
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