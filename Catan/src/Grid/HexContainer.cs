using Catan;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Grid.Hexagonal;

internal class HexContainer
{
    public HexContainer()
    {}

    public Hex Hex = null;

    public readonly Edge[] Edges = new Edge[3];

    public readonly Corner[] Corners = new Corner[2];

    public void Draw(float scale, float rotation, Vector2 translation, ShapeBatcher shapeBatcher)
    {
        Hex?.Draw(scale, 0.9f, rotation, translation, shapeBatcher);

        foreach (Corner corner in Corners)
            corner?.Draw(scale, 0.05f, rotation, translation, shapeBatcher);

        foreach (Edge edge in Edges)
            edge?.Draw(scale, 0.5f, rotation, translation, shapeBatcher);
    }
}