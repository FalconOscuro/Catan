using System.Diagnostics.CodeAnalysis;
using Grid.Hexagonal;

namespace Catan.Action;

public class RobberAction : IAction
{
    public Axial TargetPos;

    public int TargetID;

    public Resources.Type Stolen;

    public override string ToString()
    {
        return $"{OwnerID} moves Robber";
    }

    public override string GetDescription()
    {
        return string.Format(
            "Player: {0}\n" +
            "Target: {1}\n" +
            "Stolen: {3}\n" +
            "Pos: {2}",
            OwnerID, TargetID, TargetPos.ToString(), Stolen
        );
    }

    public override bool Equals([NotNullWhen(true)] object obj)
    {
        if (obj is not RobberAction action)
            return false;

        return base.Equals(obj) && action.TargetPos == TargetPos && action.TargetID == TargetID && (action.Stolen == Stolen || action.Stolen == Resources.Type.Empty || Stolen == Resources.Type.Empty);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    protected override GameState DoExecute(GameState gameState)
    {
        gameState.MoveRobber(TargetPos);

        // No resource stolen
        if (TargetID == -1)
            return gameState;

        Player targetPlayer = gameState.Players[TargetID];
        Player ownerPlayer = gameState.Players[OwnerID];

        // Steal random resource
        int targetHandSize = targetPlayer.Hand.Count();

        // No cards to steal
        if (targetHandSize < 1)
            return gameState;

        // Used for simulation
        if (Stolen == Resources.Type.Empty)
        {        
            int stolenCard = gameState.Random.Next(targetHandSize) + 1;

            Resources.Type type = Resources.Type.Empty;
            int count = 0;

            // Find type of stolen card
            while (stolenCard > count)
            {
                type ++;
                count += targetPlayer.Hand[type];
            }

            Stolen = type;
        }

        targetPlayer.Hand[Stolen] -= 1;
        ownerPlayer.Hand[Stolen] += 1;

        return gameState;
    }
}