using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;

namespace Grid.Hexagonal;

internal class Utility
{
    public static readonly float INVERSE_SQRT_3 = 0.577350269f;
    public static readonly float HALF_INVERSE_SQRT_3 = INVERSE_SQRT_3 * 0.5f;

    /// <summary>
    /// Local vertex positions for a centered hexagon where height = 1
    /// </summary>
    public static readonly ReadOnlyCollection<Vector2> UNSCALED_HEX_VERTICES = new(new Vector2[]{
        new(0f, 0f),
        new(-INVERSE_SQRT_3, 0f),
        new(-HALF_INVERSE_SQRT_3, 0.5f),
        new(HALF_INVERSE_SQRT_3, 0.5f),
        new(INVERSE_SQRT_3, 0f),
        new(HALF_INVERSE_SQRT_3, -0.5f),
        new(-HALF_INVERSE_SQRT_3, -0.5f),
    });

    public static readonly ReadOnlyCollection<int> HEX_INDICES = new(new int[]{
        0, 1, 2,
        0, 2, 3,
        0, 3, 4,
        0, 4, 5,
        0, 5, 6,
        0, 6, 1
    });
}