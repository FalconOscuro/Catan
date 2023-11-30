using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Type = Catan.Resources.Type;

namespace Catan;

/// <summary>
/// Hexagonal tile piece
/// </summary>
struct Tile
{
    /// <summary>
    /// Position relative to other tiles
    /// </summary>
    public Vector2 LocalPosition;

    public Type Resource;

    public readonly void Draw(Vector2 offset, float scale)
    {
        Catan.s_ShapeBatcher.DrawFilledCircle((LocalPosition * scale) + offset, scale * .5f, 6, Resources.GetColour(Resource));
    }

    /// <summary>
    /// Readonly array for default board setup
    /// </summary>
    public static readonly ReadOnlyCollection<Type> DEFAULT_RESOURCE_SPREAD = new(new Type[]
        {Type.Empty, Type.Wool, Type.Lumber, Type.Grain, Type.Ore, Type.Lumber, Type.Brick,
            Type.Lumber, Type.Brick, Type.Ore, Type.Wool, Type.Wool, Type.Grain, Type.Brick, 
                Type.Lumber, Type.Grain, Type.Grain, Type.Ore, Type.Wool}
    );
}