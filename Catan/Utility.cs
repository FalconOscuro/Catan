using System;

using Microsoft.Xna.Framework;

namespace Catan;

static class Utility
{
    public static void ShuffleArray<T>(this Random rand, T[] array, int iterations = 1)
    {
        int length = array.Length;

        for (int i = 0; i < 2; i++)
        {
            int n = length;

            while (n > 1)
            {
                int pos = rand.Next(n--);

                T temp = array[n];
                array[n] = array[pos];
                array[pos] = temp;
            }
        }
    }

    public static Vector2 FlipY(this Vector2 vector, float screenHeight)
    {
        vector.Y = screenHeight * (1f - (vector.Y / screenHeight));
        return vector;
    }
}