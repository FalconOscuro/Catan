using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Grid.Hexagonal;

public class Hex
{
    public Hex(Axial axial)
    {
        Position = axial;
    }

    public Color Colour = Color.Black;

    public Axial Position { get; private set; }
}

public abstract class HexFactory {
    public abstract Hex CreateHex(Axial pos);
}

public class DefaultHexFactory : HexFactory {
    public override Hex CreateHex(Axial pos) {
        return new Hex(pos);
    }
}