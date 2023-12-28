using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;

using Utility;
using Utility.Graphics;

namespace Grid.Hexagonal;

/// <summary>
/// A single edge of a hexagon within the grid
/// </summary>
public class Edge
{
    public Color Colour = Color.Black;

    public Edge()
    {}

    /// <summary>
    /// Shallow copy
    /// </summary>
    public virtual object Clone() {
        return this.MemberwiseClone();
    }

    public virtual void Draw(Transform transform, Canvas canvas)
    {
        Vector2 start = new(){
            X = -1
        };

        Vector2 end = new(){
            X = 1
        };

        start = transform.Apply(start);
        end = transform.Apply(end);

        canvas.shapeBatcher.DrawLine(start, end, transform.Scale * 0.1f, Colour);
    }

    /// <summary>
    /// Side of edge relative to a hex
    /// </summary>
    public enum Side {
        NW,
        N,
        NE,
        SE,
        S,
        SW
    }

    /// <summary>
    /// Position of an edge,
    /// Described as cardinal direction relative to axial position of hex
    /// </summary>
    public struct Key {
        public Axial Position;
        public Side Side;

        public readonly bool Aligned() {
            return Side < Side.SE;
        }

        /// <summary>
        /// Each Edge can be referred to as being relative to the adjacent 2 
        /// hexagon positions. To avoid duplicating data, when stored into a grid
        /// all positions should be converted to the relative NW/N/NE position
        /// of a tile.
        /// </summary>
        /// <returns>Position referring to same side but aligned to be NW/N/NE of a tile</returns>
        public readonly Key Align()
        {
            Key aligned = new(){
                Position = Position,
                Side = Side
            };

            // Already aligned
            if (Aligned())
                return aligned;

            aligned.Side -= 3;

            Axial offset = new(){
                Q = -((int)aligned.Side - 1),
                R = aligned.Side < Side.NE ? -1 : 0
            };

            aligned.Position += offset;

            return aligned;
        }

        /// <summary>
        /// Get position for centre of described edge converted to real-space
        /// </summary>
        /// <returns></returns>
        public readonly Vector2 GetRealPos() {
            Vector2 offset = new(0, 0.5f);

            return Position.GetRealPos() + offset.Rotate(GetRotation());
        }

        /// <summary>
        /// Get rotation for described edge from horizontal
        /// </summary>
        public readonly float GetRotation() {
            return -((int)Align().Side - 1) * MathF.PI / 3f;
        }
    
        /// <summary>
        /// Get aligned positions for vertices at either end of edge
        /// </summary>
        /// <remarks>
        /// NOTE: Returned positions are not garuanteed to exist within grid
        /// </remarks>
        /// <returns>Array of length 2</returns>
        public readonly Vertex.Key[] GetEndpoints()
        {
            // If not aligned recurse to aligned version
            if (!Aligned())
                return Align().GetEndpoints();

            Vertex.Key[] keys = new Vertex.Key[2];

            switch (Side)
            {
            case Side.NW:
                keys[0] = new(){
                    Side = Vertex.Side.W
                };
                keys[1] = new(){
                    Position = new(-1, 1),
                    Side = Vertex.Side.E
                };
                break;

            case Side.N:
                keys[0] = new(){
                    Position = new(-1, 1),
                    Side = Vertex.Side.E
                };
                keys[1] = new(){
                    Position = new(1, 0),
                    Side = Vertex.Side.W
                };
                break;

            case Side.NE:
                keys[0] = new(){
                    Position = new(1, 0),
                    Side = Vertex.Side.W
                };
                keys[1] = new(){
                    Side = Vertex.Side.E
                };
                break;
            }

            for (int i = 0; i < 2; i++)
                keys[i].Position += Position;
            
            return keys;
        }

        public override readonly bool Equals([NotNullWhen(true)] object obj)
        {
            Key? key = obj as Key?;

            if (!key.HasValue)
                return false;
            
            return key.Value == this;
        }

        public override readonly int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator==(Key a, Key b) {
            a = a.Align();
            b = b.Align();

            return a.Position == b.Position && a.Side == b.Side;
        }

        public static bool operator!=(Key a, Key b) {
            a = a.Align();
            b = b.Align();

            return a.Position != b.Position || a.Side != b.Side;
        }
    }
}