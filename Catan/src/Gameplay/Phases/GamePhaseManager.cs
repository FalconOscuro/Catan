using System;
using System.Collections.Generic;

namespace Catan.State;

/// <summary>
/// Gameplay FSM
/// </summary>
/// <remarks>
/// "States" are referred to as "Phases", inheriting from <see cref="IGamePhase"/>,
/// to prevent confusion with <see cref="GameState"/>.
/// </remarks>
public class GamePhaseManager
{
    /// <summary>
    /// Name key for current phase
    /// </summary>
    /// <remarks>
    /// Modify using <see cref="ChangePhase"/>.
    /// </remarks>
    public string CurrentPhase { get; private set; }

    /// <summary>
    /// Dictionary of all possible phases keyed by name
    /// </summary>
    private readonly Dictionary<string, IGamePhase> m_Phases = new(){
        { PreGameSettlement.NAME, new PreGameSettlement() },
        { PreGameRoad.NAME, new PreGameRoad() },
        { TurnStart.NAME, new TurnStart() },
        { TurnMain.NAME, new TurnMain() },
        { Robber.NAME, new Robber() },
        { Discard.NAME, new Discard() }
    };

    public GamePhaseManager()
    {
        // TODO: Allow skip pregame for pre-gen boards
        CurrentPhase = PreGameSettlement.NAME;
        m_Phases[CurrentPhase].OnEnter();
    }

    /// <summary>
    /// Transition to a new phase
    /// </summary>
    /// <param name="phaseName">Key for new phase</param>
    /// <param name="argn">Arguments passed to new phase on enter</param>
    public void ChangePhase(string phaseName, params object[] argn)
    {
        // Ensure phase exists
        if (!m_Phases.ContainsKey(phaseName))
            throw new ArgumentException(string.Format("{0} is an invalid phase", phaseName));
        
        // Exit current phase
        m_Phases[CurrentPhase].OnExit();

        // Exit new phase passing arguments
        CurrentPhase = phaseName;
        m_Phases[CurrentPhase].OnEnter(argn);
    }

    /// <summary>
    /// Update current phase
    /// </summary>
    /// <remarks>
    /// Can result in <see cref="ChangePhase"/>.
    /// </remarks>
    /// <param name="lastAction">Last action executed by player.</param>
    public void Update(GameState gameState, Action.IAction lastAction)
    {
        m_Phases[CurrentPhase].Update(gameState, lastAction);
    }

    /// <summary>
    /// Get a list from the current <see cref="IGamePhase"/>
    /// </summary>
    public List<Action.IAction> GetValidActions(GameState gameState)
    {
        return m_Phases[CurrentPhase].GetValidActions(gameState);
    }
}