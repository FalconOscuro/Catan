using Catan;
using Microsoft.VisualBasic;
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

    private readonly HexFactory m_HexFactory;
    private readonly EdgeFactory m_EdgeFactory;
    private readonly CornerFactory m_CornerFactory;

    private readonly Dictionary<Axial, HexContainer> m_Hexes;

    // Store tiles
    // Store edges

    private readonly ShapeBatcher m_ShapeBatcher;

    private static readonly float DRAWN_HEX_SCALE = 0.9f;

    internal HexGrid(ShapeBatcher shapeBatcher, HexFactory hexFactory, EdgeFactory edgeFactory, CornerFactory cornerFactory)
    {
        m_Hexes = new();

        m_HexFactory = hexFactory;
        m_EdgeFactory = edgeFactory;
        m_CornerFactory = cornerFactory;

        m_ShapeBatcher = shapeBatcher;

        Rotation = 0f;
    }

    public bool TryGetHex(Axial pos, out Hex hex)
    {
        hex = null;
        if (m_Hexes.TryGetValue(pos, out var container))
            hex = container.Hex;
        
        return hex != null;
    }

    public Hex CreateHex(Axial pos)
    {
        if (m_Hexes.TryGetValue(pos, out var container))
        {
            if (container.Hex != null)
                return container.Hex;
        }
            
        else
            m_Hexes[pos] = new HexContainer();
        
        Hex newHex = m_HexFactory.CreateHex(pos);
        m_Hexes[pos].Hex = newHex;

        for (Edge.Key edge = new(){Position = pos}; 
            edge.Side < Edge.Side.SW + 1; edge.Side++)
            CreateEdge(edge);

        for (Corner.Key corner = new(){Position = pos};
            corner.Side < Corner.Side.SW + 1; corner.Side++)
            CreateCorner(corner);

        return newHex;
    }

    public bool TryGetEdge(Edge.Key key, out Edge edge)
    {
        key.Align();

        edge = null;
        if (m_Hexes.TryGetValue(key.Position, out var container))
            edge = container.Edges[(int)key.Side];

        return edge != null;
    }

    public Edge CreateEdge(Edge.Key key)
    {
        key.Align();

        if (m_Hexes.TryGetValue(key.Position, out var container))
        {
            Edge edge = container.Edges[(int)key.Side];

            if (edge != null)
                return edge;
        }

        else
            m_Hexes[key.Position] = new HexContainer();
        
        Edge newEdge = m_EdgeFactory.CreateEdge(key);
        m_Hexes[key.Position].Edges[(int)key.Side] = newEdge;

        return newEdge;
    }

    public bool TryGetCorner(Corner.Key key, out Corner corner)
    {
        key.Align();

        corner = null;
        if (m_Hexes.TryGetValue(key.Position, out var container))
            corner = container.Corners[(int)key.Side];
        
        return corner != null;
    }

    public Corner CreateCorner(Corner.Key key)
    {
        key.Align();

        if (m_Hexes.TryGetValue(key.Position, out var container))
        {
            Corner corner = container.Corners[(int)key.Side];
            if (corner != null)
                return corner;
        }

        else
            m_Hexes[key.Position] = new HexContainer();

        Corner newCorner = m_CornerFactory.CreateCorner(key);
        m_Hexes[key.Position].Corners[(int)key.Side] = newCorner;

        return newCorner;
    }

    /// <summary>
    /// Get vertices for drawing a single hexagon
    /// </summary>
    /// <param name="hexKeyPair">The hexagon and its position</param>
    /// <param name="vertices">vertex array to write to</param>
    /// <param name="copyIndex">Index at which to place vertices in array</param>
    private void GetHexVertices(Axial axialPos, Hex hex, VertexPositionColor[] vertices, int copyIndex)
    {
        Vector2 localPos = new(){
            X = Width * axialPos.q * 0.75f,
            Y = Height * (axialPos.r + axialPos.q * 0.5f)
        };

        Vector3 pos = localPos.PreComputedRotate(m_SinRot, m_CosRot).ToVector3();

        pos.X += Offset.X;
        pos.Y += Offset.Y;

        float hexScale = Height * DRAWN_HEX_SCALE;

        for (int i = 0; i < 7; i++)
            vertices[i + copyIndex] = new VertexPositionColor((UNSCALED_HEX_VERTICES[i].PreComputedRotate(m_SinRot, m_CosRot).ToVector3() * hexScale) + pos, hex.Colour);
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
    public bool FindHex(Vector2 pos, out Axial axialPos)
    {
        int s;

        // Adjust pos to match transform
        pos -= Offset;
        pos = pos.PreComputedRotate(-m_SinRot, m_CosRot);

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

    public void Draw()
    {
        /// <summary>
        /// Loop through all hexes
        /// </summary>
        int index = 0;
        foreach (var hexKeyPair in m_Hexes)
            hexKeyPair.Value.Draw(Height, Rotation, Offset, m_ShapeBatcher);
    }

    public class Builder {
        public Builder(ShapeBatcher shapeBatcher) {
            pShapeBatcher = shapeBatcher;
        }

        public ShapeBatcher pShapeBatcher;
        public HexFactory pHexFactory = null;
        public EdgeFactory pEdgeFactory = null;
        public CornerFactory pCornerFactory = null;

        public HexGrid BuildHexGrid()
        {
            pHexFactory ??= new DefaultHexFactory();
            pEdgeFactory ??= new DefaultEdgeFactory();
            pCornerFactory ??= new DefaultCornerFactory();

            return new HexGrid(pShapeBatcher, pHexFactory, pEdgeFactory, pCornerFactory);
        }
    }
}