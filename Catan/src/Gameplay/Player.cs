using System.Collections.Generic;
using System.Numerics;
using System.Reflection.Metadata;
using Catan.Behaviour;
using ImGuiNET;

namespace Catan;

public class Player
{
    // NOTE: Should be hidden to other players
    public Resources.Collection Hand;

    // Could change to count No of owned instead of No remaining?
    public int Settlements = Rules.MAX_SETTLEMENTS;
    public int Roads = Rules.MAX_ROADS;
    public int Cities = Rules.MAX_CITIES;

    public int VictoryPoints = 0;

    // NOTE: Should be hidden to other players
    public List<DevCards.Type> HeldDevCards;
    public bool HasPlayedDevCard = false;

    public int KnightsPlayed = 0;
    public bool LargestArmy = false;

    // TEMP DEBUG, REMOVE!!!!
    public DMM DMM = new RandomDMM();

    public int ID {get; private set;}

    public Player(int id)
    {
        Hand = new();
        HeldDevCards = new();
        ID = id;
    }

    // NOTE: Other players should not be able to see hidden VP from Dev cards!
    public int GetTotalVP()
    {
        int hiddenVP = 0;
        foreach (DevCards.Type devCard in HeldDevCards)
            if (devCard == DevCards.Type.VictoryPoint)
                hiddenVP++;

        return VictoryPoints + hiddenVP + 
            (LargestArmy ? 2 : 0);
    }

    /// <summary>
    /// ImGui debug tools
    /// </summary>
    public void ImDraw()
    {
        ImGui.TextColored(Rules.GetPlayerIDColour(ID).ToVector4().ToNumerics(), "Colour");
        ImGui.Text(string.Format("VP: {0}", GetTotalVP()));
        ImGui.Text($"Knights Played: {KnightsPlayed}");
        ImGui.Text($"Largest Army: {LargestArmy}");

        if(ImGui.TreeNode("Hand"))
        {
            Hand.ImDraw();
            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Development Cards"))
        {
            ImGui.BeginListBox($"##Player {ID} DevCards", new Vector2(ImGui.GetContentRegionAvail().X / 2, ImGui.GetTextLineHeightWithSpacing() * 4));
            for (int i = 0; i < HeldDevCards.Count; i++)
                ImGui.Selectable($"{i}: {HeldDevCards[i]}");

            ImGui.EndListBox();
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
            Action.IAction.ImDrawActList(DMM.Actions, $"Player {ID} Actions");
            ImGui.TreePop();
        }
    }
}