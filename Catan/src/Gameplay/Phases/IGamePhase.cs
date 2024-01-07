using System.Collections.Generic;

namespace Catan.State;

/// <summary>
/// Interface State used in <see cref="GamePhaseManager"/>
/// </summary>
/// <remarks>
/// Reffered to as "Phase" to prevent confusion with <see cref="GameState"/>.
/// </remarks>
public interface IGamePhase
{
    /// <summary>
    /// Called when made <see cref="GamePhaseManager.CurrentPhase"/>.
    /// </summary>
    /// <param name="argn">Extra arguments require parsing.</param>
    void OnEnter(GameState gameState, params object[] argn);

    /// <summary>
    /// Called when removed as <see cref="GamePhaseManager.CurrentPhase"/> by <see cref="GamePhaseManager.ChangePhase(string, object[])"/>.
    /// </summary>
    void OnExit();

    /// <summary>
    /// Return list of all valid actions.
    /// </summary>
    /// <param name="gameState">Current gameState</param>
    /// <remarks>
    /// Used by a <see cref="Player.DMM"/>.
    /// </remarks>
    List<Action.IAction> GetValidActions(GameState gameState);

    /// <summary>
    /// Update phase.
    /// </summary>
    /// <remarks>
    /// Can be used to updated <see cref="GamePhaseManager.CurrentPhase"/> based on last action.
    /// </remarks>
    void Update(GameState gameState, Action.IAction lastAction);
}