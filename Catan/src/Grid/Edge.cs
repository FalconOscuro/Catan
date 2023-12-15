using System;
using Microsoft.Xna.Framework;

using Utility;
using Utility.Graphics;

namespace Grid.Hexagonal;

public class Edge
{
    public Edge()
    {}

    public virtual object Clone() {
        return this.MemberwiseClone();
    }

    public virtual void Draw(Transform transform, Canvas canvas)
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

        public readonly bool Aligned() {
            return Side < Side.SE;
        }

        public readonly Key Align()
        {
            Key aligned = new(){
                Position = Position,
                Side = Side
            };

            // Already aligned
            if (Aligned())
                return aligned;

            aligned.Side -= 3;

            Axial offset = new(){
                Q = -((int)aligned.Side - 1),
                R = aligned.Side < Side.NE ? -1 : 0
            };

            aligned.Position += offset;

            return aligned;
        }

        public readonly Vector2 GetRealPos() {
            Vector2 offset = new(0, 0.5f);

            return Position.GetRealPos() + offset.Rotate(GetRotation());
        }

        public readonly float GetRotation() {
            return -((int)Align().Side - 1) * MathF.PI / 3f;
        }
    }
}