using Catan;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Grid;

public abstract class Tileable
{
    public abstract void Draw(float shapeScale, float scale, float rotation, Vector2 translation, ShapeBatcher shapeBatcher);
}