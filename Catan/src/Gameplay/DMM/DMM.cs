using System.Collections.Generic;

namespace Catan.Behaviour;

/// <summary>
/// A decision making module, responsible for controlling a player
/// </summary>
public abstract class DMM
{
    public List<IAction> Actions;

    public abstract void Update(GameState gameState);
}