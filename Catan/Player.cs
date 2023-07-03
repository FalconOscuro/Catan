using System.Collections.Generic;

using Microsoft.Xna.Framework;

using ImGuiNET;

namespace Catan;

class Player
{
    public Player(Board board, Color colour)
    {
        m_GameBoard = board;
        m_TurnState = TurnState.End;
        m_SelectedNode = null;
        m_SelectedEdge = null;
        m_SelectedTile = null;
        Colour = colour;

        m_VictoryPoints = 2;
        m_SelectedCard = 0;

        RoadLength = 0;
        ArmySize = 0;

        m_Pieces = new Pieces();
        m_Road = new List<Edge>();
        m_Highlighted = false;

        ResourceHand = new Resources();
        m_CurrentTrade = new Trade();
        m_DevCards = new List<DevelopmentCard>();
        m_OwnedNodes = new List<Node>();
        m_ExchangeRate = new Resources(4, 4, 4, 4, 4);
    }

    public void GiveResource(Resources.Type resource, int num = 1)
    {
        ResourceHand.AddType(resource, num);
    }

    public Resources.Type StealResource()
    {
        return ResourceHand.Steal();
    }

    public void StartTurn()
    {
        m_TurnState = TurnState.Start;
    }

    public void SetState(TurnState state)
    {
        if (state == TurnState.Discard && ResourceHand.GetTotal() < 8)
            state = TurnState.End;

        else if (state == TurnState.Robber && m_TurnState == TurnState.Start)
            state = TurnState.PreRollRobber;

        if (state != TurnState.Trade)
            m_CurrentTrade = new Trade();

        m_TurnState = state;
    }

    public void SetActiveTrade(Trade trade)
    {
        m_CurrentTrade = trade;
    }

    private void Roll()
    {
        m_GameBoard.RollDice();
        SetState(TurnState.Main);
    }

    private void EndTurn()
    {
        FindLongestRoad();

        m_TurnState = TurnState.End;
        m_CurrentTrade = new Trade();

        DeselectNode();
        DeselectEdge();
        DeselectTile();

        m_SelectedCard = 0;

        for (int i = 0; i < m_DevCards.Count; i++)
            m_DevCards[i].Playable = true;
    }

    public bool HasWon()
    {      
        return m_VictoryPoints + (LargestArmy ? 2 : 0) + (LongestRoad ? 2 : 0) + GetHiddenVP() >= 10;
    }

    /// <summary>
    /// Get Victory Points from development cards
    /// </summary>
    private int GetHiddenVP()
    {
        int victoryCards = 0;
        foreach (DevelopmentCard developmentCard in m_DevCards)
            if (developmentCard is VictoryPoint)
                victoryCards++;
        
        return victoryCards;
    }

    public bool HasTurnEnded()
    {
        return m_TurnState == TurnState.End;
    }

    public int GetHandSize()
    {
        return ResourceHand.GetTotal();
    }


    // Selection & deselection of elements

    public void SelectNode(Node node)
    {
        if (m_TurnState == TurnState.End)
            return;

        DeselectNode();

        if (node != null)
        {
            m_SelectedNode = node;
            m_SelectedNode.Selected = true;
        }
    }

    private void DeselectNode()
    {
        if (m_SelectedNode != null)
            m_SelectedNode.Selected = false;

        m_SelectedNode = null;
    }

    public void SelectEdge(Edge edge)
    {
        if (m_TurnState == TurnState.End)
            return;
        
        DeselectEdge();

        if (edge != null)
        {
            m_SelectedEdge = edge;
            m_SelectedEdge.Selected = true;
        }
    }

    public void FindLongestRoad()
    {
        RoadLength = 0;

        for (int i = 0; i < m_OwnedNodes.Count; i++)
        {
            List<Edge> path = m_OwnedNodes[i].StartRecurse(this);

            if (path.Count > RoadLength)
            {
                RoadLength = path.Count;
                m_Road = path;
            }
        }
    }

    private void DeselectEdge()
    {
        if (m_SelectedEdge != null)
            m_SelectedEdge.Selected = false;
        
        m_SelectedEdge = null;
    }

