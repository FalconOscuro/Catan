using Microsoft.Xna.Framework;

using Utility;
using Utility.Graphics;

namespace Grid.Hexagonal;

public class Corner : Tileable
{
    public Corner()
    {}

    public bool DrawFilled = true;

    public virtual object Clone() {
        return this.MemberwiseClone();
    }

    public override void Draw(Transform transform, Canvas canvas)
    {
        if (DrawFilled)
            canvas.shapeBatcher.DrawFilledCircle(transform.Translation, transform.Scale, 10, Color.Black);

        else
            canvas.shapeBatcher.DrawCircle(transform.Translation, transform.Scale, 10, transform.Scale * 0.1f, Color.Black);
    }

    public enum Side {
        W,
        E,
        NW,
        NE,
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
            case Side.NW:
                Position.q--;
                Position.r++;
                Side = Side.E;
                break;

            case Side.NE:
                Position.q++;
                Side = Side.W;
                break;

            case Side.SE:
                Position.q++;
                Position.r--;
                Side = Side.W;
                break;

            case Side.SW:
                Position.q--;
                Side = Side.E;
                break;
            }
        }
    }
}