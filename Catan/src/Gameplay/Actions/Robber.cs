using System.Diagnostics.CodeAnalysis;
using Grid.Hexagonal;

namespace Catan.Action;

public class RobberAction : IAction
{
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

    public override bool Equals([NotNullWhen(true)] object obj)
    {
        if (obj is not RobberAction action)
            return false;

        return base.Equals(obj) && action.TargetPos == TargetPos && action.TargetID == TargetID;
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

        IAction steal = new Steal(){
            OwnerID = OwnerID,
            TargetID = TargetID
        };

        return steal.Execute(gameState);
    }
}

public class Steal : IAction
{
    public int TargetID;

    public Resources.Type Stolen;

    public Steal()
    {
        IsHidden = true;
    }

    public override string ToString()
    {
        return $"{OwnerID} steals from {TargetID}";
    }

    public override bool Equals([NotNullWhen(true)] object obj)
    {
        if (obj is not Steal action)
            return false;

        return base.Equals(obj) && action.TargetID == TargetID && action.Stolen == Stolen;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string GetDescription()
    {
        return string.Format(
            "Player: {0}\n" +
            "Target: {1}\n" +
            "Stolen: {2}\n",
            OwnerID, TargetID, Stolen
        );
    }

    protected override GameState DoExecute(GameState gameState)
    {
        Player targetPlayer = gameState.Players[TargetID];
        Player ownerPlayer = gameState.Players[OwnerID];

        // Steal random resource
        int targetHandSize = targetPlayer.Hand.Count();

        // No cards to steal
        if (targetHandSize < 1)
            return gameState;
        
        int stolenCard = gameState.Random.Next(targetHandSize) + 1;

        Resources.Type type = Resources.Type.Empty;
        int count = 0;

        // Find type of stolen card
        while (stolenCard > count)
        {
            type ++;
            count += targetPlayer.Hand[type];
        }

        targetPlayer.Hand[type] -= 1;
        ownerPlayer.Hand[type] += 1;
        return gameState;
    }
}