using System;
using System.Collections.Generic;
using Catan;
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
    private readonly Dictionary<Axial, Hex> m_Hexes;

    private readonly Dictionary<Vertex.Key, Vertex> m_Vertices;
    private readonly Dictionary<Edge.Key, Edge> m_Edges;

    public HexGrid()
    {
        m_Hexes = new();
        m_Vertices = new();
        m_Edges = new();
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

        // Clone dictionaries
        foreach (var keyPair in m_Hexes)
            clone.m_Hexes[keyPair.Key] = (Hex)keyPair.Value.Clone();
        
        foreach (var keyPair in m_Edges)
            clone.m_Edges[keyPair.Key] = (Edge)keyPair.Value.Clone();
        
        foreach (var keyPair in m_Vertices)
            clone.m_Vertices[keyPair.Key] = (Vertex)keyPair.Value.Clone();

        return clone;
    }

    /// <summary>
    /// Attempt to fetch hex at given position
    /// </summary>
    /// <returns>False if hex does not exist</returns>
    public bool TryGetHex<T>(Axial pos, out T hex) where T : Hex
    {
        bool found = m_Hexes.TryGetValue(pos, out var temp);
        hex = temp as T;
        
        return found;
    }

    /// <summary>
    /// Insert Hex at given position
    /// </summary>
    public void InsertHex(Axial pos, Hex hex)
    {
        m_Hexes[pos] = hex;
    }

    /// <summary>
    /// Try to get edge at given position
    /// </summary>
    /// <returns>False if edge does not exist</returns>
    public bool TryGetEdge<T>(Edge.Key key, out T edge) where T : Edge
    {
        bool found = m_Edges.TryGetValue(key.Align(), out var temp);
        edge = temp as T;

        return found;
    }

    /// <summary>
    /// Insert edge at given position
    /// </summary>
    public void InsertEdge(Edge.Key key, Edge edge)
    {
        m_Edges[key.Align()] = edge;
    }

    public bool TryGetVertex<T>(Vertex.Key key, out T vertex) where T : Vertex
    {
        bool found = m_Vertices.TryGetValue(key.Align(), out var temp);
        vertex = temp as T;
        
        return found;
    }

    public void InsertVertex(Vertex.Key key, Vertex vertex)
    {
        m_Vertices[key.Align()] = vertex;
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

        axialPos.Q = (int)MathF.Round(qFrac);
        axialPos.R = (int)MathF.Round(rFrac);
        s = (int)MathF.Round(sFrac);

        float qDiff = Math.Abs(qFrac - axialPos.Q);
        float rDiff = Math.Abs(rFrac - axialPos.R);
        float sDiff = Math.Abs(sFrac - s);

        if (qDiff > rDiff && qDiff > sDiff)
            axialPos.Q = -axialPos.R-s;
        
        else if (rDiff > sDiff)
            axialPos.R = -axialPos.Q-s;

        return m_Hexes.ContainsKey(axialPos);
    }

    public void Draw(Canvas canvas)
    {
        Transform transform = new(){
            Scale = Height,
            Rotation = Rotation,
            Translation = Offset
        };

        // Draw hexes
        foreach (var hexKeyPair in m_Hexes)
        {
            Vector2 realPos = hexKeyPair.Key.GetRealPos();

            Transform hexTransform = new(){
                Rotation = Rotation,
                Scale = Height * 0.9f,// Make modifiable const
                Translation = transform.Apply(realPos)
            };

            hexKeyPair.Value.Draw(hexTransform, canvas);
        }

        // Draw edges
        foreach (var edgeKeyPair in m_Edges)
        {
            float edgeRot = edgeKeyPair.Key.GetRotation();
            Vector2 realPos = edgeKeyPair.Key.GetRealPos();

            Transform edgeTransform = new(){
                Rotation = edgeRot + Rotation,
                Scale = Width * 0.2f,
                Translation = transform.Apply(realPos)
            };

            edgeKeyPair.Value.Draw(edgeTransform, canvas);
        }

        // Draw vertices
        foreach (var vertexKeyPair in m_Vertices)
        {
            Vector2 realPos = vertexKeyPair.Key.GetRealPos();

            // Vertices are single point so do not need rotation
            Transform vTransform = new(){
                Scale = Height * 0.05f,
                Translation = transform.Apply(realPos)
            };

            vertexKeyPair.Value.Draw(vTransform, canvas);
        }
    }
}