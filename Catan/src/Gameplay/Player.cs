using ImGuiNET;

namespace Catan;

public class Player
{
    public Resources.Collection Hand;

    public int Settlements = 5;
    public int Roads = 15;
    public int Cities = 4;

    public int VictoryPoints = 0;

    public Player()
    {
        Hand = new();
    }

    public void Update()
    {

    }

    public void ImDraw()
    {
        ImGui.SeparatorText("Hand");
        Hand.ImDraw();
    }
}