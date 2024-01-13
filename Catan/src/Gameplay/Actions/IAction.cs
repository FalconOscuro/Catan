using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using ImGuiNET;

namespace Catan.Action;

// Self evaluation of actions?

/// <summary>
/// Structure for Command pattern of actions executable by a player
/// </summary>
/// <remarks>
/// All actions should be distributed by the <see cref="GamePhaseManager"/>,
/// for execution by the <see cref="Player.DMM"/>.
/// </remarks>
public abstract class IAction
{
    public bool IsSilent = false;
    public bool IsHidden = false;
    public bool TriggerStateChange = false;

    public int OwnerID;

    /// <summary>
    /// Execute action command
    /// </summary>
    /// <remarks>
    /// Calls implementation specific command (<see cref="DoExecute"/>),
    /// followed by generic action logic (<see cref="UpdatePhase"/>).
    /// </remarks>
    public GameState Execute(GameState gameState)
    {
        gameState = DoExecute(gameState);

        gameState.UpdatePhase((IAction)MemberwiseClone());
        return gameState;
    }

    /// <summary>
    /// Get short description for action
    /// </summary>
    /// <remarks>
    /// TODO: Long description
    /// </remarks>
    public override abstract string ToString();

    public abstract string GetDescription();

    public override bool Equals([NotNullWhen(true)] object obj)
    {
        if (obj is not IAction action)
            return false;

        return action.OwnerID == OwnerID;
    }

    public override int GetHashCode()
    {
        return GetDescription().GetHashCode();
    }

    /// <summary>
    /// Command specified by implementation
    /// </summary>
    /// <remarks>
    /// Should execute a Function of <paramref name="gameState"/>
    /// </remarks>
    protected abstract GameState DoExecute(GameState gameState);

    public virtual IAction Clone()
    {
        return (IAction)MemberwiseClone();
    }

    private static int s_SelectedActionIndex = 0;

    /// <summary>
    /// ImGUI drawing for a list of actions.
    /// </summary>
    /// <param name="str_id">ID used by ImGUI.</param>
    public static void ImDrawActList(List<IAction> actions, string str_id)
    {
        float spacing = ImGui.GetTextLineHeightWithSpacing();
        Vector2 size = new(ImGui.GetContentRegionAvail().X / 2f, 8 * spacing);
        if (ImGui.BeginListBox("##"+str_id, size))
        {
            for (int i = 0; i < actions.Count; i++)
            {
                bool isSelected = i == s_SelectedActionIndex;

                if(ImGui.Selectable($"{i}: {actions[i]}", isSelected))
                {
                    s_SelectedActionIndex = i;
                    isSelected = true;
                }

                if (isSelected)
                    ImGui.SetItemDefaultFocus();
            }

            ImGui.EndListBox();

            ImGui.SameLine();
            string text = actions.Count > s_SelectedActionIndex ? actions[s_SelectedActionIndex].GetDescription() : "";
            ImGui.InputTextMultiline("##"+str_id, ref text, 1024, size, ImGuiInputTextFlags.ReadOnly);
        }
    }

    public static bool operator==(IAction a, IAction b)
    {
        return a.Equals(b);
    }

    public static bool operator!=(IAction a, IAction b)
    {
        return !a.Equals(b);
    }
}