    public void SelectTile(Tile tile)
    {
        if (m_TurnState == TurnState.End)
            return;
        
        DeselectTile();

        if (tile != null)
        {
            m_SelectedTile = tile;
            m_SelectedTile.Selected = true;
        }
    }

    private void DeselectTile()
    {
        if (m_SelectedTile != null)
            m_SelectedTile.Selected = false;

        m_SelectedTile = null;
    }

    public void RegisterNode(Node node)
    {
        m_OwnedNodes.Add(node);
    }

    /// <summary>
    /// Debug menu UI
    /// </summary>
    public void DebugDrawUI()
    {
        ImGui.Text(string.Format("State: {0}", m_TurnState.ToString()));
        ImGui.Text(string.Format("VP: {0}", m_VictoryPoints + (LargestArmy ? 2 : 0) + (LongestRoad ? 2 : 0)));
        ImGui.Text(string.Format("Resource Cards: {0}", GetHandSize()));
        ImGui.Text(string.Format("Longest Road: {0}", RoadLength));
        ImGui.Separator();

        if (ImGui.Button("Calculate Longest Road"))
            FindLongestRoad();

        ImGui.Checkbox("Highlight Road", ref m_Highlighted);
        for (int i = 0; i < m_Road.Count; i++)
            m_Road[i].Selected = m_Highlighted;

        ResourceHand.UIDraw(true);
    }


    // Gameplay UI

    public void GameDrawUI()
    {
        if (m_TurnState == TurnState.End)
            return;

        ResourceHand.UIDraw();

        ImGui.Separator();

        switch(m_TurnState)
        {
            case TurnState.PreGame1:
            case TurnState.Pregame2:
                PreGameUI();
                break;

            case TurnState.Start:
                TurnStartUI();
                break;
            
            case TurnState.Main:
                TurnMainUI();
                break;
            
            case TurnState.Discard:
                DiscardUI();
                break;
            
            case TurnState.Robber:
            case TurnState.PreRollRobber:
                RobberUI();
                break;
            
            case TurnState.Trade:
                TradeAcceptUI();
                break;

            case TurnState.RoadBuilding:
            case TurnState.RoadBuilding2:
                RoadBuildingUI();
                break;

            case TurnState.YearOfPlenty:
                YearOfPlentyUI();
                break;

            case TurnState.Monopoly:
                MonopolyUI();
                break;
        }
    }

    private void PreGameUI()
    {
        if (!ImGui.Button("Build") || m_SelectedNode == null || m_SelectedEdge == null)
            return;
            
        else if (!m_SelectedNode.IsAvailable() || !m_SelectedEdge.IsAvailable()
            || (m_SelectedEdge.Nodes[0] != m_SelectedNode && m_SelectedEdge.Nodes[1] != m_SelectedNode))
            return;

        m_SelectedNode.Owner = this;
        m_SelectedEdge.Owner = this;
        m_OwnedNodes.Add(m_SelectedNode);

        if (m_TurnState == TurnState.Pregame2)
        {
            Trade trade = new Trade();
            trade.To = ResourceHand;
            trade.From = m_GameBoard.ResourceBank;

            for (int i = 0; i < 3; i++)
                if (m_SelectedNode.Tiles[i] != null)
                    trade.Giving.AddType(m_SelectedNode.Tiles[i].Type, 1);
            
            trade.TryExecute();
        }

        EndTurn();
    }

    private void TurnStartUI()
    {
        if (ImGui.Button("Roll"))
            Roll();
        
        int knightIndex = -1;
        for (int i = 0; i < m_DevCards.Count; i++)
            if (m_DevCards[i] is Knight && m_DevCards[i].Playable)
                knightIndex = i;

        if (knightIndex != -1)
        {
            ImGui.SameLine();
            if (ImGui.Button("Play Knight"))
                PlayCard(knightIndex);
        }
    }

