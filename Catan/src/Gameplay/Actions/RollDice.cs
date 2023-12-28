using System;

namespace Catan;

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
    public override string ToString()
    {
        return "Dice Roll"; // Should display result
    }

    /// <summary>
    /// Executes <see cref="GameState.RollDice"/>.
    /// </summary>
    protected override void DoExecute(GameState gameState)
    {
        (int, int) dice = gameState.RollDice();
        int roll = dice.Item1 + dice.Item2;

        gameState.LastRoll = roll; // TODO: Remove logic from command
    }
}