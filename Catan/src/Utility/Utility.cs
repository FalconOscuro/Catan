using System;

using Microsoft.Xna.Framework;

using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Utility;

static class Utility
{
    public static void HelpMarker(string description)
    {
        ImGui.TextDisabled("(?)");
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35f);
            ImGui.TextUnformatted(description);
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
        }
    }

    public static Vector2 FlipY(this Vector2 vector, float screenHeight)
    {
        vector.Y = screenHeight * (1f - (vector.Y / screenHeight));
        return vector;
    }

    public static Vector2 FlipY(this Point point, float screenHeight)
    {
        return new Vector2(point.X, screenHeight * (1f - (point.Y / screenHeight)));
    }

    public static float NextFloat(this Random rand, float max = 1f, float min = 0f)
    {
        return (rand.NextSingle() * (max - min)) + min;
    }

    public static bool NextBool(this Random rand, float fraction = .5f)
    {
        return rand.NextSingle() < fraction;
    }

    public static Vector2 PreComputedRotate(this Vector2 vector, float sinT, float cosT)
    {
        return new Vector2(
            vector.X * cosT - vector.Y * sinT,
            vector.X * sinT + vector.Y * cosT
        );
    }

    public static Vector2 Rotate(this Vector2 vector, float r)
    {
        float sin = MathF.Sin(r);
        float cos = MathF.Cos(r);

        return new Vector2(
            vector.X * cos - vector.Y * sin,
            vector.X * sin + vector.Y * cos
        );
    }

    public static Vector3 ToVector3(this Vector2 vector)
    {
        return new(vector.X, vector.Y, 0f);
    }

    public static Vector2 GetSizeVec(this Viewport viewport)
    {
        return new(viewport.Width, viewport.Height);
    }

    // Fisher-Yates Shuffle
    public static void Shuffle<T>(this Random rand, T[] array)
    {
        int n = array.Length;
        while (n > 1)
        {
            int k = rand.Next(n--);
            (array[n], array[k]) = (array[k], array[n]);
        }
    }
}