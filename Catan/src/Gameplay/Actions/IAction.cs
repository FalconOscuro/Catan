using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;

namespace Catan;

/// <summary>
/// Structure for Command patter of actions executable by a player
/// </summary>
public abstract class IAction
{
    public void Execute(GameState gameState)
    {
        DoExecute(gameState);

        //gameState.PlayedActions.Add(this);
        gameState.UpdatePhase(this);
    }

    public override abstract string ToString();

    protected abstract void DoExecute(GameState gameState);

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