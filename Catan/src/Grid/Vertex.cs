using Microsoft.Xna.Framework;

using Utility;
using Utility.Graphics;

namespace Grid.Hexagonal;
using static Utility;

public class Vertex
{
    public Vertex()
    {}

    public bool DrawFilled = true;

    public virtual object Clone() {
        return this.MemberwiseClone();
    }

    public virtual void Draw(Transform transform, Canvas canvas)
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

        public readonly bool Aligned() {
            return Side < Side.NW;
        }

        public readonly Key Align()
        {
            Key aligned = new(){
                Position = Position,
                Side = Side
            };

            if (Aligned())
                return aligned;

            // Could be cleaned up
            switch (Side)
            {
            case Side.NW:
                aligned.Position.Q--;
                aligned.Position.R++;
                aligned.Side = Side.E;
                break;

            case Side.NE:
                aligned.Position.Q++;
                aligned.Side = Side.W;
                break;

            case Side.SE:
                aligned.Position.Q++;
                aligned.Position.R--;
                aligned.Side = Side.W;
                break;

            case Side.SW:
                aligned.Position.Q--;
                aligned.Side = Side.E;
                break;
            }

            return aligned;
        }

        public readonly Vector2 GetRealPos() {
            return Position.GetRealPos() + 
                new Vector2(INVERSE_SQRT_3 * (Align().Side == Side.W ? -1 : 1), 0);
        }

        public readonly Key[] GetAdjacentEdges()
        {
            Key aligned = Align();

            int mult;
            Side adjSide;

            if (aligned.Side == Side.W)
            {
                mult = -1;
                adjSide = Side.E;
            }

            else
            {
                mult = 1;
                adjSide = Side.W;
            }

            Key[] keys = new Key[3];

            keys[0] = new(){
                Position = aligned.Position + (new Axial(2, -1) * mult),
                Side = adjSide
            };

            keys[1] = new(){
                Position = aligned.Position + (new Axial(1, -1) * mult),
                Side = adjSide
            };

            keys[2] = new(){
                Position = aligned.Position + (new Axial(1, 0) * mult),
                Side = adjSide
            };

            return keys;
        }
    }
}