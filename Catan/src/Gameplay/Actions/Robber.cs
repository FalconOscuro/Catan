using System;
using Grid.Hexagonal;

namespace Catan.Action;

public class RobberAction : IAction
{
    public int OwnerID;

    public Axial TargetPos;

    public int TargetID;

    public override string ToString()
    {
        return $"{OwnerID} moves Robber";
    }

    public override string GetDescription()
    {
        return string.Format(
            "Player: {0}\n" +
            "Target: {1}\n" +
            "Pos: {2}",
            OwnerID, TargetID, TargetPos.ToString()
        );
    }

    protected override void DoExecute(GameState gameState)
    {
        gameState.MoveRobber(TargetPos, TargetID, OwnerID);
    }
}