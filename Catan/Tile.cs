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

            Selected = false;
            Robber = false;
        }

        public Vector2 Position;

        public Resources.Type Type;

        public int Value;

        public Node[] Nodes = new Node[6];

        public bool Selected;

        public bool Robber;

        public bool TestCollision(Vector2 point, float scale)
        {
            // Heavily simplified, using approximation of inner circle for collision
            // .75f is magic number, actual scale is .9f of input scale & shortest edge distane is ~= .87 of scale
            // .87 * .9f squared is then ~= to magic number .75f
            return Vector2.DistanceSquared(point, Position) < scale * scale * .75f;
        }

        public void Distribute()
        {
            if (Robber)
                return;

            foreach (Node node in Nodes)
                node.Distribute(Type);
        }

        public void ShapeDraw(ShapeBatcher shapeBatcher, float scale)
        {
            // Hexagon is basically a 6 sided circle ¯\_(ツ)_/¯
            shapeBatcher.DrawFilledCircle(Position, (scale + (Selected ? 2f : 0f)) * .9f, 6, Resources.GetResourceColour(Type));
        }

        public void SpriteDraw(SpriteBatch spriteBatch, SpriteFont font, float windowHeight, int active)
        {
            string text;
            if (Robber)
                text = "R";

            else if (Value == 7)
                return;

            else
                text = Value.ToString();

            Color colour;
            if (Value == active)
            {
                if (Robber)
                    colour = Color.Gray;

                else
                    colour = Color.Red;
            }

            else
                colour = Color.Black;

            spriteBatch.DrawString(font, text, Position.FlipY(windowHeight), colour);
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