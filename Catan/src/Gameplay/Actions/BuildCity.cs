using System;
using System.Diagnostics.CodeAnalysis;
using Grid.Hexagonal;

namespace Catan.Action;

/// <summary>
/// Build a new city
/// </summary>
/// <remarks>
/// Logic: <see cref="GameState.BuildCity(int, Vertex.Key)"/><br/>
/// Phases: <see cref="TurnMain"/>
/// </remarks>
public class BuildCityAction : IAction
{
    /// <summary>
    /// Position for city
    /// </summary>
    public Vertex.Key Position;

    public BuildCityAction(int ownerID, Vertex.Key position)
    {
        OwnerID = ownerID;
        Position = position;
    }

    public override string ToString()
    {
        return string.Format("{0} build city", OwnerID);
    }

    public override string GetDescription()
    {
        return string.Format(
            "Player: {0}\n" +
            "Position: {1}",
            OwnerID, Position.ToString()
        );
    }

    public override bool Equals([NotNullWhen(true)] object obj)
    {
        if (obj is not BuildCityAction action)
            return false;
        
        return base.Equals(action) && action.Position == Position;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    protected override GameState DoExecute(GameState gameState)
    {
        Player player = gameState.Players[OwnerID];
        player.Settlements++;
        player.Cities--;
        player.VictoryPoints++;

        if (!gameState.Board.TryGetVertex(Position, out Node corner))
            throw new Exception();
        
        corner.City = true;

        IAction trade = new Trade(){
            OwnerID = OwnerID,
            TargetID = -1,
            Giving = Rules.CITY_COST,
            IsHidden = true
        };

        return trade.Execute(gameState);
    }
}