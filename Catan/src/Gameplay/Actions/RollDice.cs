using System;
using Grid.Hexagonal;

namespace Catan.Action;

// NOTES:
// Need to be able to simulate different dice roll results
// Potentially separate some of the logic from the gamestate?

/// <summary>
/// Roll Dice
/// </summary>
/// <remarks>
/// Logic: <see cref="GameState.RollDice"/><br/>
/// Phases: <see cref="TurnStart"/>
/// </remarks>
public class RollDiceAction : IAction
{
    public (int, int) Rolled;
    public int RolledSum { get { return Rolled.Item1 + Rolled.Item2; }}
    public bool TriggerRobber { get { return RolledSum == 7; }}

    public RollDiceAction()
    {
        TriggerStateChange = true;
    }

    public override string ToString()
    {
        return $"{OwnerID} rolls {RolledSum}";
    }

    public override string GetDescription()
    {
        return string.Format(
            "Player: {0}\n" +
            "Rolled: {1}\n" +
            "Sum: {2}\n" +
            "Robber: {3}",
            OwnerID, Rolled, RolledSum, TriggerRobber
        );
    }

    /// <summary>
    /// Executes <see cref="GameState.RollDice"/>.
    /// </summary>
    protected override GameState DoExecute(GameState gameState)
    {
        Rolled = gameState.RollDice();
        int rollSum = RolledSum;
        
        // On robber no resources distributed
        if (TriggerRobber)
            return gameState;
        
        Resources.Collection[] playerTrades = new Resources.Collection[Rules.NUM_PLAYERS];

        for (int i = 0; i < Rules.NUM_PLAYERS; i++)
            playerTrades[i] = new();

        // Get all distribution trades for roll
        foreach (var pos in gameState.TileValueMap[rollSum])
        {
            // Skip robber tile
            if (pos == gameState.RobberPos)
                continue;

            if (!gameState.Board.TryGetHex(pos, out Tile tile))
                throw new Exception(string.Format("Expected tile at (q={0}, r={1}), but found none!", pos.Q, pos.R));

            Resources.Type type = tile.Resource;

            // Skip if none of resource type available
            if (gameState.Bank[type] == 0)
                continue;

            for (Vertex.Key key = new(){Position = pos}; key.Side < Vertex.Side.SW + 1; key.Side++)
            {
                if (!gameState.Board.TryGetVertex(key, out Node corner))
                    throw new Exception(string.Format("Expected intersection at (q={0}, r{1}, side={2}), but found none!", pos.Q, pos.R, key.Side.ToString()));
                
                if (corner.OwnerID == -1)
                    continue;

                playerTrades[corner.OwnerID][type] += corner.City ? 2 : 1;
            }
        }

        // Get sum total
        Resources.Collection total = new();
        for (int i = 0; i < Rules.NUM_PLAYERS; i++)
            total += playerTrades[i];
        
        // There are no trades, exit function
        if (total.Count() == 0)
            return gameState;

        // Ensure trades can be executed
        for (Resources.Type type = Resources.Type.Brick; type < Resources.Type.Wool + 1; type++)
        {
            if (gameState.Bank[type] >= total[type])
                continue;
            
            // Check number of requesting players
            int firstPlayerIndex = -1;
            bool cannotSupply = false;
            for (int i = 0; i < Rules.NUM_PLAYERS; i++)
            {
                if(playerTrades[i][type] == 0)
                    continue;
                
                else if (firstPlayerIndex == -1)
                    firstPlayerIndex = i;
                
                else
                {
                    cannotSupply = true;
                    break;
                }
            }

            // A trade is cancelled if there is not enough supply,
            // unless only 1 player is requesting
            if (cannotSupply)
                for (int i = firstPlayerIndex; i < Rules.NUM_PLAYERS; i++)
                    playerTrades[i][type] = 0;
            
            else
                playerTrades[firstPlayerIndex][type] = gameState.Bank[type];
        }

        // Execute all trades
        for (int i = 0; i < Rules.NUM_PLAYERS; i++)
            if (playerTrades[i].Count() != 0)
            {
                IAction trade = new Trade(){
                    OwnerID = i,
                    TargetID = -1,
                    Receiving = playerTrades[i]
                };

                gameState = trade.Execute(gameState);
            }
        
        return gameState;
    }
}