using Catan;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Grid.Hexagonal;
using static Utility;

/// <summary>
/// Hexagonal Grid
/// </summary>
public class HexGrid<T> where T : Hex
{
    /// <summary>
    /// Backing field for height property
    /// Do not modify directly, use the height setter
    /// </summary>
    private float m_Height;

    /// <summary>
    /// Height of a hexagon
    /// </summary>
    public float Height 
    { 
        get{ return m_Height; }
        set{
            m_Height = value;
            Width = value * 2f * INVERSE_SQRT_3;
        }
    }

    /// <summary>
    /// Width of a hexagon, calculated from the height
    /// </summary>
    public float Width { get; private set;}

    public Vector2 Offset { get; set; }

    /// <summary>
    /// Backing field for rotation property
    /// Do not modify directly use Rotation setter
    /// </summary>
    private float m_Rotation;

    /// <summary>
    /// Pre-computer sine of rotation
    /// controlled via setter, do not modify
    /// </summary>
    private float m_SinRot;

    /// <summary>
    /// Pre-computed cosine of rotation
    /// controlled via setter, do not modify
    /// </summary>
    private float m_CosRot;

    /// <summary>
    /// Grid rotation in radians
    /// </summary>
    public float Rotation { 
        get { return m_Rotation; }
        set {
            m_Rotation = value;

            m_CosRot = MathF.Cos(value);
            m_SinRot = MathF.Sin(value);
        } 
    }

    /// <summary>
    /// Hexagonal tiles keyed by their position
    /// </summary>
    private readonly Dictionary<(int, int), T> m_Tiles;

    private readonly ShapeBatcher m_ShapeBatcher;

    private static readonly float DRAWN_HEX_SCALE = 0.9f;

    public HexGrid(ShapeBatcher shapeBatcher)
    {
        m_Tiles = new Dictionary<(int, int), T>();
        m_ShapeBatcher = shapeBatcher;

        Rotation = 0f;
    }

    public T this[int x, int y]
    {
        get
        {
            if (m_Tiles.TryGetValue((x, y), out T found))
                return found;

            return null;
        }

        set
        {
            m_Tiles[(x, y)] = value;
        }
    }

    /// <summary>
    /// Get vertices for drawing a single hexagon
    /// </summary>
    /// <param name="hexKeyPair">The hexagon and its position</param>
    /// <param name="vertices">vertex array to write to</param>
    /// <param name="copyIndex">Index at which to place vertices in array</param>
    private void GetHexVertices(KeyValuePair<(int, int), T> hexKeyPair, VertexPositionColor[] vertices, int copyIndex)
    {
        int x = hexKeyPair.Key.Item1;
        int y = hexKeyPair.Key.Item2;

        T hex = hexKeyPair.Value;

        Vector3 pos = new Vector2(
            x * Width * 0.75f, 
            (y * Height) + (Math.Abs(x % 2) == 1 ? Height * 0.5f : 0f)
                ).PreComputedRotate(m_SinRot, m_CosRot).ToVec3();

        pos.X += Offset.X;
        pos.Y += Offset.Y;

        float hexScale = Height * DRAWN_HEX_SCALE;

        for (int i = 0; i < 7; i++)
            vertices[i + copyIndex] = new VertexPositionColor((UNSCALED_HEX_VERTICES[i].PreComputedRotate(m_SinRot, m_CosRot).ToVec3() * hexScale) + pos, hex.Colour);
    }

    private static void GetHexIndices(int[] indices, int index)
    {
        int vertexOffset = index * 7;
        int copyIndex = index * 18;

        // Offset to match current vertex count
        for (int i = 0; i < 18; i++)
            indices[i + copyIndex] = HEX_INDICES[i] + vertexOffset;
    }

    /// <summary>
    /// Convert a point to a position within the hex grid
    /// </summary>
    /// <returns>Whether hex at x/y exists</returns>
    public bool FindHex(Vector2 pos, out int x, out int y)
    {
        // Adjust pos to match transform
        pos -= Offset;

        pos = pos.PreComputedRotate(-m_SinRot, m_CosRot);
        pos.X += Width * 0.5f;
        pos.Y += Height * 0.5f;

        // Get test rectangle pos
        x = (int)MathF.Floor(pos.X / (Width * 0.75f));

        bool odd = Math.Abs(x) % 2 == 1;

        // Offset by half on odd columns
        y = (int)MathF.Floor((pos.Y / Height) - (odd ? 0.5f : 0));

        Vector2 rectPos = new(){
            X = x * Width * .75f,
            Y = (y + (odd ? 1f : 0.5f)) * Height
        };

        // Position relative to test rect
        Vector2 testPos = pos - rectPos;

        bool isAbove = false, isBelow = false;

        // If point is in left quarter of rectangle 
        // rectangle pos may not equal hex pos
        if (testPos.X * 4 < Width)
        {
            if (testPos.Y > 0f)
                isAbove = testPos.Y * INVERSE_SQRT_3 > testPos.X;

            else if (testPos.Y < 0f)
                isBelow = -testPos.Y * INVERSE_SQRT_3 > testPos.X;
        }

        if (isAbove)
        {
            if (odd)
                y++;
            x--;
        }

        else if (isBelow)
        {
            if (!odd)
                y--;
            x--;
        }

        return m_Tiles.ContainsKey((x, y));
    }

    public void Draw()
    {
        int hexCount = m_Tiles.Count;

        /// <summary>
        /// Allocate space for vertices and indices
        /// </summary>
        int[] indices = new int[18 * hexCount];
        VertexPositionColor[] vertices = new VertexPositionColor[7 * hexCount];

        /// <summary>
        /// Loop through all hexes
        /// </summary>
        int index = 0;
        foreach (var hexKeyPair in m_Tiles)
        {
            GetHexVertices(hexKeyPair, vertices, index * 7);
            GetHexIndices(indices, index++);
        }

        m_ShapeBatcher.DrawPrimitives(vertices, indices);
    }
}