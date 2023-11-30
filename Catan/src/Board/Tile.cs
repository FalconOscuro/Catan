using System.Collections.ObjectModel;
using ImGuiNET;
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

    public int Value;

    public bool Robber;

    public readonly void Draw(Vector2 offset, float scale)
    {
        Vector2 globalPosition = (LocalPosition * scale) + offset;

        float hexScale = scale * HEX_SCALE;
        float centerScale = hexScale * CENTER_SCALE;

        Catan.s_ShapeBatcher.DrawFilledCircle(globalPosition, hexScale, 6, Resources.GetColour(Resource));
        Catan.s_ShapeBatcher.DrawFilledCircle(globalPosition, centerScale, 10, Robber ? Color.Black : Color.Orange);

        if (Resource != Type.Empty || Robber)
        {
            string valueString = Value.ToString();
            Vector2 textPos = (new Vector2(LocalPosition.X, -LocalPosition.Y) * scale) + offset - (Catan.s_Font.MeasureString(valueString) / 2);

            Catan.s_SpriteBatch.DrawString(Catan.s_Font, valueString, textPos, Color.Black);
        }
    }

    private static readonly float HEX_SCALE = 0.51f;
    private static readonly float CENTER_SCALE = 0.25f;

    /// <summary>
    /// Default resource distribution
    /// </summary>
    public static readonly ReadOnlyCollection<Type> DEFAULT_RESOURCE_SPREAD = new(new Type[]
        {Type.Empty, Type.Wool, Type.Lumber, Type.Grain, Type.Ore, Type.Lumber, Type.Brick,
            Type.Lumber, Type.Brick, Type.Ore, Type.Wool, Type.Wool, Type.Grain, Type.Brick, 
                Type.Lumber, Type.Grain, Type.Grain, Type.Ore, Type.Wool}
    );

    /// <summary>
    /// Default value distribution, Desert tile is omitted as it should have no value
    /// </summary>
    public static readonly ReadOnlyCollection<int> DEFAULT_NUMBER_SPREAD = new(new int[]
        {4, 3, 4, 3, 11, 6, 9, 10, 8, 5, 11, 6, 5, 8, 9, 12, 10, 2}
    );
}