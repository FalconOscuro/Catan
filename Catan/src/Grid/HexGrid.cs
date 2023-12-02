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

    /// <summary>
    /// Hexagonal tiles keyed by their position
    /// </summary>
    private readonly Dictionary<(int, int), T> m_Tiles;

    private readonly ShapeBatcher m_ShapeBatcher;

    private static readonly float DRAWN_HEX_SCALE = 0.9f;

    public HexGrid(ShapeBatcher shapeBatcher, float height)
    {
        Height = height;
        m_Tiles = new Dictionary<(int, int), T>();
        m_ShapeBatcher = shapeBatcher;
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

        Vector3 pos = new()
        {
            Y = y * Height + (x % 2 == 1 ? Height * 0.5f : 0f),
            X = x * Width * 0.75f,
            Z = 0f
        };

        float hexScale = Height * DRAWN_HEX_SCALE;

        for (int i = 0; i < 7; i++)
            vertices[i + copyIndex] = new VertexPositionColor((UNSCALED_HEX_VERTICES[i] * hexScale) + pos, hex.Colour);
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