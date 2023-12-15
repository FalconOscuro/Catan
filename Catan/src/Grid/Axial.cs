using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;

namespace Grid.Hexagonal;
using static Utility;

/// <summary>
/// Interger position in 2D axial space
/// </summary>
/// <remarks>
/// Used for hexagonal grids
/// </remarks>
public struct Axial
{
    public Axial(int q, int r)
    {
        Q = q;
        R = r;
    }


    public int Q;
    public int R;

    public override readonly bool Equals([NotNullWhen(true)] object obj)
    {
        Axial? axial = obj as Axial?;

        if (!axial.HasValue)
            return false;
        
        return axial.Value == this;
    }

    public override readonly int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static bool operator==(Axial a, Axial b) {
        return a.Q == b.Q && a.Q == b.Q;
    }

    public static bool operator!=(Axial a, Axial b) {
        return a.Q != b.Q || a.R != b.R;
    }

    public static Axial operator+(Axial a, Axial b) {
        return new(a.Q + b.Q, a.R + b.R);
    }

    public static Axial operator-(Axial a, Axial b) {
        return new(a.Q - b.Q, a.R - b.R);
    }

    public static Axial operator*(Axial a, int s) {
        return new(a.Q * s, a.R * s);
    }

    public static Axial operator/(Axial a, int s) {
        return new(a.Q / s, a.R / s);
    }

    /// <summary>
    /// Convert axial space to real-space
    /// </summary>
    public Vector2 GetRealPos() {
        return new Vector2(){
            X = INVERSE_SQRT_3 * Q * 1.5f,
            Y = R + Q * 0.5f
        };
    }
}