using System.Collections.Generic;

namespace Catan.Behaviour;

/// <summary>
/// A decision making module responsible for controlling a <see cref="Player"/>.
/// </summary>
public abstract class DMM
{
    /// <summary>
    /// List of all viable <see cref="IAction"/>.
    /// </summary>
    /// <remarks>
    /// Determined by <see cref="IGamePhase.GetValidActions(GameState)"/>.
    /// </remarks>
    public List<IAction> Actions = new();

    /// <summary>
    /// Update function called during owning Players turn
    /// </summary>
    /// <remarks>
    /// Should pick a valid action from <see cref="Actions"/>,
    /// potentially change to take copy of gamestate and return chosen action?
    /// </remarks>
    public abstract void Update(GameState gameState);
}