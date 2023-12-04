using System.Numerics;
using Microsoft.Xna.Framework.Graphics;

namespace Catan;

public struct Canvas {
    public ShapeBatcher shapeBatcher;
    public SpriteBatch spriteBatch;

    public Vector2 ScreenSize;

    public void Begin()
    {
        shapeBatcher.Begin();
        spriteBatch.Begin();
    }

    public void End()
    {
        shapeBatcher.End();
        spriteBatch.End();
    }
}