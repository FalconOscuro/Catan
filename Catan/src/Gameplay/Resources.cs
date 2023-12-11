using System.Reflection.Metadata;
using ImGuiNET;
using Microsoft.Xna.Framework;

namespace Catan;

public class Resources
{
    /// <summary>
    /// Different types of resources
    /// Empty resembles the desert
    /// </summary>
    public enum Type
    {
        Empty,
        Brick,
        Grain,
        Lumber,
        Ore,
        Wool
    }

    /// <summary>
    /// A collection of resource card counts
    /// </summary>
    public struct Collection
    {
        public Collection()
        {}

        public void ImDraw()
        {
            for (Type type = Type.Brick; type < Type.Wool + 1; type++)
                ImGui.Text(string.Format("{0}: {1}", type.ToString(), this[type]));
        }

        public Collection Clone()
        {
            Collection clone = new();
            m_Resources.CopyTo(clone.m_Resources, 0);

            return clone;
        }

        public readonly int Brick
        {
            get { return m_Resources[(int)Type.Brick - 1]; }
            set { m_Resources[(int)Type.Brick - 1] = value; }
        }

        public readonly int Grain
        {
            get { return m_Resources[(int)Type.Grain - 1]; }
            set { m_Resources[(int)Type.Grain - 1] = value; }
        }

        public readonly int Lumber
        {
            get { return m_Resources[(int)Type.Lumber - 1]; }
            set { m_Resources[(int)Type.Lumber - 1] = value; }
        }

        public readonly int Ore
        {
            get { return m_Resources[(int)Type.Ore - 1]; }
            set { m_Resources[(int)Type.Ore - 1] = value; }
        }

        public readonly int Wool
        {
            get { return m_Resources[(int)Type.Wool - 1]; }
            set { m_Resources[(int)Type.Wool - 1] = value; }
        }

        /// <summary>
        /// Indexer by resource type
        /// </summary>
        public readonly int this[Type type]
        {
            get {return type == Type.Empty ? 0 : m_Resources[(int)type - 1]; }
            set 
            {
                if (type == Type.Empty)
                    return;
                
                m_Resources[(int)type - 1] = value;
            }
        }

        public static Collection operator+(Collection lhs, Collection rhs)
        {
            Collection result = new();

            for (Type type = Type.Brick; type < Type.Wool + 1; type++)
                result[type] = lhs[type] + rhs[type];
            
            return result;
        }

        public static Collection operator-(Collection lhs, Collection rhs)
        {
            Collection result = new();

            for (Type type = Type.Brick; type < Type.Wool + 1; type++)
                result[type] = lhs[type] - rhs[type];
            
            return result;
        }

        public static bool operator>=(Collection lhs, Collection rhs)
        {
            bool result = true;

            for (Type type = Type.Brick; type < Type.Wool + 1; type++)
                result &= lhs[type] >= rhs[type];

            return result;
        }

        public static bool operator<=(Collection lhs, Collection rhs)
        {
            bool result = true;

            for (Type type = Type.Brick; type < Type.Wool + 1; type++)
                result &= lhs[type] <= rhs[type];
            
            return result;
        }

        /// <summary>
        /// Storing resources in an array removes the need to use switch statements
        /// but results in repetitive getter/setters if you want to access a resource as a variable instead of parsing the type enum
        /// </summary>
        /// <value>Brick, Grain, Lumber, Ore, Wool</value>
        private readonly int[] m_Resources = {0, 0, 0, 0, 0};
    }

    /// <summary>
    /// Get colour associated to resource type
    /// </summary>
    public static Color GetColour(Type type)
    {
        switch(type)
        {
            case Type.Brick:
                return Color.Maroon;
            
            case Type.Grain:
                return Color.Yellow;

            case Type.Lumber:
                return Color.DarkGreen;

            case Type.Ore:
                return Color.Gray;

            case Type.Wool:
                return Color.LightGreen;

            default:
                return Color.Orange;
        }
    }
}