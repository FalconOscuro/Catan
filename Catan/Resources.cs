using System;

using Microsoft.Xna.Framework;

using ImGuiNET;

namespace Catan;

struct Resources
{
    public Resources()
    {}

    public Resources(int lumber, int brick, int grain, int wool, int ore)
    {
        Lumber = lumber;
        Brick = brick;
        Grain = grain;
        Wool = wool;
        Ore = ore;
    }

    public int Lumber = 0;
    public int Brick = 0;
    public int Grain = 0;
    public int Wool = 0;
    public int Ore = 0;

    public readonly int GetType(Type type)
    {
        return type switch
        {
            Type.Lumber => Lumber,
            Type.Brick => Brick,
            Type.Grain => Grain,
            Type.Wool => Wool,
            Type.Ore => Ore,
            _ => 0,
        };
    }

    public void SetType(Type type, int num)
    {
        switch (type)
        {
        case Type.Lumber:
            Lumber = num;
            break;

        case Type.Brick:
            Brick = num;
            break;

        case Type.Grain:
            Grain = num;
            break;

        case Type.Wool:
            Wool = num;
            break;

        case Type.Ore:
            Ore = num;
            break;
        }
    }

    public void AddType(Type type, int amount)
    {
        switch (type)
        {
        case Type.Lumber:
            Lumber += amount;
            break;

        case Type.Brick:
            Brick += amount;
            break;

        case Type.Grain:
            Grain += amount;
            break;

        case Type.Wool:
            Wool += amount;
            break;

        case Type.Ore:
            Ore += amount;
            break;
        }
    }

    public Type Steal()
    {
        int total = GetTotal();
        if (total == 0)
            return Type.Empty;
        
        Random rand = new();
        int target = rand.Next(total) + 1;

        if (FindTarget(ref Lumber, ref target))
            return Type.Lumber;
        
        else if (FindTarget(ref Brick, ref target))
            return Type.Brick;
        
        else if (FindTarget(ref Grain, ref target))
            return Type.Grain;
        
        else if (FindTarget(ref Wool, ref target))
            return Type.Wool;

        else if (FindTarget(ref Ore, ref target))
            return Type.Ore;
        
        return Type.Empty;
    }

    private static bool FindTarget(ref int resourceCount, ref int target)
    {
        if (target <= resourceCount)
        {
            resourceCount--;
            return true;
        }
        
        target -= resourceCount;
        return false;
    }

    public void Add(Resources resources)
    {
        Lumber = MathHelper.Max(Lumber + resources.Lumber, 0);
        Brick = MathHelper.Max(Brick + resources.Brick, 0);
        Grain = MathHelper.Max(Grain + resources.Grain, 0);
        Wool = MathHelper.Max(Wool + resources.Wool, 0);
        Ore = MathHelper.Max(Ore + resources.Ore, 0);
    }

    public bool TryTake(Resources resources)
    {
        if (resources > this)
            return false;

        Lumber -= resources.Lumber;
        Brick -= resources.Brick;
        Grain -= resources.Grain;
        Wool -= resources.Wool;
        Ore -= resources.Ore;
        return true;
    }

    public readonly int GetTotal()
    {
        return Lumber + Brick + Grain + Wool + Ore;
    }

    public void UIDraw(bool modify = false)
    {
        if (ImGui.BeginTable("Resources", 5))
        {
            ImGui.TableNextRow();
            for (int i = 0; i < 5; i++)
            {
                ImGui.TableSetColumnIndex(i);
                ImGui.Text(((Type)i).ToString());
            }

            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            DisplayResource(modify, Type.Lumber.ToString(), ref Lumber);

            ImGui.TableSetColumnIndex(1);
            DisplayResource(modify, Type.Brick.ToString(), ref Brick);

            ImGui.TableSetColumnIndex(2);
            DisplayResource(modify, Type.Grain.ToString(), ref Grain);

            ImGui.TableSetColumnIndex(3);
            DisplayResource(modify, Type.Wool.ToString(), ref Wool);

            ImGui.TableSetColumnIndex(4);
            DisplayResource(modify, Type.Ore.ToString(), ref Ore);

            ImGui.EndTable();
        }
    }

    private static void DisplayResource(bool modify, string name, ref int resource)
    {
        ImGui.Text(string.Format("{0}", resource));

        if (!modify)
            return;

        ImGui.SameLine();
        if (ImGui.ArrowButton(name + "Up", ImGuiDir.Up))
            resource++;
            
        ImGui.SameLine();
        if (ImGui.ArrowButton(name + "Down", ImGuiDir.Down) && resource > 0)
            resource--;
    }

