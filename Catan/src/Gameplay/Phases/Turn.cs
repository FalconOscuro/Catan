using System.Collections.Generic;

namespace Catan;

public interface ITurnPhase : IGamePhase
{}

public class TurnStart : ITurnPhase
{
    public void OnEnter(params object[] argn)
    {}

    public void OnExit()
    {}

    public List<IAction> GetValidActions(GameState gameState)
    {
        List<IAction> actions = new()
        {
            new RollDiceAction()
        };

        // Dev cards

        return actions;
    }

    public void NextPhase(GameState gameState, IAction lastAction)
    {
        if (lastAction.GetType() != typeof(RollDiceAction))
            return;
        
        else if (gameState.LastRoll == 7)
        {
            // robber
        }

        else
        {
            gameState.DistributeResources();
            gameState.PhaseManager.ChangePhase(TurnMain.NAME);
        }
    }

    public const string NAME = "TurnStart";
}

public class TurnMain : ITurnPhase
{
    public void OnEnter(params object[] argn)
    {}

    public void OnExit()
    {}

    public List<IAction> GetValidActions(GameState gameState)
    {
        List<IAction> actions = new();
        int currentPlayer = gameState.GetCurrentPlayer();

        gameState.GetValidRoadActions(currentPlayer, actions);
        gameState.GetValidSettlementActions(currentPlayer, actions);
        gameState.GetValidCityActions(currentPlayer, actions);

        actions.Add(new EndTurn());

        return actions;
    }

    public void NextPhase(GameState gameState, IAction lastAction)
    {
        if (lastAction is EndTurn)
            gameState.PhaseManager.ChangePhase(TurnStart.NAME);
    }

    public const string NAME = "TurnMain";
}