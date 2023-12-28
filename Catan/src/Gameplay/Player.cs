using Catan.Behaviour;
using ImGuiNET;

namespace Catan;

public class Player
{
    public Resources.Collection Hand;

    // Could change to count No of owned instead of No remaining?
    public int Settlements = Rules.MAX_SETTLEMENTS;
    public int Roads = Rules.MAX_ROADS;
    public int Cities = Rules.MAX_CITIES;

    public int VictoryPoints = 0;

    // TEMP DEBUG, REMOVE!!!!
    public DMM DMM = new RandomDMM();

    public Player()
    {
        Hand = new();
    }

    public void ImDraw()
    {
        ImGui.SeparatorText("Hand");
        Hand.ImDraw();
    }
}