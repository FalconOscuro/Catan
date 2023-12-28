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

    /// <summary>
    /// ImGui debug tools
    /// </summary>
    public void ImDraw()
    {
        ImGui.Text(string.Format("VP: {0}", VictoryPoints));

        if(ImGui.TreeNode("Hand"))
        {
            Hand.ImDraw();
            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Pieces"))
        {
            ImGui.Text(string.Format("Settlements: {0}", Settlements));
            ImGui.Text(string.Format("Roads: {0}", Roads));
            ImGui.Text(string.Format("Cities: {0}", Cities));
            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Valid Actions"))
        {
            IAction.ImDrawActList(DMM.Actions, GetHashCode().ToString());
        }
    }
}