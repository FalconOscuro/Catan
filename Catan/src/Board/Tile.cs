using Microsoft.Xna.Framework;
using Grid.Hexagonal;
using Utility;
using Utility.Graphics;

using Type = Catan.Resources.Type;

namespace Catan;

/// <summary>
/// Gameplay board tile
/// </summary>
public class Tile : Hex 
{
    /// <summary>
    /// Protected <see cref="Resource"/> field.
    /// </summary>
    /// <remarks>
    /// Should only be modified via <see cref="Resource"/> setter.
    /// </remarks>
    private Type m_Resource;

    /// <summary>
    /// The resource type for this tile.
    /// </summary>
    /// <value> Determines the <see cref="Colour"/></value>
    public Type Resource { 
        get { return m_Resource; }
        set { m_Resource = value; Colour = Resources.GetColour(value); }
    }

    /// <summary>
    /// Roll number associated to the tile
    /// </summary>
    public int Value = 0;

    /// <summary>
    /// True if value was rolled
    /// </summary>
    public bool Active = false;

    /// <summary>
    /// True if occupied by robber
    /// </summary>
    public bool Robber = false;

    public Tile()
    {
        Resource = Type.Empty;
    }

    /// <summary>
    /// Draw underlying hex and info
    /// </summary>
    public override void Draw(Transform transform, Canvas canvas)
    {
        // Draw back hex
        base.Draw(transform, canvas);

        // Draw circle for value visibility
        // Coloured same as desert for easy blending
        // Also acts as robber indicator
        canvas.shapeBatcher.DrawFilledCircle(transform.Translation, transform.Scale * 0.15f, 10, 
            Robber ? Color.Black : Resources.GetColour(Type.Empty));

        // Draw value number
        if (Resource != Type.Empty && !Robber)
        {
            string valueString = Value.ToString();
            Vector2 texPos = transform.Translation.FlipY(canvas.ScreenSize.Y) - (Catan.s_Font.MeasureString(valueString) * 0.5f);

            canvas.spriteBatch.DrawString(Catan.s_Font, valueString, texPos, Active ? Color.Red : Color.Black);
        }
    }
}