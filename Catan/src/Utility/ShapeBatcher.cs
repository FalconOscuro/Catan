using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Utility.Graphics;

/*
 * Shape batcher is based on code development by Two-Bit Coding
 * For an in depth explanation see
 * https://www.youtube.com/watch?v=ZqwfoMjJAO4
 * https://www.youtube.com/watch?v=nG9mTQcGnG0
 * https://www.youtube.com/watch?v=rrDDryCRl94
 * */

public class ShapeBatcher
{
    private readonly Game m_Game;

    private bool m_Disposed;
    private readonly BasicEffect m_Effect;
    private readonly VertexPositionColor[] m_Vertices;
    private readonly int[] m_Indices;

    private int m_ShapeCount = 0;
    private int m_VertexCount = 0;
    private int m_IndexCount = 0;

    private bool m_Started = false;

    public static readonly float MIN_LINE_THICKNESS = 2f;
    public static readonly float MAX_LINE_THICKNESS = 10f;

    public static readonly float SIN_60 = 0.8660254037844386467637231707529361834714026269051903140279034897f;

    public ShapeBatcher(Game game)
    {
        m_Game = game ?? throw new ArgumentNullException(nameof(game));
        m_Disposed = false;
        m_Effect = new BasicEffect(m_Game.GraphicsDevice)
        {
            TextureEnabled = false,
            FogEnabled = false,
            LightingEnabled = false,
            VertexColorEnabled = true,
            World = Matrix.Identity,
            View = Matrix.Identity,
            Projection = Matrix.Identity
        };

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

    public void DrawCircle(Vector2 center, float radius, int numVertices, float thickness, Color colour)
    {
        const int MIN_POINTS = 3;
        const int MAX_POINTS = 256;

        numVertices = Math.Clamp(numVertices, MIN_POINTS, MAX_POINTS);

        float deltaAngle = MathHelper.TwoPi / numVertices;
        float angle = 0f;

        for (int i = 0; i < numVertices; i++)
        {
            float ax = center.X + radius * MathF.Sin(angle);
            float ay = center.Y + radius * MathF.Cos(angle);

            angle += deltaAngle;

            float bx = center.X + radius * MathF.Sin(angle);
            float by = center.Y + radius * MathF.Cos(angle);
            DrawLine(new Vector2(ax, ay), new Vector2(bx, by), thickness, colour);
        }
    }

    public void DrawFilledCircle(Vector2 center, float radius, int numVertices, Color colour)
    {
        EnsureStarted();
        const int MIN_POINTS = 3;
        const int MAX_POINTS = 255;

        numVertices = Math.Clamp(numVertices, MIN_POINTS, MAX_POINTS);
        EnsureSpace(numVertices + 1, numVertices * 3);

        float deltaAngle = MathHelper.TwoPi / numVertices;
        float angle = 0f;
        Vector3 centerV3 = new(center.X, center.Y, 0f);

        // Arrange indices
        for (int i = 0; i < numVertices - 1; i++)
        {
            m_Indices[m_IndexCount++] = m_VertexCount;
            m_Indices[m_IndexCount++] = m_VertexCount + i + 1;
            m_Indices[m_IndexCount++] = m_VertexCount + i + 2;
        }

        m_Indices[m_IndexCount++] = m_VertexCount;
        m_Indices[m_IndexCount++] = m_VertexCount + numVertices;
        m_Indices[m_IndexCount++] = m_VertexCount + 1;

        m_Vertices[m_VertexCount++] = new VertexPositionColor(centerV3, colour);
        for (int i = 0; i < numVertices; i++)
        {
            Vector3 pos = new(MathF.Sin(angle), MathF.Cos(angle), 0);
            pos = (pos * radius) + centerV3;
            m_Vertices[m_VertexCount++] = new VertexPositionColor(pos, colour);

            angle += deltaAngle;
        }

        m_ShapeCount++;
    }

    public void DrawArrow(Vector2 start, Vector2 vector, float thickness, float arrowSize, Color colour)
    {
        Vector2 lineEnd = start + vector;

        Vector2 u = vector * (1f / vector.Length());
        Vector2 v = new(-u.Y, u.X);

        Vector2 arrowHead1 = lineEnd - arrowSize * u + arrowSize * v;
        Vector2 arrowHead2 = lineEnd - arrowSize * u - arrowSize * v;

        DrawLine(start, lineEnd, thickness, colour);
        DrawLine(lineEnd, arrowHead1, thickness, colour);
        DrawLine(lineEnd, arrowHead2, thickness, colour);
    }

    public void DrawPrimitives(VertexPositionColor[] vertices, int[] indices)
    {
        EnsureStarted();

        int numVertices = vertices.Length;
        int numIndices = indices.Length;
        EnsureSpace(numVertices, numIndices);

        // Copy indices
        Array.Copy(vertices, 0, m_Vertices, m_VertexCount, numVertices);

        for (int i = 0; i < numIndices; i++)
            m_Indices[m_IndexCount++] = indices[i] + m_VertexCount;

        m_VertexCount += numVertices;

        m_ShapeCount++;
    }
}