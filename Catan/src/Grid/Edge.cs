using Microsoft.Xna.Framework;

using Utility;
using Utility.Graphics;

namespace Grid.Hexagonal;

public class Edge : Tileable
{
    public Edge()
    {}

    public virtual object Clone() {
        return this.MemberwiseClone();
    }

    public override void Draw(Transform transform, Canvas canvas)
    {
        Vector2 start = new(){
            X = -1
        };

        Vector2 end = new(){
            X = 1
        };

        start = transform.Apply(start);
        end = transform.Apply(end);

        canvas.shapeBatcher.DrawLine(start, end, transform.Scale * 0.1f, Color.Black);
    }

    public enum Side {
        NW,
        N,
        NE,
        SE,
        S,
        SW
    }

    public struct Key {
        public Axial Position;
        public Side Side;

        public void Align()
        {
            switch (Side)
            {
            case Side.SE:
                Position.q++;
                Position.r--;
                Side = Side.NW;
                return;

            case Side.S:
                Position.r--;
                Side = Side.N;
                return;

            case Side.SW:
                Position.q--;
                Side = Side.NE;
                return;
            }
        }
    }
}