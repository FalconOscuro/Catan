using System;

using Microsoft.Xna.Framework;

using ImGuiNET;

namespace Catan;

struct Resources
{
    public Resources(int lumber, int brick, int grain, int wool, int ore)
    {
        Lumber = lumber;
        Brick = brick;
        Grain = grain;
        Wool = wool;
        Ore = ore;
    }

    public int Lumber;
    public int Brick;
    public int Grain;
    public int Wool;
    public int Ore;

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
        
        Random rand = new Random();
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

    public bool TryTake(Resources resources)
    {
        if (resources.Lumber > Lumber || resources.Brick > Brick || resources.Grain > Grain
            || resources.Wool > Wool || resources.Ore > Ore)
            return false;

        Lumber -= resources.Lumber;
        Brick -= resources.Brick;
        Grain -= resources.Grain;
        Wool -= resources.Wool;
        Ore -= resources.Ore;
        return true;
    }

    public int GetTotal()
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

    public static Color GetResourceColour(Type resource)
    {
        switch(resource)
        {
            case Type.Empty:
                return Color.Wheat;

            case Type.Lumber:
                return Color.DarkGreen;

            case Type.Brick:
                return Color.Brown;

            case Type.Grain:
                return Color.Goldenrod;
            
            case Type.Wool:
                return Color.LightGreen;
            
            case Type.Ore:
                return Color.Gray;
        }

        return Color.Black;
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