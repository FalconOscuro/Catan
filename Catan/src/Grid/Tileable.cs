using Catan;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Grid;

public abstract class Tileable
{
    public abstract void Draw(Transform transform, Canvas canvas);
}