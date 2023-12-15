using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Utility;
using Utility.Graphics;

namespace Grid.Hexagonal;
using static Utility;

public class Hex
{
    public Hex()
    {}

    public Color Colour = Color.Black;

    public virtual object Clone() {
        return this.MemberwiseClone();
    }

    public virtual void Draw(Transform transform, Canvas canvas)
    {
        int[] indices = new int[18];
        HEX_INDICES.CopyTo(indices, 0);

        VertexPositionColor[] vertices = new VertexPositionColor[7];

        for (int i = 0; i < 7; i++)
            vertices[i] = new VertexPositionColor(transform.Apply(UNSCALED_HEX_VERTICES[i]).ToVector3(), Colour);

        canvas.shapeBatcher.DrawPrimitives(vertices, indices);
    }
}