    public static Resources operator+(Resources a, Resources b)
    {
        return new Resources(
            MathHelper.Max(a.Lumber + b.Lumber, 0),
            MathHelper.Max(a.Brick + b.Brick, 0),
            MathHelper.Max(a.Grain + b.Grain, 0),
            MathHelper.Max(a.Wool + b.Wool, 0),
            MathHelper.Max(a.Ore + b.Ore, 0)
            );
    }

    public static Resources operator-(Resources a, Resources b)
    {
        return new Resources(
            MathHelper.Max(a.Lumber - b.Lumber, 0),
            MathHelper.Max(a.Brick - b.Brick, 0),
            MathHelper.Max(a.Grain - b.Grain, 0),
            MathHelper.Max(a.Wool - b.Wool, 0),
            MathHelper.Max(a.Ore - b.Ore, 0)
        );
    }

    public static Resources operator*(Resources a, Resources b)
    {
        return new Resources(
            a.Lumber * b.Lumber,
            a.Brick * b.Brick,
            a.Grain * b.Grain,
            a.Wool * b.Wool,
            a.Ore * b.Ore
        );
    }

    public static bool operator<=(Resources a, Resources b)
    {
        return a.Lumber <= b.Lumber && a.Brick <= b.Brick 
            && a.Grain <= b.Grain && a.Lumber <= b.Lumber && a.Ore <= b.Ore;
    }

    public static bool operator>=(Resources a, Resources b)
    {
        return a.Lumber >= b.Lumber && a.Brick >= b.Brick 
            && a.Grain >= b.Grain && a.Lumber >= b.Lumber && a.Ore >= b.Ore;
    }

    public static bool operator<(Resources a, Resources b)
    {
        return a.Lumber < b.Lumber || a.Brick < b.Brick 
            || a.Grain < b.Grain || a.Lumber < b.Lumber || a.Ore < b.Ore;
    }

    public static bool operator>(Resources a, Resources b)
    {
        return a.Lumber > b.Lumber || a.Brick > b.Brick 
            || a.Grain > b.Grain || a.Lumber > b.Lumber || a.Ore > b.Ore;
    }

    public static Color GetResourceColour(Type resource)
    {
        return resource switch
        {
            Type.Empty => Color.Wheat,
            Type.Lumber => Color.DarkGreen,
            Type.Brick => Color.Brown,
            Type.Grain => Color.Goldenrod,
            Type.Wool => Color.LightGreen,
            Type.Ore => Color.Gray,
            _ => Color.Black,
        };
    }

    public override string ToString()
    {
        string asString = "[";

        if (Lumber != 0)
            asString += string.Format("L:{0},", Lumber);
        
        if (Brick != 0)
            asString += string.Format("B:{0},", Brick);
        
        if (Grain != 0)
            asString += string.Format("G:{0},", Grain);
        
        if (Wool != 0)
            asString += string.Format("W:{0},", Wool);
        
        if (Ore != 0)
            asString += string.Format("O:{0},", Ore);
        

        return asString.TrimEnd(',') + "]";
    }

    public enum Type
    {
        Empty = -1,
        Lumber,
        Brick,
        Grain,
        Wool,
        Ore
    }
}

struct ResourceWeights
{
    public ResourceWeights()
    {
        Lumber = 1f;
        Brick = 1f;
        Grain = 1f;
        Wool = 1f;
        Ore = 1f;
    }

    public ResourceWeights(float lumber, float brick, float grain, float wool, float ore)
    {
        Lumber = lumber;
        Brick = brick;
        Grain = grain;
        Wool = wool;
        Ore = ore;
    }

    public readonly float GetResourcesWeight(Resources resources)
    {
        return Lumber * resources.Lumber +
            Brick * resources.Brick +
            Grain * resources.Grain +
            Wool * resources.Wool +
            Ore * resources.Ore;
    }

    public readonly float GetResourceWeight(Resources.Type type)
    {
        return type switch
        {
            (Resources.Type.Lumber) => Lumber,
            (Resources.Type.Brick) => Brick,
            (Resources.Type.Grain) => Grain,
            (Resources.Type.Wool) => Wool,
            (Resources.Type.Ore) => Ore,
            _ => 1f,
        };
    }

    public void AddResourceType(Resources.Type type, float n)
    {
        switch (type)
        {
        case Resources.Type.Lumber:
            Lumber += n;
            break;
        
        case Resources.Type.Brick:
            Brick += n;
            break;

        case Resources.Type.Grain:
            Grain += n;
            break;

        case Resources.Type.Wool:
            Wool += n;
            break;
        
        case Resources.Type.Ore:
            Ore += n;
            break;
        }
    }

    public readonly float Sum()
    {
        return Lumber + Brick + Grain + Wool + Ore;
    }

    public static readonly ResourceWeights Zero = new(0, 0, 0, 0, 0);

    public float Lumber;
    public float Brick;
    public float Grain;
    public float Wool;
    public float Ore;
}