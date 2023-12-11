using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Utility;
using Utility.Graphics;

namespace Grid.Hexagonal;
using static Utility;

/// <summary>
/// Hexagonal Grid
/// </summary>
public class HexGrid
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

    /// <summary>
    /// Offset position for center of hex graph
    /// </summary>
    public Vector2 Offset { get; set; }

    /// <summary>
    /// Grid rotation in radians
    /// </summary>
    public float Rotation { get; set; }

    /// <summary>
    /// Dictionary of hex containers
    /// Each containing 1 hex, 3 sides and 2 corners
    /// </summary>
    private readonly Dictionary<Axial, HexContainer> m_Hexes;

    public HexGrid()
    {
        m_Hexes = new();
    }

    /// <summary>
    /// Creates deep copy of this grid
    /// </summary>
    public HexGrid Clone()
    {
        HexGrid clone = new(){
            Height = Height,
            Offset = Offset,
            Rotation = Rotation
        };

        // Clone dictionary
        foreach (var keyPair in m_Hexes)
            clone.m_Hexes[keyPair.Key] = keyPair.Value.Clone();

        return clone;
    }

    /// <summary>
    /// Attempt to fetch hex at given position
    /// </summary>
    /// <returns>False if hex does not exist</returns>
    public bool TryGetHex(Axial pos, out Hex hex)
    {
        hex = null;
        if (m_Hexes.TryGetValue(pos, out var container))
            hex = container.Hex;
        
        return hex != null;
    }

    /// <summary>
    /// Create and return Hex at given position
    /// </summary>
    /// <returns>Created hex or existing hex position is occupied</returns>
    public void InsertHex(Axial pos, Hex hex)
    {
        // Test for container
        if (!m_Hexes.ContainsKey(pos))
            m_Hexes[pos] = new HexContainer(pos);
        
        m_Hexes[pos].Hex = hex;
    }

    /// <summary>
    /// Try to get edge at given position
    /// </summary>
    /// <returns>False if edge does not exist</returns>
    public bool TryGetEdge(Edge.Key key, out Edge edge)
    {
        // Align key to Hex container format
        key.Align();

        edge = null;
        if (m_Hexes.TryGetValue(key.Position, out var container))
            edge = container.Edges[(int)key.Side];

        return edge != null;
    }

    /// <summary>
    /// Create edge at given position
    /// </summary>
    /// <returns>Created edge or pre-existing edge</returns>
    public void InsertEdge(Edge.Key key, Edge edge)
    {
        // Align to container format
        key.Align();

        // Test for container
        if (!m_Hexes.ContainsKey(key.Position))
            m_Hexes[key.Position] = new HexContainer(key.Position);

        m_Hexes[key.Position].Edges[(int)key.Side] = edge;
    }

    public bool TryGetCorner(Corner.Key key, out Corner corner)
    {
        key.Align();

        corner = null;
        if (m_Hexes.TryGetValue(key.Position, out var container))
            corner = container.Corners[(int)key.Side];
        
        return corner != null;
    }

    public void InsertCorner(Corner.Key key, Corner corner)
    {
        key.Align();

        if (!m_Hexes.ContainsKey(key.Position))
            m_Hexes[key.Position] = new HexContainer(key.Position);

        m_Hexes[key.Position].Corners[(int)key.Side] = corner;
    }

    /// <summary>
    /// Convert a point to a position within the hex grid
    /// </summary>
    /// <returns>Whether hex at x/y exists</returns>
    public bool FindHex(Vector2 pos, out Axial axialPos)
    {
        int s;

        // Adjust pos to match transform
        pos -= Offset;
        pos = pos.Rotate(Rotation);

        float qFrac = pos.X * 4 / (Width * 3);
        float rFrac = (pos.Y - (pos.X * INVERSE_SQRT_3)) / Height;
        float sFrac = -qFrac-rFrac;

        axialPos.q = (int)MathF.Round(qFrac);
        axialPos.r = (int)MathF.Round(rFrac);
        s = (int)MathF.Round(sFrac);

        float qDiff = Math.Abs(qFrac - axialPos.q);
        float rDiff = Math.Abs(rFrac - axialPos.r);
        float sDiff = Math.Abs(sFrac - s);

        if (qDiff > rDiff && qDiff > sDiff)
            axialPos.q = -axialPos.r-s;
        
        else if (rDiff > sDiff)
            axialPos.r = -axialPos.q-s;

        return m_Hexes.ContainsKey(axialPos);
    }

    public void Draw(Canvas canvas)
    {
        Transform transform = new(){
            Scale = Height,
            Rotation = Rotation,
            Translation = Offset
        };

        /// <summary>
        /// Loop through all hexes
        /// </summary>
        foreach (var hexKeyPair in m_Hexes)
            hexKeyPair.Value.Draw(transform, canvas);
    }
}