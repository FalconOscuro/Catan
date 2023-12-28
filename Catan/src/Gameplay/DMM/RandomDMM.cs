using System;

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

    public override void Update(GameState gameState)
    {
        Actions[m_Random.Next(0, Actions.Count)].Execute(gameState);
    }
}