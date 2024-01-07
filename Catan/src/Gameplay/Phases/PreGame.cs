using System;
using System.Collections.Generic;
using Catan.Action;
using Grid.Hexagonal;

namespace Catan.State;

// TODO: 2nd pregame resources

/// <summary>
/// Simple interface used by all pre-game states
/// </summary>
public interface IPreGamePhase : IGamePhase
{}

/// <summary>
/// Pregame settlement placement
/// </summary>
/// <remarks>
/// Actions: <br/>
/// - <see cref="BuildSettlementAction"/>
/// </remarks>
public class PreGameSettlement : IPreGamePhase
{
    /// <summary>
    /// True if 2nd half of pregame
    /// </summary>
    /// <remarks>
    /// Used to track turn-progression direction and determining if resources need to distributed.
    /// </remarks>
    private bool m_IsPregame2;

    /// <param name="argn">arg0: <see cref="m_IsPreGame2"/>, defaults to false</param>
    public void OnEnter(GameState gameState, params object[] argn)
    {
        if (argn.Length == 0)
            m_IsPregame2 = false;
        
        else
            m_IsPregame2 = (bool)argn[0];
    }

    public void OnExit()
    {}

    public List<IAction> GetValidActions(GameState gameState)
    {
        List<IAction> actions = TurnMain.GetValidSettlementActions(gameState, true);

        return actions;
    }

    /// <remarks>
    /// Always advances <see cref="GamePhaseManager.CurrentPhase"/> to <see cref="PreGameRoad"/>,
    /// passing <see cref="m_IsPregame2"/> and settlement pos for lastAction.
    /// </remarks>
    public void Update(GameState gameState, Action.IAction lastAction)
    {
        // Only BuildSettlementActions are valid
        BuildSettlementAction settlement = lastAction as BuildSettlementAction ?? 
            throw new ArgumentException("PreGameSettlement got unexpected type for lastAction, expected BuildSettlementAction");

        // Advances to PreGameRoad, passing built settlement as argn
        gameState.PhaseManager.ChangePhase(PreGameRoad.NAME, gameState, settlement.Position, m_IsPregame2);
    }

    public const string NAME = "PreGameSettlement";
}

/// <summary>
/// Pregame road placement
/// </summary>
/// <remarks>
/// Actions: <br/>
/// - <see cref="BuildRoadAction"/>
/// </remarks>
public class PreGameRoad : IPreGamePhase
{
    /// <summary>
    /// Position of previously placed settlement
    /// </summary>
    /// <remarks>
    /// Road must be connected to previous settlement from <see cref="PreGameSettlement"/>.
    /// </remarks>
    private Vertex.Key m_SettlementPos;

    /// <summary>
    /// True if 2nd half of pregame
    /// </summary>
    /// <remarks>
    /// Used to track turn-progression direction.
    /// </remarks>
    private bool m_IsPregame2;


    /// <param name="argn">
    /// arg0: <see cref="m_SettlementPos"/>,
    /// arg1: <see cref="m_IsPregame2"/>
    /// </param>
    public void OnEnter(GameState gameState, params object[] argn)
    {
        if (argn.Length < 2)
            throw new ArgumentException(string.Format("PreGameRoad OnEnter() given incorrect number of arguments: expected 2 got {0}", argn.Length));
        
        m_SettlementPos = (Vertex.Key)argn[0];
        m_IsPregame2 = (bool)argn[1];
    }

    public void OnExit()
    {}

    public List<IAction> GetValidActions(GameState gameState)
    {
        List<IAction> actions = new();
        int playerID = gameState.GetCurrentPlayerID();

        // Check validity for all surrounding edges of settlement
        Edge.Key[] edges = m_SettlementPos.GetProtrudingEdges();
        foreach (Edge.Key edgePos in edges)
            if (gameState.Board.TryGetEdge<Path>(edgePos, out _))
                actions.Add(new BuildRoadAction(playerID, edgePos, true){
                    TriggerStateChange = true
                });

        return actions;
    }

    /// <remarks>
    /// Advances <see cref="GamePhaseManager.CurrentPhase"/> to <see cref="PreGameSettlement"/> passing <see cref="m_IsPreGame2"/>,
    /// or <see cref="TurnStart"/> if end of pre-game reached.
    /// </remarks>
    public void Update(GameState gameState, IAction lastAction)
    {
        // Responsible for advancing current player turn

        // Increase if 1st half
        if (!m_IsPregame2)
        {
            // If end of 1st half, do not advance turn, update flage for pregame2
            if (gameState.CurrentPlayerOffset == Rules.NUM_PLAYERS - 1)
                m_IsPregame2 = true;
            
            else
                gameState.CurrentPlayerOffset++;
        }

        // if pregame 2 & offset is 0, pregame is finished, advance to turnstart to begin main game loop
        else if (gameState.CurrentPlayerOffset == 0)
        {
            gameState.PhaseManager.ChangePhase(TurnStart.NAME, gameState);
            return;
        }

        // Else, is not at end of pregame 2, advance back through players
        else
            gameState.CurrentPlayerOffset--;

        gameState.PhaseManager.ChangePhase(PreGameSettlement.NAME, gameState, m_IsPregame2);
    }

    public const string NAME = "PreGameRoad";
}