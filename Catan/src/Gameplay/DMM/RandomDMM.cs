using System;
using System.Collections.Generic;
using Catan.Action;

namespace Catan.Behaviour;

/// <summary>
/// Simple DMM, decides action at random from <see cref="Actions"/>.
/// </summary>
public class RandomDMM : DMM
{
    private Random m_Random;

    public RandomDMM()
    {
        m_Random = new();
    }

    public override int GetNextAction(GameState gameState, List<IAction> actions)
    {
        return m_Random.Next(0, actions.Count);
    }
}