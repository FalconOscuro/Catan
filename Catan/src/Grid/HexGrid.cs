using Catan;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
            Width = Height * 2f * INVERSE_SQRT_3;
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
    /// Pre-computed cosine of rotation
    /// controlled via setter, do not modify
    /// </summary>
    private float m_CosRot;

    /// <summary>
    /// Pre-computer sine of rotation
    /// controlled via setter, do not modify
    /// </summary>
    private float m_SinRot;

    /// <summary>
    /// Grid rotation in radians
    /// </summary>
    public float Rotation { 
        get { return m_Rotation; }
        set {
            m_Rotation = value;

            m_CosRot = MathF.Cos(m_Rotation);
            m_SinRot = MathF.Sin(m_Rotation);
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

        Vector3 pos = new Vector2(x * Width * 0.75f, (y * Height) + (Math.Abs(x % 2) == 1 ? Height * 0.5f : 0f)).PreComputedRotate(m_SinRot, m_CosRot);
        pos.X += Offset.X;
        pos.Y += Offset.Y;

        float hexScale = Height * DRAWN_HEX_SCALE;

        for (int i = 0; i < 7; i++)
            vertices[i + copyIndex] = new VertexPositionColor((UNSCALED_HEX_VERTICES[i].PreComputedRotate(m_SinRot, m_CosRot) * hexScale) + pos, hex.Colour);
    }

    private static void GetHexIndices(int[] indices, int index)
    {
        int vertexOffset = index * 7;
        int copyIndex = index * 18;

        // Offset to match current vertex count
        for (int i = 0; i < 18; i++)
            indices[i + copyIndex] = HEX_INDICES[i] + vertexOffset;
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