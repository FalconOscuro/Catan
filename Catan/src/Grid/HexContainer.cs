namespace Grid.Hexagonal;

internal class HexContainer
{
    public HexContainer(){
        Hex = null;

        W_Edge = null;
        NW_Edge = null;
        NE_Edge = null;

        N_Corner = null;
        S_Corner = null;
    }

    public Hex Hex;

    public Edge W_Edge;
    public Edge NW_Edge;
    public Edge NE_Edge;

    public Corner N_Corner;
    public Corner S_Corner;

    public Edge GetEdge(Edge.Side side)
    {
        return side switch
        {
            Edge.Side.W => W_Edge,
            Edge.Side.NW => NW_Edge,
            Edge.Side.NE => NE_Edge,
            _ => null,
        };
    }

    public void SetEdge(Edge.Side side, Edge edge)
    {
        switch (side)
        {
        case Edge.Side.W:
            W_Edge = edge;
            break;

        case Edge.Side.NW:
            NW_Edge = edge;
            break;

        case Edge.Side.NE:
            NE_Edge = edge;
            break;
        }
    }

    public Corner GetCorner(Corner.Side side)
    {
        return side switch
        {
            Corner.Side.N => N_Corner,
            Corner.Side.S => S_Corner,
            _ => null,
        };
    }

    public void SetCorner(Corner.Side side, Corner corner)
    {
        switch (side)
        {
        case Corner.Side.N:
            N_Corner = corner;
            break;

        case Corner.Side.S:
            S_Corner = corner;
            break;
        }
    }
}