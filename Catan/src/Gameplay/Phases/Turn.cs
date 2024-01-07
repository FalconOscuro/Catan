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
        {
            for (Resources.Type type = Resources.Type.Brick; type < Resources.Type.Wool + 1; type++)
                actions.Add(new Monopoly(){
                    OwnerID = player.ID,
                    TargetResource = type
                });
        }

        if (PlayableDevCards[DevCards.Type.YearOfPlenty])
        {}

        return actions;
    }
}

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
        IEnumerable<IAction> actions = new List<IAction>();
        int currentPlayer = gameState.GetCurrentPlayerID();

        // Check for buildables
        actions = actions.Concat(
            GetValidSettlementActions(gameState).Concat(
                GetValidCityActions(gameState).Concat(
                    GetValidRoadActions(gameState)
        )));

        // Purchasing Dev cards
        if (gameState.DevCardDeck.Count > 0 && gameState.Players[currentPlayer].Hand >= Rules.DEVELOPMENT_CARD_COST)
        {
            IAction action = new BuyDevCardAction(){
                OwnerID = currentPlayer
            };
            actions = actions.Append(action);
        }

        actions = actions.Concat(GetDevCardActions(gameState));

        // Bank trades
        actions = actions.Concat(GetBankTrades(gameState));

        // Endturn is always valid
        return actions.Append(new EndTurn(){OwnerID = currentPlayer}).ToList();
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

    /// <summary>
    /// Add all valid settlement actions for the current player to the valid action list
    /// </summary>
    /// <param name="pregame"></param>
    public static List<IAction> GetValidSettlementActions(GameState gameState, bool pregame = false)
    {
        List<IAction> actions = new();
        Player player = gameState.GetCurrentPlayer();

        // Does player have settlements remaining
        if (player.Settlements == 0)
            return actions;

        // Can player afford it, ignored with pregame flag
        else if (!(Rules.SETTLEMENT_COST <= player.Hand || pregame))
            return actions;
        
        List<Vertex.Key> nodes = gameState.Board.GetAllVertices();

        foreach(Vertex.Key nodePos in nodes)
        {
            if (!gameState.Board.TryGetVertex(nodePos, out Node node))
                continue;
            // Throw error?

            // node is already occupied
            else if (node.OwnerID != -1)
                continue;
            
            // These checks are ignored in pregame phase
            if (!pregame)
            {
                // Check if connected by road
                Edge.Key[] edges = nodePos.GetProtrudingEdges();
                
                bool connected = false;
                foreach (Edge.Key edgePos in edges)
                {
                    if (gameState.Board.TryGetEdge(edgePos, out Path path))
                        connected |= path.OwnerID == player.ID;
                }

                if (!connected)
                    continue;
            }

            // Check if too close to other settlements
            Vertex.Key[] adjNodes = nodePos.GetAdjacentVertices();

            bool isValid = true;
            foreach(Vertex.Key adjNodePos in adjNodes)
            {
                if (gameState.Board.TryGetVertex(adjNodePos, out Node adjNode))
                    isValid &= adjNode.OwnerID == -1;
            }

            if (!isValid)
                continue;
            
            actions.Add(new BuildSettlementAction(player.ID, nodePos){
                TriggerStateChange = pregame
            });
        }

        return actions;
    }

    /// <summary>
    /// Add all valid city actions for current player to the valid action list
    /// </summary>
    public static List<IAction> GetValidCityActions(GameState gameState)
    {
        List<IAction> actions = new();
        Player player = gameState.GetCurrentPlayer();

        // Check if player has remaining cities, or replaceable settlements
        if (player.Cities == 0 || player.Settlements == Rules.MAX_SETTLEMENTS)
            return actions;
        
        // Check if can afford
        else if (!(Rules.CITY_COST <= player.Hand))
            return actions;
        
        List<Vertex.Key> nodes = gameState.Board.GetAllVertices();
        foreach (Vertex.Key nodePos in nodes)
        {
            if (!gameState.Board.TryGetVertex(nodePos, out Node node))
                continue; // Should be impossible, throw error?
            
            else if (node.OwnerID == player.ID && !node.City)
                actions.Add(new BuildCityAction(player.ID, nodePos));
        }

        return actions;
    }

    public static List<IAction> GetValidRoadActions(GameState gameState, bool free = false)
    {
        List<IAction> actions = new();
        Player player = gameState.GetCurrentPlayer();

        // Check for remaining roads
        if (player.Roads == 0)
            return actions;
        
        // Check if player can afford, ignored with free flag
        else if (!(Rules.ROAD_COST <= player.Hand || free))
            return actions;
        
        List<Edge.Key> edges = gameState.Board.GetAllEdges();

        foreach (Edge.Key edgePos in edges)
        {
            if (CheckRoadPos(gameState.Board, edgePos, player.ID))
                actions.Add(new BuildRoadAction(player.ID, edgePos));
        }

        return actions;
    }

    /// <summary>
    /// Evaluates eligibility for a single road
    /// </summary>
    private static bool CheckRoadPos(HexGrid board, Edge.Key pos, int playerID)
    {
        if (!board.TryGetEdge(pos, out Path edge))
            return false; // Should be impossible, throw error?

        // Path already owned
        else if (edge.OwnerID != -1)
            return false;
        
        Vertex.Key[] nodes = pos.GetEndpoints();

        foreach(Vertex.Key nodePos in nodes)
        {
            if (!board.TryGetVertex(nodePos, out Node node))
                continue; // Should be impossible, throw error?
            
            // Path connects to owned node
            else if (node.OwnerID == playerID)
                return true;
            
            // Owned by other player, cannot build through
            else if (node.OwnerID != -1)
                continue;
            
            // Loop protruding edges from current node
            // Searching for connected roads
            Edge.Key[] adjEdges = nodePos.GetProtrudingEdges();
            foreach(Edge.Key adjEdgePos in adjEdges)
            {
                // Skip target edge
                if (adjEdgePos == pos)
                    continue;
                
                if (!board.TryGetEdge(adjEdgePos, out Path adjEdge))
                    continue;
                
                // Found connected road
                else if (adjEdge.OwnerID == playerID)
                    return true;
            }
        }

        // All possibilites checked, cannot build here
        return false;
    }

    public static List<IAction> GetBankTrades(GameState gameState)
    {
        List<IAction> actions = new();

        int playerID = gameState.GetCurrentPlayerID();
        Resources.Collection playerHand = gameState.GetCurrentPlayer().Hand;

        // Trading with bank impossible
        if (playerHand.Count() < Rules.PORT_RATIO)
            return actions;
        
        // Iterate all resources for selling
        for (Resources.Type sellType = Resources.Type.Brick; sellType < Resources.Type.Wool + 1; sellType++)
        {
            // TODO: Update for port trades
            int ratio = Rules.BANK_RATIO;

            // Not enought to trade, skip
            if (playerHand[sellType] < ratio)
                continue;
            
            Resources.Collection giving = new();
            giving[sellType] = ratio;

            // Iterate resources for buying 
            for (Resources.Type buyType = Resources.Type.Brick; buyType < Resources.Type.Wool + 1; buyType++)
            {
                // Skip same type and un-available resources
                if (buyType == sellType || gameState.Bank[buyType] < 1)
                    continue;
                
                Resources.Collection receiving = new();
                receiving[buyType] = 1;

                IAction trade = new Trade(){
                    OwnerID = playerID,
                    TargetID = -1,
                    Giving = giving,
                    Receiving = receiving
                };

                actions.Add(trade);
            }
        }

        return actions;
    }

    public const string NAME = "TurnMain";
}