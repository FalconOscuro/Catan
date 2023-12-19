using System;

namespace Catan;

// NOTES:
// Need to be able to simulate different dice roll results
// Potentially separate some of the logic from the gamestate?

/// <summary>
/// Roll Dice
/// </summary>
public class RollDiceAction : IAction
{
    protected override void DoExecute(GameState gameState)
    {
        (int, int) dice = gameState.RollDice();
        int roll = dice.Item1 + dice.Item2;

        gameState.LastRoll = roll;

        if (roll == 7)
        {
            // Discard
            // Else Robber

        }

        else
        {
            gameState.DistributeResources();
        }
    }
}