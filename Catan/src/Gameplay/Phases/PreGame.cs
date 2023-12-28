using System;
using System.Collections.Generic;
using System.Linq;
using Grid.Hexagonal;

namespace Catan;

public interface IPreGamePhase : IGamePhase
{}

public class PreGameSettlement : IPreGamePhase
{
    private bool m_IsPregame2;

    public void OnEnter(params object[] argn)
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
        List<IAction> actions = new();
        int playerID = gameState.GetCurrentPlayer();

        // Check all nodes
        List<Vertex.Key> nodes = gameState.Board.GetAllVertices();
        foreach (Vertex.Key nodePos in nodes)
        {
            if (gameState.CheckSettlementPos(playerID, nodePos, true))
                actions.Add(new BuildSettlementAction(playerID, nodePos, true));
        }

        return actions;
    }

    public void NextPhase(GameState gameState, IAction lastAction)
    {
        if (lastAction.GetType() != typeof(BuildSettlementAction))
            throw new ArgumentException("PreGameSettlement got unexpected type for lastAction, expected BuildSettlementAction");

        // Advances to PreGameRoad, passing built settlement as argn
        Vertex.Key settlementPos = ((BuildSettlementAction)lastAction).Position;

        // Need to gain starting resources

        gameState.PhaseManager.ChangePhase(PreGameRoad.NAME, settlementPos, m_IsPregame2);
    }

    public const string NAME = "PreGameSettlement";
}

public class PreGameRoad : IPreGamePhase
{
    private Vertex.Key m_SettlementPos;
    private bool m_IsPregame2;

    public void OnEnter(params object[] argn)
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
        int playerID = gameState.GetCurrentPlayer();

        Edge.Key[] edges = m_SettlementPos.GetProtrudingEdges();

        foreach (Edge.Key edgePos in edges)
            if (gameState.Board.TryGetEdge<Path>(edgePos, out _))
                actions.Add(new BuildRoadAction(playerID, edgePos, true));

        return actions;
    }

    public void NextPhase(GameState gameState, IAction lastAction)
    {
        if (!m_IsPregame2)
        {
            if (gameState.CurrentPlayerOffset == Rules.NUM_PLAYERS - 1)
                m_IsPregame2 = true;
            
            else
                gameState.CurrentPlayerOffset++;
        }

        else
        {
            if (gameState.CurrentPlayerOffset == 0)
            {
                // Go to turn start phase
                gameState.PhaseManager.ChangePhase(TurnStart.NAME);
            }

            gameState.CurrentPlayerOffset--;
        }

        gameState.PhaseManager.ChangePhase(PreGameSettlement.NAME, m_IsPregame2);
    }

    public const string NAME = "PreGameRoad";
}