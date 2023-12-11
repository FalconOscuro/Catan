using Microsoft.Xna.Framework;

namespace Utility;

public struct Transform {
    public float Scale;
    
    public float Rotation;

    public Vector2 Translation;

    public readonly Vector2 Apply(Vector2 v)
    {
        return (v.Rotate(Rotation) * Scale) + Translation;
    }
}