using System;
using System.Collections.Generic;

namespace Catan.State;
using Type = Resources.Type;

public class Robber : IGamePhase
{
    public void OnEnter(params object[] argn)
    {}

    public void OnExit()
    {}

    public List<Action.IAction> GetValidActions(GameState gameState)
    {
        return new();
    }

    public void Update(GameState gameState, Action.IAction lastAction)
    {}

    public const string NAME = "Robber";
}

public class Discard : IGamePhase
{
    public void OnEnter(params object[] argn)
    {
        GameState gameState = argn[0] as GameState;

        FindTarget(gameState);
    }

    public void OnExit()
    {}

    public List<Action.IAction> GetValidActions(GameState gameState)
    {
        List<Action.IAction> actions = new();
        int playerID = gameState.GetCurrentPlayer();

        Resources.Collection hand = gameState.Players[playerID].Hand;

        // Half hand size rounded down
        int discardNum = Math.DivRem(hand.Count(), 2).Quotient;
        Resources.Collection discarding = new();
        
        foreach (var found in RecurseOptions(hand, new Resources.Collection(), discardNum))
        {
            Action.Trade trade = new(){
                OwnerID = playerID,
                TargetID = -1,
                Giving = found.Clone()
            };

            actions.Add(trade);
        }

        return actions;
    }

    /// <summary>
    /// Recursively finds all discard combinations with a given hand
    /// </summary>
    /// <param name="hand">hand for owning player</param>
    /// <param name="current">current resource collection being discarded</param>
    /// <param name="targetSum">Cards remaining to be discarded</param>
    /// <param name="index">index for current resource</param>
    /// <returns></returns>
    private IEnumerable<Resources.Collection> RecurseOptions(Resources.Collection hand, Resources.Collection current, int targetSum, Type index = Type.Brick)
    {
        for (; index < Type.Wool + 1; index++)
        {
            if (hand[index] == 0)
                continue;
            
            int diff = Math.Min(targetSum, hand[index]);
            current[index] += diff;

            // Possible combination found
            if (diff == targetSum)
            {
                yield return current.Clone();

                current[index] -= 1;
                diff--;
            }

            // Shouldn't cause issues without check, but avoids un-necessary function calls
            if (index != Type.Wool)
                while(diff > 0)
                {
                    // Inefficient?
                    // Each recursion creates a new iterator
                    foreach(var found in RecurseOptions(hand, current.Clone(), targetSum - diff, index + 1))
                        yield return found;

                    diff--;
                    current[index] -= 1;
                }
        }
    }

    public void Update(GameState gameState, Action.IAction lastAction)
    {
        gameState.CurrentPlayerOffset++;
        FindTarget(gameState);
    }

    private void FindTarget(GameState gameState)
    {
        for (; gameState.CurrentPlayerOffset < Rules.NUM_PLAYERS; gameState.CurrentPlayerOffset++)
            if (gameState.Players[gameState.GetCurrentPlayer()].Hand.Count() > Rules.MAX_HAND_SIZE)
                return;
        
        gameState.CurrentPlayerOffset = 0;
        gameState.PhaseManager.ChangePhase(Robber.NAME);
    }

    public const string NAME = "Discard";
}