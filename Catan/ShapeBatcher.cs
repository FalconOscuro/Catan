using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan;

/*
 * Shape batcher is based on code development by Two-Bit Coding
 * For an in depth explanation see
 * https://www.youtube.com/watch?v=ZqwfoMjJAO4
 * https://www.youtube.com/watch?v=nG9mTQcGnG0
 * https://www.youtube.com/watch?v=rrDDryCRl94
 * */

internal class ShapeBatcher
{
    private Game1 m_Game;

    private bool m_Disposed;
    private BasicEffect m_Effect;
    private VertexPositionColor[] m_Vertices;
    private int[] m_Indices;

    private int m_ShapeCount = 0;
    private int m_VertexCount = 0;
    private int m_IndexCount = 0;

    private bool m_Started = false;

    public static readonly float MIN_LINE_THICKNESS = 2f;
    public static readonly float MAX_LINE_THICKNESS = 10f;

    public ShapeBatcher(Game1 game)
    {
        m_Game = game ?? throw new ArgumentNullException(nameof(game));
        m_Disposed = false;
        m_Effect = new BasicEffect(m_Game.GraphicsDevice);
        m_Effect.TextureEnabled = false;
        m_Effect.FogEnabled = false;
        m_Effect.LightingEnabled = false;
        m_Effect.VertexColorEnabled = true;
        m_Effect.World = Matrix.Identity;
        m_Effect.View = Matrix.Identity;
        m_Effect.Projection = Matrix.Identity;

        const int MAX_VERTEX_COUNT = 1024;
        const int MAX_INDEX_COUNT = MAX_VERTEX_COUNT * 3;
        m_Vertices = new VertexPositionColor[MAX_VERTEX_COUNT];
        m_Indices = new int[MAX_INDEX_COUNT];
    }

    public void Dispose()
    {
        if (m_Disposed)
            return;

        m_Effect?.Dispose();
        m_Disposed = true;
    }

    public void Begin()
    {
        if (m_Started)
            throw new System.Exception("Batch already started.");

        Viewport viewport = m_Game.GraphicsDevice.Viewport;
        m_Effect.Projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, 0, viewport.Height, 0f, 1f);

