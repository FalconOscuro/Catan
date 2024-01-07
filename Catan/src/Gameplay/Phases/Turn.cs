using System.Collections.Generic;
using System.Linq;
using Catan.Action;
using Grid.Hexagonal;

namespace Catan.State;

public abstract class ITurnPhase : IGamePhase
{
    public Dictionary<DevCards.Type, bool> PlayableDevCards = new();

    public virtual void OnEnter(GameState gameState, params object[] argn)
    {
        Player player = gameState.GetCurrentPlayer();

        for (DevCards.Type devCard = DevCards.Type.Knight; devCard < DevCards.Type.Monopoly + 1; devCard++)
            PlayableDevCards[devCard] = player.HeldDevCards.Contains(devCard);
    }

    public abstract void OnExit();

    public abstract void Update(GameState gameState, IAction lastAction);

    public abstract List<IAction> GetValidActions(GameState gameState);

    protected List<IAction> GetDevCardActions(GameState gameState)
    {
        List<IAction> actions = new();
        Player player = gameState.GetCurrentPlayer();

        // Empty hand, also need to check if dev card already played & Differentiate same turn bought dev cards
        if (player.HeldDevCards.Count == 0 || player.HasPlayedDevCard)
            return actions;

        if (PlayableDevCards[DevCards.Type.Knight])
        {
            foreach ((Axial targetPos, int targetID) in Robber.GetAllRobberMoves(gameState, player.ID))
                actions.Add(new KnightAction(){
                    OwnerID = player.ID,
                    TargetID = targetID,
                    TargetPos = targetPos
                });
        }

        if (PlayableDevCards[DevCards.Type.RoadBuilding])
        {}

        if (PlayableDevCards[DevCards.Type.Monopoly])
        {}

        if (PlayableDevCards[DevCards.Type.YearOfPlenty])
        {}

        return actions;
    }
}

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
    public override void OnEnter(GameState gameState, params object[] argn)
    {
        base.OnEnter(gameState, argn);

        gameState.GetCurrentPlayer().HasPlayedDevCard = false;
    }

    public override void OnExit()
    {}

    public override List<IAction> GetValidActions(GameState gameState)
    {
        List<IAction> actions = new()
        {
            new RollDiceAction(){
                OwnerID = gameState.GetCurrentPlayerID()
            } // diceroll is always valid action
        };

        actions = actions.Concat(GetDevCardActions(gameState)).ToList();

        return actions;
    }

    /// <remarks>
    /// Advances to <see cref="TurnMain"/> if lastAction was <see cref="RollDiceAction"/>.
    /// </remarks>
    public override void Update(GameState gameState, Action.IAction lastAction)
    {
        // Account for dev cards
        if (lastAction is not RollDiceAction diceRoll)
            return;

        else if (diceRoll.TriggerRobber)
            gameState.PhaseManager.ChangePhase(Discard.NAME, gameState);

        else
            gameState.PhaseManager.ChangePhase(TurnMain.NAME, gameState);
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
    public override void OnExit()
    {}

    public override List<IAction> GetValidActions(GameState gameState)
    {
        List<IAction> actions = new();
        int currentPlayer = gameState.GetCurrentPlayerID();

        // Check for buildables
        gameState.GetValidRoadActions(currentPlayer, actions);
        gameState.GetValidSettlementActions(currentPlayer, actions);
        gameState.GetValidCityActions(currentPlayer, actions);

        // Purchasing Dev cards
        if (gameState.DevCardDeck.Count > 0 && gameState.Players[currentPlayer].Hand >= Rules.DEVELOPMENT_CARD_COST)
        {
            IAction action = new BuyDevCardAction(){
                OwnerID = currentPlayer
            };
            actions.Add(action);
        }

        actions = actions.Concat(GetDevCardActions(gameState)).ToList();

        // Endturn is always valid
        actions.Add(new EndTurn(){OwnerID = currentPlayer});

        return actions;
    }

    /// <remarks>
    /// Advances to <see cref="TurnStart"/> on <see cref="EndTurn"/>".
    /// Could be moved to gameState??
    /// </remarks>
    public override void Update(GameState gameState, IAction lastAction)
    {
        if (lastAction is EndTurn)
            gameState.PhaseManager.ChangePhase(TurnStart.NAME, gameState);
    }

    public const string NAME = "TurnMain";
}