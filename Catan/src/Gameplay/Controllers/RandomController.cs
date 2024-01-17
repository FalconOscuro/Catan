using System;
using System.Collections.Generic;
using Catan.Action;

namespace Catan.Behaviour;

/// <summary>
/// Simple DMM, decides action at random from <see cref="Actions"/>.
/// </summary>
public class RandomController : Controller
{
    private Random m_Random;

    public RandomController()
    {
        m_Random = new();
    }

    public override int ChooseAction(GameState gameState, List<IAction> actions)
    {
        return m_Random.Next(0, actions.Count);
    }

    public override void ImDraw()
    {
    }
}