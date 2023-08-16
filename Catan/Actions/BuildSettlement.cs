namespace Catan;

class BuildSettlement : Action
{
    // Need to target a specific node
    public BuildSettlement(int playerID, int targetNodeID):
        base(playerID)
    {
        TargetNodeID = targetNodeID;
    }

    public override bool TryCheckPreConditions(Catan.GameState gameState)
    {
        return gameState.BoardState.Nodes[TargetNodeID].IsAvailable(gameState.IsPregame() ? -1 : PlayerID)
            && gameState.PlayerStates[PlayerID].UnbuiltSettlements > 0;
    }

    public override bool CheckPreConditions(Catan.GameState gameState)
    {
        return TryCheckPreConditions(gameState) &&
            (gameState.PlayerStates[PlayerID].HeldResources > Player.SETTLEMENT_COST || 
                (gameState.IsPregame() && 
                    gameState.PlayerStates[PlayerID].UnbuiltSettlements > 
                        (gameState.Phase == Catan.State.Pregame1 ? 4 : 5))); // Check if pregame settlement has already been built
    }

    public override Catan.GameState TryCheckResult(Catan.GameState gameState)
    {
        Catan.GameState newState = (Catan.GameState)gameState.Clone();

        newState.BoardState.Nodes[TargetNodeID].OwnerID = PlayerID;

        newState.PlayerStates[PlayerID].VictoryPoints++;
        newState.PlayerStates[PlayerID].UnbuiltSettlements--;

        if (newState.Phase == Catan.State.Pregame2)
        {
            for (int i = 0; i < 3; i++)
            {
                Tile tile = newState.BoardState.Nodes[TargetNodeID].GetTile(i);

                if (tile == null)
                    continue;

                newState.PlayerStates[PlayerID].HeldResources.AddType(tile.Type, 1);
                newState.BoardState.ResourceBank.AddType(tile.Type, -1);
            }
        }

        return base.TryCheckResult(newState);
    }

    public override Catan.GameState CheckResult(Catan.GameState gameState)
    {
        Catan.GameState newState = (Catan.GameState)gameState.Clone();

        if (newState.Phase == Catan.State.Main)
        {
            newState.PlayerStates[PlayerID].HeldResources.TryTake(Player.SETTLEMENT_COST);
            newState.BoardState.ResourceBank.Add(Player.SETTLEMENT_COST);
        }

        return TryCheckResult(newState);
    }

    public int TargetNodeID { get; private set; }
}