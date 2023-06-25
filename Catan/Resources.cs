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

    public bool TryTake(Resources resources)
    {
        return false;
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
            DisplayResource(modify, ref Lumber);

            ImGui.TableSetColumnIndex(1);
            DisplayResource(modify, ref Brick);

            ImGui.TableSetColumnIndex(2);
            DisplayResource(modify, ref Grain);

            ImGui.TableSetColumnIndex(3);
            DisplayResource(modify, ref Wool);

            ImGui.TableSetColumnIndex(4);
            DisplayResource(modify, ref Ore);

            ImGui.EndTable();
        }
    }

    private static void DisplayResource(bool modify, ref int resource)
    {
        ImGui.Text(string.Format("{0}", resource));

        if (!modify)
            return;

        ImGui.SameLine();
        if (ImGui.ArrowButton(resource.GetHashCode().ToString() + "Up", ImGuiDir.Up))
            resource++;
            
        ImGui.SameLine();
        if (ImGui.ArrowButton(resource.GetHashCode().ToString() + "Down", ImGuiDir.Down))
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