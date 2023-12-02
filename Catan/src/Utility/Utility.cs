using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using ImGuiNET;

namespace Catan;

static class Utility
{
    public static void ShuffleArray<T>(this Random rand, T[] array, int iterations = 2)
    {
        int length = array.Length;

        for (int i = 0; i < iterations; i++)
        {
            int n = length;

            while (n > 1)
            {
                int pos = rand.Next(n--);

                (array[pos], array[n]) = (array[n], array[pos]);
            }
        }
    }

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

    public static Vector3 PreComputedRotate(this Vector2 vector, float sinT, float cosT)
    {
        return new Vector3(
            vector.X * cosT - vector.Y * sinT,
            vector.X * sinT + vector.Y * cosT,
            0f
        );
    }
}