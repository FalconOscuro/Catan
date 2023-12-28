using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;

namespace Catan;

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
    /// <summary>
    /// Execute action command
    /// </summary>
    /// <remarks>
    /// Calls implementation specific command (<see cref="DoExecute"/>),
    /// followed by generic action logic (<see cref="UpdatePhase"/>).
    /// </remarks>
    public void Execute(GameState gameState)
    {
        DoExecute(gameState);

        //gameState.PlayedActions.Add(this);
        gameState.UpdatePhase(this);
    }

    /// <summary>
    /// Get short description for action
    /// </summary>
    /// <remarks>
    /// TODO: Long description
    /// </remarks>
    public override abstract string ToString();

    /// <summary>
    /// Command specified by implementation
    /// </summary>
    /// <remarks>
    /// Should execute a Function of <paramref name="gameState"/>
    /// </remarks>
    protected abstract void DoExecute(GameState gameState);

    /// <summary>
    /// ImGUI drawing for a list of actions.
    /// </summary>
    /// <param name="str_id">ID used by ImGUI.</param>
    public static void ImDrawActList(List<IAction> actions, string str_id)
    {
        uint id = ImGui.GetID(str_id);
        if(ImGui.BeginChild(id, new Vector2(ImGui.GetContentRegionAvail().X, 100f), true))
        {
            for (int i = actions.Count - 1; i >= 0; i--)
                ImGui.Text(actions[i].ToString());
        }
        ImGui.EndChild();
    }
}