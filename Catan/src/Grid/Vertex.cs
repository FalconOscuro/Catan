using System;
using Microsoft.Xna.Framework;

using Utility;
using Utility.Graphics;

namespace Grid.Hexagonal;
using static Utility;

/// <summary>
/// A singular vertex for a hexagon in a grid
/// </summary>
public class Vertex
{
    public Vertex()
    {}

    public bool DrawFilled = true;
    public Color Colour = Color.Black;

    public virtual object Clone() {
        return this.MemberwiseClone();
    }

    public virtual void Draw(Transform transform, Canvas canvas)
    {
        if (DrawFilled)
            canvas.shapeBatcher.DrawFilledCircle(transform.Translation, transform.Scale, 10, Colour);

        else
            canvas.shapeBatcher.DrawCircle(transform.Translation, transform.Scale, 10, transform.Scale * 0.1f, Colour);
    }

    /// <summary>
    /// Side of vertex relative to a tiles axial position
    /// </summary>
    public enum Side {
        W,
        E,
        NW,
        NE,
        SE,
        SW
    }

    /// <summary>
    /// Describes the position of a vertex
    /// As a cardinal direction from an axial position
    /// </summary>
    public struct Key
    {
        public Axial Position;
        public Side Side;

        public readonly bool Aligned() {
            return Side < Side.NW;
        }

        /// <summary>
        /// Each vertex can be referred to as being relative to the surrounding 3 
        /// hexagon positions. To avoid duplicating data, when stored into a grid
        /// all positions should be converted to the relative east or west position
        /// on a tile.
        /// </summary>
        /// <returns>Position referring to same vertex but aligned to be E/W of a tile</returns>
        public readonly Key Align()
        {
            Key aligned = new(){
                Position = Position,
                Side = Side
            };

            if (Aligned())
                return aligned;

            // Could be cleaned up
            switch (Side)
            {
            case Side.NW:
                aligned.Position.Q--;
                aligned.Position.R++;
                aligned.Side = Side.E;
                break;

            case Side.NE:
                aligned.Position.Q++;
                aligned.Side = Side.W;
                break;

            case Side.SE:
                aligned.Position.Q++;
                aligned.Position.R--;
                aligned.Side = Side.W;
                break;

            case Side.SW:
                aligned.Position.Q--;
                aligned.Side = Side.E;
                break;
            }

            return aligned;
        }

        /// <summary>
        /// Get the un-transformed real-space position of the vertex
        /// </summary>
        public readonly Vector2 GetRealPos() {
            return Position.GetRealPos() + 
                new Vector2(INVERSE_SQRT_3 * (Align().Side == Side.W ? -1 : 1), 0);
        }

        /// <summary>
        /// Get an array of aligned positions for all adjacent vertices.
        /// </summary>
        /// <remarks>
        /// NOTE: Returned positions are not garuanteed to exist within grid
        /// </remarks>
        /// <returns>Array of length 3</returns>
        public readonly Key[] GetAdjacentVertices()
        {
            // If unaligned recurse to aligned version
            if (!Aligned())
                return Align().GetAdjacentVertices();

            int mult;
            Side adjSide;

            if (Side == Side.W)
            {
                mult = -1;
                adjSide = Side.E;
            }

            else
            {
                mult = 1;
                adjSide = Side.W;
            }

            Key[] keys = new Key[3];

            for (int i = 0; i < 3; i++)
                keys[i] = new(){
                    Position = Position + 
                        (new Axial(i == 0 ? 2 : 1, i == 2 ? 0 : -1) * mult),
                    Side = adjSide
                };

            return keys;
        }

        /// <summary>
        /// Get an array of aligned positions for all protruding edges
        /// </summary>
        /// <remarks>
        /// NOTE: Returned positions are not garuanteed to exist within grid
        /// </remarks>
        /// <returns>Array of length 3</returns>
        public readonly Edge.Key[] GetProtrudingEdges()
        {
            // If not aligned recurse to aligned version
            if (!Aligned())
                return Align().GetProtrudingEdges();

            Edge.Key[] keys = new Edge.Key[]{new(), new(), new()};

            if (Side == Side.W)
            {
                keys[0].Side = Edge.Side.NW;
                keys[1].Position = Position + new Axial(-1, 0);
                keys[2].Side = Edge.Side.NE;
            }

            else
            {
                keys[0].Side = Edge.Side.NE;
                keys[1].Position = Position + new Axial(1, -1);
                keys[2].Side = Edge.Side.NW;
            }

            keys[0].Position = Position;
            keys[1].Side = Edge.Side.N;
            keys[2].Position = keys[1].Position;

            return keys;
        }
    }
}