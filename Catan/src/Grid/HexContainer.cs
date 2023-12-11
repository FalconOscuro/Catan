using System;
using Microsoft.Xna.Framework;
using Utility;
using Utility.Graphics;

namespace Grid.Hexagonal;
using static Utility;

internal class HexContainer
{
    public HexContainer(Axial axial)
    {
        Position = axial;
    }

    public Axial Position;

    public Hex Hex = null;

    public readonly Edge[] Edges = new Edge[3];

    public readonly Corner[] Corners = new Corner[2];

    public HexContainer Clone()
    {
        HexContainer clone = new(Position){
            Hex = Hex == null ? null : (Hex)Hex.Clone()
        };

        // Clone edges
        for (int i = 0; i < 3; i++)
            clone.Edges[i] = Edges[i] == null ? null : (Edge)Edges[i].Clone();

        // Clone Corners
        for (int i = 0; i < 2; i++)
            clone.Corners[i] = Corners[i] == null ? null : (Corner)Corners[i].Clone();

        return clone;
    }

    public void Draw(Transform transform, Canvas canvas)
    {
        Vector2 realPos = Position.GetRealPos();

        DrawHex(realPos, transform, canvas);
        DrawEdges(realPos, transform, canvas);
        DrawCorners(realPos, transform, canvas);
    }

    private void DrawHex(Vector2 realPos, Transform transform, Canvas canvas)
    {
        if (Hex != null)
        {
            Transform hexTransform = new(){
                Rotation = transform.Rotation,
                Scale = transform.Scale * 0.9f,
                Translation = transform.Apply(realPos)
            };

            Hex.Draw(hexTransform, canvas);
        }
    }

    private void DrawEdges(Vector2 realPos, Transform transform, Canvas canvas)
    {
        Vector2 offset = new(0, 0.5f);
        float deltaRot = -MathF.PI / 3f;

        Transform edgeTransform = new(){
                Scale = transform.Scale * INVERSE_SQRT_3 * 0.4f,
                Rotation = -deltaRot + transform.Rotation,
                };

        foreach (Edge edge in Edges)
        {
            if (edge != null)
            {
                edgeTransform.Translation = transform.Apply(realPos + 
                    offset.Rotate(edgeTransform.Rotation - transform.Rotation));
                
                edge.Draw(edgeTransform, canvas);
            }

            edgeTransform.Rotation += deltaRot;
        }
    }

    private void DrawCorners(Vector2 realPos, Transform transform, Canvas canvas)
    {
        Vector2 offset = new(-INVERSE_SQRT_3, 0);
        foreach (Corner corner in Corners)
        {
            if (corner != null)
            {
                Transform cornerTransform = new(){
                    Scale = transform.Scale * 0.05f,
                    Translation = transform.Apply(realPos + offset)
                };

                corner.Draw(cornerTransform, canvas);
            }

            offset.X = -offset.X;
        }
    }
}