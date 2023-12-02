using Microsoft.Xna.Framework;

namespace Catan;

struct Node
{
    public Vector2 LocalPosition;

    public readonly void Draw(Vector2 offset, float scale)
    {
        Catan.s_ShapeBatcher.DrawCircle((LocalPosition * scale) + offset, scale * .1f, 6, 1f, Color.Black);
    }
}