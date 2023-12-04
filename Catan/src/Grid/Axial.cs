using Microsoft.Xna.Framework;

namespace Grid.Hexagonal;
using static Utility;

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

    public Vector2 GetRealPos() {
        return new Vector2(){
            X = INVERSE_SQRT_3 * q * 1.5f,
            Y = r + q * 0.5f
        };
    }
}