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

public abstract class EdgeFactory {
    public abstract Edge CreateEdge(Edge.Key key);
}

public class DefaultEdgeFactory : EdgeFactory {
    public override Edge CreateEdge(Edge.Key key) {
        return new Edge(key);
    }
}