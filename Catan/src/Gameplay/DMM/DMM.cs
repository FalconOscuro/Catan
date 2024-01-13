using System.Collections.Generic;
using Catan.Action;

namespace Catan.Behaviour;

/// <summary>
/// A decision making module responsible for controlling a <see cref="Player"/>.
/// </summary>
public abstract class DMM
{
    public int OwnerID;

    /// <summary>
    /// Update function called during owning Players turn
    /// </summary>
    /// <remarks>
    /// Should pick a valid action from <see cref="Actions"/>,
    /// potentially change to take copy of gamestate and return chosen action?
    /// </remarks>
    public abstract int GetNextAction(GameState gameState, List<IAction> actions);

    public abstract void ImDraw();
}