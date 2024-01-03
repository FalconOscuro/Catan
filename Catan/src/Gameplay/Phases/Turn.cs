using System.Collections.Generic;

namespace Catan;

public interface ITurnPhase : IGamePhase
{}

// TODO: Dev cards

/// <summary>
/// Start of turn phase
/// </summary>
/// <remarks>
/// Actions: <br/>
/// - <see cref="RollDice"/>
/// </remarks>
public class TurnStart : ITurnPhase
{
    public void OnEnter(params object[] argn)
    {
        // TODO: Update all dev cards to be playable
    }

    public void OnExit()
    {}

    public List<IAction> GetValidActions(GameState gameState)
    {
        List<IAction> actions = new()
        {
            new RollDiceAction() // diceroll is always valid action
        };

        // Dev cards

        return actions;
    }

    /// <remarks>
    /// Advances to <see cref="TurnMain"/> if lastAction was <see cref="RollDiceAction"/>.
    /// </remarks>
    public void Update(GameState gameState, IAction lastAction)
    {
        // Account for dev cards
        if (lastAction.GetType() != typeof(RollDiceAction))
            return;
        
        else if (gameState.LastRoll == 7)
            gameState.PhaseManager.ChangePhase(Discard.NAME, gameState);

        else
        {
            // Distribute resources and change state
            // Could be moved to gamestate??
            gameState.DistributeResources();
            gameState.PhaseManager.ChangePhase(TurnMain.NAME);
        }
    }

    public const string NAME = "TurnStart";
}

/// <summary>
/// Main turn state
/// </summary>
/// <remarks>
/// Actions: <br/>
/// - <see cref="BuildRoadAction"/><br/>
/// - <see cref="BuildSettlementAction"/><br/>
/// - <see cref="BuildCityAction"/><br/>
/// - <see cref="EndTurn"/>
/// </remarks>
public class TurnMain : ITurnPhase
{
    // TODO: Keep track of played dev cards
    public void OnEnter(params object[] argn)
    {}

    public void OnExit()
    {}

    public List<IAction> GetValidActions(GameState gameState)
    {
        List<IAction> actions = new();
        int currentPlayer = gameState.GetCurrentPlayer();

        // Check for buildables
        gameState.GetValidRoadActions(currentPlayer, actions);
        gameState.GetValidSettlementActions(currentPlayer, actions);
        gameState.GetValidCityActions(currentPlayer, actions);

        // TODO: Trading, Dev cards

        // Endturn is always valid
        actions.Add(new EndTurn());

        return actions;
    }

    /// <remarks>
    /// Advances to <see cref="TurnStart"/> on <see cref="EndTurn"/>".
    /// Could be moved to gameState??
    /// </remarks>
    public void Update(GameState gameState, IAction lastAction)
    {
        if (lastAction is EndTurn)
            gameState.PhaseManager.ChangePhase(TurnStart.NAME);
    }

    public const string NAME = "TurnMain";
}