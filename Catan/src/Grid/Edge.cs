using System.Security.AccessControl;

namespace Grid.Hexagonal;

public class Edge
{
    private Key m_Position;

    public Edge(Key position)
    {
        m_Position = position;
    }

    public Key GetPosition() {
        return m_Position;
    }

    public enum Side {
        W,
        NW,
        NE,
        E,
        SE,
        SW
    }

    public struct Key {
        public Axial Position;
        public Side Side;

        public void Align()
        {
            switch (Side)
            {
            case Side.E:
                Position.q++;
                Side = Side.W;
                return;

            case Side.SE:
                Position.r--;
                Side = Side.NW;
                return;

            case Side.SW:
                Position.r--;
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