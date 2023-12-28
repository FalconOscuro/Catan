using System;

namespace Catan.Behaviour;

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