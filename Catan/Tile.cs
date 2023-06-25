using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Catan;

class Tile
{
    public Tile()
        {
            Position = Vector2.Zero;
            Type = Resources.Type.Empty;
            Value = 0;
        }

        public Vector2 Position;

        public Resources.Type Type;

        public int Value;

        public Node[] Nodes = new Node[6];

        public void Distribute()
        {
            foreach (Node node in Nodes)
                node.Distribute(Type);
        }

        public void ShapeDraw(ShapeBatcher shapeBatcher, float scale)
        {
            shapeBatcher.DrawHex(Position, scale * .9f, Resources.GetResourceColour(Type));
        }

        public void SpriteDraw(SpriteBatch spriteBatch, SpriteFont font, float windowHeight, int active)
        {
            spriteBatch.DrawString(font, Value.ToString(), Position.FlipY(windowHeight), Value == active ? Color.Red : Color.Black);
        }

        // Default resource layout defined by rulebook
        public static readonly Resources.Type[] DEFAULT_RESOURCE_SPREAD = {
                    Resources.Type.Ore, Resources.Type.Wool, Resources.Type.Lumber,
                Resources.Type.Grain, Resources.Type.Brick, Resources.Type.Wool, Resources.Type.Brick,
            Resources.Type.Grain, Resources.Type.Lumber, Resources.Type.Empty, Resources.Type.Lumber, Resources.Type.Ore,
                Resources.Type.Lumber, Resources.Type.Ore, Resources.Type.Grain, Resources.Type.Wool,
                    Resources.Type.Brick, Resources.Type.Grain, Resources.Type.Wool
            };

        public static readonly int[] DEFAULT_NUMBER_SPREAD = {
            10, 2, 9,
            12, 6, 4, 10,
            9, 11, 3, 8,
            8, 3, 4, 5,
            5, 6, 11
        };
}