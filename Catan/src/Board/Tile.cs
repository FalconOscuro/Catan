using System;
using Microsoft.Xna.Framework;

namespace Catan;

struct Tile
{
    public Vector2 Position;

    public readonly void Draw(Vector2 offset, float scale)
    {
        Catan.s_ShapeBatcher.DrawFilledCircle((Position * scale) + offset, scale * SCALE_FILL_PERCENT, 6, Color.Red);
    }

    private static readonly float SCALE_FILL_PERCENT = 0.47f;
}