    private void TurnMainUI()
    {
        if (ImGui.BeginTabBar("TurnOptions"))
        {
            if (ImGui.BeginTabItem("Build"))
            {
                if (ImGui.Button("Settlement"))
                    TryBuildSettlement();

                ImGui.SameLine();

                if (ImGui.Button("Road"))
                    TryBuildRoad();

                ImGui.SameLine();

                if (ImGui.Button("City"))
                    TryBuildCity();

                ImGui.SameLine();
                if (ImGui.Button("Development Card"))
                    TryGetDevCard();

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Trade"))
            {
                OfferTradeUI();
                ImGui.EndTabItem();
            }

            if (m_DevCards.Count > 0)
                if (ImGui.BeginTabItem("Dev Cards"))
                {
                    string[] cards = new string[m_DevCards.Count];

                    for (int i = 0; i < m_DevCards.Count; i++)
                        cards[i] = m_DevCards[i].Name;

                    ImGui.ListBox("Development Cards", ref m_SelectedCard, cards, cards.Length);

                    if (!m_DevCards[m_SelectedCard].Playable)
                        ImGui.Text("Unplayable");

                    else if (ImGui.Button("Use"))
                    {
                        m_DevCards[m_SelectedCard].Activate(this);
                    }

                    ImGui.EndTabItem();
                }

            ImGui.EndTabBar();
            ImGui.Separator();
        }

        if (ImGui.Button("End Turn"))
            EndTurn();
    }

    private void PlayCard(int pos)
    {
        if (pos >= m_DevCards.Count)
            return;
        
        else if (!m_DevCards[pos].Playable)
            return;
        
        m_DevCards[pos].Activate(this);
        m_DevCards.RemoveAt(pos);

        for (int i = 0; i < m_DevCards.Count; i++)
            m_DevCards[i].Playable = false;
    }

    private void OfferTradeUI()
    {
        bool bank = m_CurrentTrade.To == m_GameBoard.ResourceBank;

        if (ImGui.Checkbox("Bank Trade", ref bank))
            m_CurrentTrade.To = bank ? m_GameBoard.ResourceBank : null;

        if (bank)
        {
            ImGui.Text("Exchange rate");
            m_ExchangeRate.UIDraw();
        }
        ImGui.Separator();
        
        m_CurrentTrade.UIDraw();

        ImGui.Separator();

        if (ImGui.Button("Trade"))
        {
            m_CurrentTrade.From = ResourceHand;

            if (bank)
            {
                for (Resources.Type i = 0; (int)i < 5; i++)
                    if (m_CurrentTrade.Giving.GetType(i) % m_ExchangeRate.GetType(i) != 0)
                        return;

                if (m_CurrentTrade.Giving.GetTotal() / 4 == m_CurrentTrade.Receiving.GetTotal())
                    m_CurrentTrade.TryExecute();
            }

            else
                m_GameBoard.PostTrade(m_CurrentTrade);
        }
    }

    private void TradeAcceptUI()
    {
        m_CurrentTrade.UIDraw(false);

        ImGui.Separator();

        if (ImGui.Button("Accept"))
        {
            m_CurrentTrade.To = ResourceHand;
            m_CurrentTrade.TryExecute();
            EndTurn();
        }

        ImGui.SameLine();

        if (ImGui.Button("Decline"))
            EndTurn();
    }

    private void DiscardUI()
    {
        int discardTarget = GetHandSize() / 2;
        m_CurrentTrade.Giving.UIDraw(true);

        if (ImGui.Button("Discard") && m_CurrentTrade.Giving.GetTotal() == discardTarget)
        {
            m_CurrentTrade.To = m_GameBoard.ResourceBank;
            m_CurrentTrade.From = ResourceHand;

            if (m_CurrentTrade.TryExecute())
            {
                EndTurn();
            }
        }
    }

    private void RobberUI()
    {
        if (ImGui.Button("Move Robber") && m_SelectedTile != null)
        {
            if (m_SelectedTile.Robber)
                return;

            bool targetablePlayer = false;
            foreach (Node node in m_SelectedTile.Nodes)
                if (node.Owner != null && node.Owner != this)
                    targetablePlayer = true;
            
            if (targetablePlayer)
            {
                if (m_SelectedNode == null)
                    return;
                
                else if (m_SelectedNode.Owner == null || m_SelectedNode.Owner == this)
                    return;

                bool adjacent = false;
                foreach (Node node in m_SelectedTile.Nodes)
                    if (node == m_SelectedNode)
                        adjacent = true;
                
                if (!adjacent)
                    return;
                
                GiveResource(m_SelectedNode.Owner.StealResource());
            }

            m_GameBoard.MoveRobber(m_SelectedTile);

            if (m_TurnState == TurnState.PreRollRobber)
                SetState(TurnState.Start);

            else
                SetState(TurnState.Main);
        }
    }

    private void RoadBuildingUI()
    {
        if (ImGui.Button("Build Road"))
            if (TryBuildRoad(true))
            {
                if (m_TurnState == TurnState.RoadBuilding)
                    SetState(TurnState.RoadBuilding2);
                
                else
                    SetState(TurnState.Main);
            }
    }

    private void YearOfPlentyUI()
    {
        m_CurrentTrade.Giving.UIDraw(true);

        if (ImGui.Button("Take") && m_CurrentTrade.Giving.GetTotal() == 2)
        {
            m_CurrentTrade.From = m_GameBoard.ResourceBank;
            m_CurrentTrade.To = ResourceHand;

            if (m_CurrentTrade.TryExecute())
                SetState(TurnState.Main);
        }
    }

    private void MonopolyUI()
    {
        for (Resources.Type i = 0; (int)i < 5; i++)
            {
                if ((int)i != 0)
                    ImGui.SameLine();

                if (ImGui.Button(i.ToString()))
                {
                    m_GameBoard.Monopoly(i);
                    SetState(TurnState.Main);
                }
            }
    }

    /// <summary>
    /// Attempt to build a settlement
    /// </summary>
    private bool TryBuildSettlement(bool ignoreCost = false)
    {
        if (m_SelectedNode == null)
            return false;
        
        if (m_SelectedNode.IsAvailable(this) && m_Pieces.Settlements > 0)
        {
            Trade trade = new Trade();
            trade.From = ResourceHand;
            trade.To = m_GameBoard.ResourceBank;
            trade.Giving = SETTLEMENT_COST;

            if (ignoreCost ? true : trade.TryExecute())
            {
                m_SelectedNode.Owner = this;
                m_Pieces.Settlements--;
                m_VictoryPoints++;
                m_OwnedNodes.Add(m_SelectedNode);
                UpdateExchange(m_SelectedNode.PortType);
                m_GameBoard.CheckLongestRoad(true);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Updates exchange rate when claiming new nodes
    /// </summary>
    private void UpdateExchange(Port.TradeType port)
    {
        if (port == Port.TradeType.Empty)
            return;
        
        else if (port == Port.TradeType.Versatile)
        {
            for (Resources.Type i = 0; (int)i < 5; i++)
                if (m_ExchangeRate.GetType(i) > 3)
                    m_ExchangeRate.SetType(i, 3);
        }
        
        else
            m_ExchangeRate.SetType((Resources.Type)port, 2);
    }
    
    /// <summary>
    /// Attempt to build a road
    /// </summary>
    private bool TryBuildRoad(bool ignoreCost = false)
    {
        if (m_SelectedEdge == null)
            return false;
        
        if (m_SelectedEdge.IsAvailable(this) && m_Pieces.Roads > 0)
        {
            Trade trade = new Trade();
            trade.From = ResourceHand;
            trade.To = m_GameBoard.ResourceBank;

            if (!ignoreCost)
                trade.Giving = ROAD_COST;

            if (trade.TryExecute() || ignoreCost)
            {
                m_SelectedEdge.Owner = this;
                m_Pieces.Roads--;

                FindLongestRoad();
                m_GameBoard.CheckLongestRoad(false);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Attempt to build a city
    /// </summary>
    private bool TryBuildCity()
    {
        if (m_SelectedNode == null)
            return false;
        
        if (m_SelectedNode.Owner == this && m_SelectedNode.IsCity == false && m_Pieces.Cities > 0)
        {
            Trade trade = new Trade();
            trade.From = ResourceHand;
            trade.To = m_GameBoard.ResourceBank;
            trade.Giving = CITY_COST;

            if (trade.TryExecute())
            {
                m_SelectedNode.IsCity = true;
                m_Pieces.Settlements++;
                m_Pieces.Cities--;
                m_VictoryPoints++;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Attempt to buy a development card
    /// </summary>
    private bool TryGetDevCard()
    {
        if (m_GameBoard.DevelopmentCards.Count < 0)
            return false;

        Trade trade = new Trade();
        trade.From = ResourceHand;
        trade.To = m_GameBoard.ResourceBank;
        trade.Giving = DEVELOPMENT_CARD_COST;

        if (trade.TryExecute())
        {
            m_DevCards.Add(m_GameBoard.DevelopmentCards.Dequeue());
            return true;
        }

        return false;
    }

    /// <summary>
    /// Different potential states
    /// </summary>
    public enum TurnState {
        PreGame1,
        Pregame2,
        Start,
        Main,
        Discard,
        Robber,
        PreRollRobber,
        RoadBuilding,
        RoadBuilding2,
        YearOfPlenty,
        Monopoly,
        Trade,
        End
    }

    /// <summary>
    /// Display player colour
    /// </summary>
    /// <value></value>
    public Color Colour { get; private set; }

    // Largest Army

    /// <summary>
    /// Played knight cards
    /// </summary>
    /// <value></value>
    public int ArmySize { get; set; }

    /// <summary>
    /// Boolean largest army victory managed by game
    /// </summary>
    public bool LargestArmy { get; set; }

    // Longest Road

    /// <summary>
    /// Segment length of longest road
    /// </summary>
    public int RoadLength { get; private set; }

    /// <summary>
    /// Boolean longest road victory card, managed by game
    /// </summary>
    public bool LongestRoad { get; set; }

    /// <summary>
    /// List describing longest road
    /// </summary>
    private List<Edge> m_Road;

    /// <summary>
    /// Debug longest road highlight
    /// </summary>
    private bool m_Highlighted;

    /// <summary>
    /// Current state
    /// </summary>
    private TurnState m_TurnState;

    /// <summary>
    /// Owned resource cards
    /// </summary>
    public Resources ResourceHand;

    /// <summary>
    /// Stores trade whilst being created
    /// </summary>
    private Trade m_CurrentTrade;

    /// <summary>
    /// Exchange rate with bank
    /// Starts at 4,4,4,4,4 & varies based on ports
    /// </summary>
    private Resources m_ExchangeRate;

    /// <summary>
    /// Reference to the game board
    /// </summary>
    private Board m_GameBoard;

    // Actively selected elements by player
    // Cleared at end of turn

    private Node m_SelectedNode;
    private Edge m_SelectedEdge;
    private Tile m_SelectedTile;

    /// <summary>
    /// Owned developments cards
    /// </summary>
    private List<DevelopmentCard> m_DevCards;
    private int m_SelectedCard;

    private struct Pieces
    {
        public Pieces()
        {
            Settlements = 3;
            Cities = 4;
            Roads = 13;
        }

        public int Settlements;
        public int Cities;
        public int Roads;
    }
    /// <summary>
    /// Limit for number of pieces the player can place
    /// </summary>
    private Pieces m_Pieces;

    /// <summary>
    /// List of all nodes owned by this player
    /// Primarily used as starting points for finding longest road
    /// </summary>
    private List<Node> m_OwnedNodes;

    /// <summary>
    /// Victory points owned by player
    /// </summary>
    private int m_VictoryPoints;

    // Static variables showing costs for different elements
    private static readonly Resources ROAD_COST = new Resources(1, 1, 0, 0, 0);
    private static readonly Resources SETTLEMENT_COST = new Resources(1, 1, 1, 1, 0);
    private static readonly Resources CITY_COST = new Resources(0, 0, 2, 0, 3);
    private static readonly Resources DEVELOPMENT_CARD_COST = new Resources(0, 0, 1, 1, 1);
}