        m_Started = true;
    }

    public void End()
    {
        Flush();
        m_Started = false;
    }

    private void Flush()
    {
        if (m_ShapeCount == 0)
            return;

        EnsureStarted();

        foreach (EffectPass pass in m_Effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            m_Game.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                PrimitiveType.TriangleList,
                m_Vertices,
                0,
                m_VertexCount,
                m_Indices,
                0,
                m_IndexCount / 3
            );
        }

        m_ShapeCount = 0;
        m_IndexCount = 0;
        m_VertexCount = 0;
    }

    private void EnsureStarted()
    {
        if (!m_Started)
            throw new System.Exception("Batch not started.");
    }

    private void EnsureSpace(int shapeVertexCount, int shapeIndexCount)
    {
        if (shapeVertexCount > m_Vertices.Length)
            throw new System.Exception("Maximum shape vertex count is " + m_Vertices.Length);

        if (shapeIndexCount > m_Indices.Length)
            throw new System.Exception("Maximum shape index count is " + m_Indices.Length);

        if (m_VertexCount + shapeVertexCount > m_Vertices.Length ||
            m_IndexCount + shapeIndexCount > m_Indices.Length)
            Flush();
    }

    /// <summary>
    /// Draws a line from pA to PB as a rectangle with thickness pThickness and colour pColour.
    /// </summary>
    /// <param name="a">Start of the line</param>
    /// <param name="b">End of the line</param>
    /// <param name="thickness">Thickness of the line (clamped between 2 and 10)</param>
    /// <param name="colour">Colour of the line</param>
    public void DrawLine(Vector2 a, Vector2 b, float thickness, Color colour)
    {
        EnsureStarted();

        const int shapeVertexCount = 4;
        const int shapeIndexCount = 6;

        EnsureSpace(shapeVertexCount, shapeIndexCount);

        thickness = Math.Clamp(thickness, MIN_LINE_THICKNESS, MAX_LINE_THICKNESS);

        float halfThickness = thickness * 0.5f;

        float e1x = b.X - a.X;
        float e1y = b.Y - a.Y;

        float invLength = halfThickness / MathF.Sqrt(e1x * e1x + e1y * e1y);

        e1x *= invLength;
        e1y *= invLength;

        float e2x = -e1x;
        float e2y = -e1y;

        float n1x = -e1y;
        float n1y = e1x;

        float n2x = -n1x;
        float n2y = -n1y;

        m_Indices[m_IndexCount++] = 0 + m_VertexCount;
        m_Indices[m_IndexCount++] = 1 + m_VertexCount;
        m_Indices[m_IndexCount++] = 2 + m_VertexCount;
        m_Indices[m_IndexCount++] = 0 + m_VertexCount;
        m_Indices[m_IndexCount++] = 2 + m_VertexCount;
        m_Indices[m_IndexCount++] = 3 + m_VertexCount;

        m_Vertices[m_VertexCount++] = new VertexPositionColor(new Vector3(a.X + n1x + e2x, a.Y + n1y + e2y, 0f), colour);
        m_Vertices[m_VertexCount++] = new VertexPositionColor(new Vector3(b.X + n1x + e1x, b.Y + n1y + e1y, 0f), colour);
        m_Vertices[m_VertexCount++] = new VertexPositionColor(new Vector3(b.X + n2x + e1x, b.Y + n2y + e1y, 0f), colour);
        m_Vertices[m_VertexCount++] = new VertexPositionColor(new Vector3(a.X + n2x + e2x, a.Y + n2y + e2y, 0f), colour);

        m_ShapeCount++;
    }

    public void DrawCircle(Vector2 centre, float radius, int numVertices, float thickness, Color colour)
    {
        const int MIN_POINTS = 3;
        const int MAX_POINTS = 256;

        numVertices = Math.Clamp(numVertices, MIN_POINTS, MAX_POINTS);

        float deltaAngle = MathHelper.TwoPi / numVertices;
        float angle = 0f;

        for (int i = 0; i < numVertices; i++)
        {
            float ax = centre.X + radius * MathF.Sin(angle);
            float ay = centre.Y + radius * MathF.Cos(angle);

            angle += deltaAngle;

            float bx = centre.X + radius * MathF.Sin(angle);
            float by = centre.Y + radius * MathF.Cos(angle);
            DrawLine(new Vector2(ax, ay), new Vector2(bx, by), thickness, colour);
        }
    }

    public void DrawArrow(Vector2 start, Vector2 vector, float thickness, float arrowSize, Color colour)
    {
        Vector2 lineEnd = start + vector;

        Vector2 u = vector * (1f / vector.Length());
        Vector2 v = new Vector2(-u.Y, u.X);

        Vector2 arrowHead1 = lineEnd - arrowSize * u + arrowSize * v;
        Vector2 arrowHead2 = lineEnd - arrowSize * u - arrowSize * v;

        DrawLine(start, lineEnd, thickness, colour);
        DrawLine(lineEnd, arrowHead1, thickness, colour);
        DrawLine(lineEnd, arrowHead2, thickness, colour);
    }

    /// <summary>
    /// Draw filled hexagon from triangle primitives
    /// </summary>
    /// <param name="centre">hexagon centre position</param>
    /// <param name="scale">Side Length</param>
    /// <param name="colour">Fill colour</param>
    public void DrawHex(Vector2 centre, float scale, Color colour)
    {
        EnsureStarted();

        const int hexVertexCount = 7;
        const int hexIndexCount = 18;

        EnsureSpace(hexVertexCount, hexIndexCount);
        
        // Pre-computed sin & cosine values
        // Angle is 60 or PI/3 radians for hexagon
        // cos(60) = .5f && cos(x) = cos(-x)
        const float cosine = .5f;
        // sin(60) = sqrt(3)/2 && sin(-x) = -sin(x)
        const float sine = 0.8660254037844386467637231707529361834714026269051903140279034897f;

        // Only need 3 points as hexagon is dihedral (6 reflection symmetries) so can use negatives for other points
        Vector3 p1 = new Vector3(-sine, cosine, 0f) * scale;
        Vector3 p2 = new Vector3(0f, scale, 0f);
        Vector3 p3 = new Vector3(-p1.X, p1.Y, 0f);

        Vector3 centerV3 = new Vector3(centre.X, centre.Y, 0f);

        for (int i = 1; i < 6; i++)
        {
            m_Indices[m_IndexCount++] = m_VertexCount;
            m_Indices[m_IndexCount++] = m_VertexCount + i;
            m_Indices[m_IndexCount++] = m_VertexCount + i + 1;
        }

        // Create last triangle avoiding modulo
        m_Indices[m_IndexCount++] = m_VertexCount;
        m_Indices[m_IndexCount++] = m_VertexCount + 6;
        m_Indices[m_IndexCount++] = m_VertexCount + 1;

        m_Vertices[m_VertexCount++] = new VertexPositionColor(centerV3, colour);
        
        m_Vertices[m_VertexCount++] = new VertexPositionColor(centerV3 + p1, colour);
        m_Vertices[m_VertexCount++] = new VertexPositionColor(centerV3 + p2, colour);
        m_Vertices[m_VertexCount++] = new VertexPositionColor(centerV3 + p3, colour);
        m_Vertices[m_VertexCount++] = new VertexPositionColor(centerV3 - p1, colour);
        m_Vertices[m_VertexCount++] = new VertexPositionColor(centerV3 - p2, colour);
        m_Vertices[m_VertexCount++] = new VertexPositionColor(centerV3 - p3, colour);

        m_ShapeCount++;
    }
}