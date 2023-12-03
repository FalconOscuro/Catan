namespace Grid.Hexagonal;

public struct Axial
{
    public int q;
    public int r;

    public static bool operator==(Axial a, Axial b) {
        return a.q == b.q && a.q == b.q;
    }

    public static bool operator!=(Axial a, Axial b) {
        return a.q != b.q || a.r != b.r;
    }
}