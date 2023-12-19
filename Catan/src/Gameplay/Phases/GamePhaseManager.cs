using System;
using System.Collections.Generic;

namespace Catan;

public class GamePhaseManager
{
    public string CurrentPhase { get; private set; }

    private readonly Dictionary<string, IGamePhase> m_Phases = new(){
        { PreGameSettlement.NAME, new PreGameSettlement() },
        { PreGameRoad.NAME, new PreGameRoad() },
        { TurnStart.NAME, new TurnStart() },
        { TurnMain.NAME, new TurnMain() }
    };

    public GamePhaseManager()
    {
        CurrentPhase = PreGameSettlement.NAME;
        m_Phases[CurrentPhase].OnEnter();
    }

    public void ChangePhase(string phaseName, params object[] argn)
    {
        if (!m_Phases.ContainsKey(phaseName))
            throw new ArgumentException(string.Format("{0} is an invalid phase", phaseName));
        
        m_Phases[CurrentPhase].OnExit();

        CurrentPhase = phaseName;
        m_Phases[CurrentPhase].OnEnter();
    }

    public void NextPhase(GameState gameState, IAction lastAction)
    {
        m_Phases[CurrentPhase].NextPhase(gameState, lastAction);
    }
}