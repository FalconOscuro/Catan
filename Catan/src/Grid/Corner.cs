using Catan;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Grid.Hexagonal;
using static Utility;

public class Corner : Tileable
{
    private Key m_Position;

    public Corner(Key position)
    {
        m_Position = position;
    }

    public Key GetPosition() {
        return m_Position;
    }

    public bool DrawFilled = true;

    public override void Draw(float shapeScale, float scale, float rotation, Vector2 translation, ShapeBatcher shapeBatcher)
    {
        Vector2 localPos = new(){
            X = shapeScale * INVERSE_SQRT_3 * m_Position.Position.q * 1.5f,
            Y = shapeScale * (m_Position.Position.r + m_Position.Position.q * 0.5f)
        };

        Vector2 offset = new(){
            X = -shapeScale * INVERSE_SQRT_3
        };

        if (m_Position.Side == Side.NW)
        {
            offset.Y += shapeScale;
            offset *= 0.5f;
        }

        localPos += offset;

        Vector2 pos = localPos.Rotate(rotation) + translation;

        if (DrawFilled)
            shapeBatcher.DrawFilledCircle(pos, shapeScale * scale, 10, Color.Green);

        else
            shapeBatcher.DrawCircle(pos, shapeScale * scale, 10, scale, Color.Green);
    }

    public enum Side {
        W,
        NW,
        NE,
        E,
        SE,
        SW
    }

    public struct Key
    {
        public Axial Position;
        public Side Side;

        public void Align()
        {
            switch (Side)
            {
            case Side.NE:
                Position.q++;
                Side = Side.W;
                break;

            case Side.E:
                Position.q++;
                Position.r--;
                Side = Side.NW;
                break;

            case Side.SE:
                Position.q++;
                Position.r--;
                Side = Side.W;
                break;

            case Side.SW:
                Position.r--;
                Side = Side.NW;
                break;
            }
        }
    }
}

public abstract class CornerFactory {
    public abstract Corner CreateCorner(Corner.Key key);
}

public class DefaultCornerFactory : CornerFactory{
    public override Corner CreateCorner(Corner.Key key) {
        return new Corner(key);
    }
}