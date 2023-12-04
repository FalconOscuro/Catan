using System;
using System.Security.AccessControl;
using Catan;
using Microsoft.Xna.Framework;

namespace Grid.Hexagonal;
using static Utility;

public class Edge : Tileable
{
    private Key m_Position;

    public Edge(Key position)
    {
        m_Position = position;
    }

    public Key GetPosition() {
        return m_Position;
    }

    public override void Draw(float shapeScale, float scale, float rotation, Vector2 translation, ShapeBatcher shapeBatcher)
    {
        Vector2 localPos = new(){
            X = shapeScale * INVERSE_SQRT_3 * m_Position.Position.q * 1.5f,
            Y = shapeScale * (m_Position.Position.r + m_Position.Position.q * 0.5f)
        };

        Vector2 start = new(){
            X = shapeScale * scale * INVERSE_SQRT_3 * 0.5f
        };

        Vector2 end = new(){
            X = -start.X
        };

        Vector2 offset = new(){
            Y = shapeScale * 0.5f
        };

        start += offset;
        end += offset;

        float edgeRot = -(((int)m_Position.Side) - 1) * MathF.PI / 3f;
        
        start = start.Rotate(edgeRot);
        end = end.Rotate(edgeRot);

        start += localPos;
        end += localPos;

        start = start.Rotate(rotation) + translation;
        end = end.Rotate(rotation) + translation;

        shapeBatcher.DrawLine(start, end, shapeScale * scale, Color.Yellow);
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

public abstract class EdgeFactory {
    public abstract Edge CreateEdge(Edge.Key key);
}

public class DefaultEdgeFactory : EdgeFactory {
    public override Edge CreateEdge(Edge.Key key) {
        return new Edge(key);
    }
}