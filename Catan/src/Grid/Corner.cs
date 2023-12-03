namespace Grid.Hexagonal;

public class Corner
{
    private Key m_Position;

    public Corner(Key position)
    {
        m_Position = position;
    }

    public Key GetPosition() {
        return m_Position;
    }

    public enum Side {
        N,
        NE,
        SE,
        S,
        SW,
        NW
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
                Position.r++;
                Side = Side.S;
                break;

            case Side.SE:
                Position.q++;
                Position.r--;
                Side = Side.N;
                break;

            case Side.SW:
                Position.q--;
                Position.r--;
                Side = Side.N;
                break;

            case Side.NW:
                Position.q--;
                Position.r++;
                Side = Side.S;
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