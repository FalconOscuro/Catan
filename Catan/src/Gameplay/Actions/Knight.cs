using System.Diagnostics.CodeAnalysis;

namespace Catan.Action;

public class KnightAction : RobberAction
{
    public override string ToString()
    {
        return $"{OwnerID} plays knight";
    }

    public override bool Equals([NotNullWhen(true)] object obj)
    {
        return base.Equals(obj) && obj is KnightAction;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    protected override GameState DoExecute(GameState gameState)
    {
        gameState.SetDevCardToPlayed(DevCards.Type.Knight, OwnerID);
        gameState.Players[OwnerID].KnightsPlayed++;
        gameState.UpdateLargestArmy(OwnerID);

        return base.DoExecute(gameState);
    }
};