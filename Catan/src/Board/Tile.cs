using Microsoft.Xna.Framework;
using Grid.Hexagonal;

namespace Catan;

public class Tile : Hex 
{
    private Resources.Type m_Resource;

    public Resources.Type Resource { 
        get { return m_Resource; }
        set { m_Resource = value; Colour = Resources.GetColour(value); }
    }

    public int Value = 0;

    public Tile(Axial axial):
        base(axial)
    {
        Resource = Resources.Type.Empty;
    }

    public override void Draw(Transform transform, Canvas canvas)
    {
        base.Draw(transform, canvas);

        canvas.shapeBatcher.DrawFilledCircle(transform.Translation, transform.Scale * 0.15f, 10, Resources.GetColour(Resources.Type.Empty));

        if (Resource != Resources.Type.Empty)
        {
            string valueString = Value.ToString();

            Vector2 texPos = transform.Translation.FlipY(canvas.ScreenSize.Y) - (Catan.s_Font.MeasureString(valueString) * 0.5f);

            canvas.spriteBatch.DrawString(Catan.s_Font, valueString, texPos, Color.Black);
        }
    }
}

public class TileFactory : HexFactory {
    public override Hex CreateHex(Axial pos)
    {
        return new Tile(pos);
    }
}