using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ImGuiNET;

namespace Catan;

class Player
{
    public Player(Catan board, Color colour)
    {
        m_GameBoard = board;
        m_TurnState = TurnState.End;
        m_SelectedNode = null;
        m_SelectedEdge = null;
        m_SelectedTile = null;
        Colour = colour;

        m_SelectedCard = 0;

        RoadLength = 0;
        ArmySize = 0;

        m_Status = new PlayerStatus();
        m_Road = new List<Edge>();
        m_Highlighted = false;

        m_ControlledNodes = new List<NodeContainer>();
        m_OwnedEdges = new List<EdgeContainer>();

        m_CurrentTrade = new Trade(m_GameBoard);
    }

    public virtual void StartGame()
    {}

    public void GiveResource(Resources.Type resource, int num = 1)
    {
        m_Status.HeldResources.AddType(resource, num);
    }

    public Resources.Type StealResource()
    {
        return m_Status.HeldResources.Steal();
    }

    public void StartTurn()
    {
        m_TurnState = TurnState.Start;
    }

    public void SetState(TurnState state)
    {
        if (state == TurnState.Discard && m_Status.HeldResources.GetTotal() < 8)
            state = TurnState.End;

        else if (state == TurnState.Robber && m_TurnState == TurnState.Start)
            state = TurnState.PreRollRobber;

        if (state != TurnState.Trade)
            m_CurrentTrade = new Trade(m_GameBoard);

        m_TurnState = state;
    }

    public void SetActiveTrade(Trade trade)
    {
        m_CurrentTrade = trade;
    }

    public Resources GetHand()
    {
        return m_Status.HeldResources;
    }

    public virtual void OnTradeComplete(Trade trade)
    { }

    protected void Roll()
    {
        if (m_TurnState != TurnState.Start)
            return;

        m_GameBoard.RollDice();
        SetState(TurnState.Main);
    }

    protected void EndTurn()
    {
        FindLongestRoad();

        m_TurnState = TurnState.End;
        m_CurrentTrade = new Trade(m_GameBoard);

        DeselectNode();
        DeselectEdge();
        DeselectTile();

        m_SelectedCard = 0;

        for (int i = 0; i < m_Status.DevelopmentCards.Count; i++)
            m_Status.DevelopmentCards[i].Playable = true;
    }

    public bool HasWon()
    {      
        return m_Status.VictoryPoints + (LargestArmy ? 2 : 0) + (LongestRoad ? 2 : 0) + GetHiddenVP() >= 10;
    }

    /// <summary>
    /// Get Victory Points from development cards
    /// </summary>
    private int GetHiddenVP()
    {
        int victoryCards = 0;
        foreach (DevelopmentCard developmentCard in m_Status.DevelopmentCards)
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
        return m_Status.HeldResources.GetTotal();
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

    protected void DeselectNode()
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

    protected void DeselectEdge()
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

    protected void DeselectTile()
    {
        if (m_SelectedTile != null)
            m_SelectedTile.Selected = false;

        m_SelectedTile = null;
    }

    public virtual void Update()
    {}

    public virtual void SpriteDraw(SpriteBatch spriteBatch, SpriteFont font, float windowHeight)
    {}

    public void FindLongestRoad()
    {
        RoadLength = 0;

        int unSearched = m_OwnedEdges.Count;

        while (RoadLength < unSearched)
        {
            int i = 0;
            while (m_OwnedEdges[i].Traversed)
                i++;

            List<Edge> road = m_OwnedEdges[i].TraverseLongest(this);

            if (road.Count > RoadLength)
            {
                RoadLength = road.Count;
                m_Road = road;
            }

            unSearched = 0;
            foreach (EdgeContainer edge in m_OwnedEdges)
                if (!edge.Traversed)
                    unSearched++;
        }

        for (int i = 0; i < m_OwnedEdges.Count; i++)
            m_OwnedEdges[i].Traversed = false;
        
        for (int i = 0; i < m_ControlledNodes.Count; i++)
            m_ControlledNodes[i].Traversed = false;
    }

    /// <summary>
    /// Debug menu UI
    /// </summary>
    public virtual void DebugDrawUI()
    {
        ImGui.Text(string.Format("State: {0}", m_TurnState.ToString()));
        ImGui.Text(string.Format("VP: {0}", m_Status.VictoryPoints + (LargestArmy ? 2 : 0) + (LongestRoad ? 2 : 0)));
        ImGui.Text(string.Format("Resource Cards: {0}", GetHandSize()));
        ImGui.Text(string.Format("Longest Road: {0}", RoadLength));
        ImGui.Separator();

        if (ImGui.Button("Calculate Longest Road"))
            FindLongestRoad();

        ImGui.Checkbox("Highlight Road", ref m_Highlighted);
        for (int i = 0; i < m_Road.Count; i++)
            m_Road[i].Selected = m_Highlighted;

        m_Status.HeldResources.UIDraw(true);
    }


    // Gameplay UI

    public void GameDrawUI()
    {
        if (m_TurnState == TurnState.End)
            return;

        m_Status.HeldResources.UIDraw();

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

        if (m_TurnState == TurnState.Pregame2)
        {
            Trade trade = new Trade(m_GameBoard);
            trade.To = m_Status.HeldResources;
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
        for (int i = 0; i < m_Status.DevelopmentCards.Count; i++)
            if (m_Status.DevelopmentCards[i] is Knight && m_Status.DevelopmentCards[i].Playable)
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
                    TryBuildSettlement(m_SelectedNode);

                ImGui.SameLine();

                if (ImGui.Button("Road"))
                    TryBuildRoad(m_SelectedEdge);

                ImGui.SameLine();

                if (ImGui.Button("City"))
                    TryBuildCity(m_SelectedNode);

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

            if (m_Status.DevelopmentCards.Count > 0)
                if (ImGui.BeginTabItem("Dev Cards"))
                {
                    string[] cards = new string[m_Status.DevelopmentCards.Count];

                    for (int i = 0; i < m_Status.DevelopmentCards.Count; i++)
                        cards[i] = m_Status.DevelopmentCards[i].Name;

                    ImGui.ListBox("Development Cards", ref m_SelectedCard, cards, cards.Length);

                    if (!m_Status.DevelopmentCards[m_SelectedCard].Playable)
                        ImGui.Text("Unplayable");

                    else if (ImGui.Button("Use"))
                    {
                        m_Status.DevelopmentCards[m_SelectedCard].Activate(this);
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
        if (pos >= m_Status.DevelopmentCards.Count)
            return;
        
        else if (!m_Status.DevelopmentCards[pos].Playable)
            return;
        
        m_Status.DevelopmentCards[pos].Activate(this);
        m_Status.DevelopmentCards.RemoveAt(pos);

        for (int i = 0; i < m_Status.DevelopmentCards.Count; i++)
            m_Status.DevelopmentCards[i].Playable = false;
    }

    private void OfferTradeUI()
    {
        bool bank = m_CurrentTrade.To == null;

        if (ImGui.Checkbox("Bank Trade", ref bank))
            m_CurrentTrade.To = bank ? m_GameBoard.ResourceBank : null;

        if (bank)
        {
            ImGui.Text("Exchange rate");
            m_Status.ExchangeRate.UIDraw();
        }
        ImGui.Separator();
        
        m_CurrentTrade.UIDraw();

        ImGui.Separator();

        if (ImGui.Button("Trade"))
        {
            m_CurrentTrade.From = m_Status.HeldResources;

            if (bank)
            {
                for (Resources.Type i = 0; (int)i < 5; i++)
                    if (m_CurrentTrade.Giving.GetType(i) % m_Status.ExchangeRate.GetType(i) != 0)
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
            m_CurrentTrade.To = m_Status.HeldResources;
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
            m_CurrentTrade.From = m_Status.HeldResources;

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
            if (!TryMoveRobber(m_SelectedTile, m_SelectedNode))
                return;            

            else if (m_TurnState == TurnState.PreRollRobber)
                SetState(TurnState.Start);

            else
                SetState(TurnState.Main);
        }
    }

    protected bool TryMoveRobber(Tile targetTile, Node targetNode)
    {
        if (targetTile.Robber)
            return false;
        
        bool targetablePlayer = false;
        foreach (Node node in targetTile.Nodes)
            if (node.Owner != null && node.Owner != this)
                targetablePlayer = true;
        
        if (targetablePlayer)
        {
            if (targetNode == null)
                return false;
            
            else if (targetNode.Owner == null || targetNode.Owner == this)
                return false;

            bool adjacent = false;
            foreach (Node node in targetTile.Nodes)
                if (node == targetNode)
                    adjacent = true;
            
            if (!adjacent)
                return false;
            
            GiveResource(targetNode.Owner.StealResource());
        }

        m_GameBoard.MoveRobber(targetTile);
        return true;
    }

    private void RoadBuildingUI()
    {
        if (ImGui.Button("Build Road"))
            if (TryBuildRoad(m_SelectedEdge))
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
            m_CurrentTrade.To = m_Status.HeldResources;

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
    protected bool TryBuildSettlement(Node targetNode)
    {
        bool freeResource = m_TurnState == TurnState.Pregame2;
        bool purchased = m_TurnState == TurnState.PreGame1 || freeResource;

        if (targetNode == null)
            return false;
        
        else if (targetNode.IsAvailable(purchased ? null : this) && m_Status.UnbuiltSettlements > 0)
        {
            Trade trade = new Trade(m_GameBoard);
            trade.From = m_Status.HeldResources;
            trade.To = m_GameBoard.ResourceBank;
            trade.Giving = SETTLEMENT_COST;

            if (!purchased)
                purchased = trade.TryExecute();

            if (purchased)
            {
                if (freeResource)
                {
                    trade.Giving = new Resources();

                    foreach (Tile tile in targetNode.Tiles)
                        if (tile != null)
                            trade.Receiving.AddType(tile.Type, 1);
                    
                    trade.TryExecute();
                }

                targetNode.Owner = this;
                m_Status.UnbuiltSettlements--;
                m_Status.VictoryPoints++;
                UpdateExchange(targetNode.PortType);
                m_GameBoard.CheckLongestRoad(true);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Updates exchange rate when claiming new nodes
    /// </summary>
    protected void UpdateExchange(Port.TradeType port)
    {
        if (port == Port.TradeType.Empty)
            return;
        
        else if (port == Port.TradeType.Versatile)
        {
            for (Resources.Type i = 0; (int)i < 5; i++)
                if (m_Status.ExchangeRate.GetType(i) > 3)
                    m_Status.ExchangeRate.SetType(i, 3);
        }
        
        else
            m_Status.ExchangeRate.SetType((Resources.Type)port, 2);
    }
    
    /// <summary>
    /// Attempt to build a road
    /// </summary>
    protected bool TryBuildRoad(Edge targetEdge)
    {
        bool purchased = 
            m_TurnState == TurnState.PreGame1 || m_TurnState == TurnState.Pregame2
            || m_TurnState == TurnState.RoadBuilding || m_TurnState == TurnState.RoadBuilding2;

        if (targetEdge == null)
            return false;
        
        if (targetEdge.IsAvailable(this) && m_Status.UnbuiltRoads > 0)
        {
            Trade trade = new Trade(m_GameBoard);
            trade.From = m_Status.HeldResources;
            trade.To = m_GameBoard.ResourceBank;
            trade.Giving = ROAD_COST;

            if (!purchased)
                purchased = trade.TryExecute();

            if (purchased)
            {
                targetEdge.Owner = this;
                m_Status.UnbuiltRoads--;

                NodeContainer.LinkNewEdge(targetEdge, ref m_ControlledNodes, ref m_OwnedEdges);

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
    protected bool TryBuildCity(Node target)
    {
        if (target == null)
            return false;
        
        if (target.Owner == this && target.IsCity == false && m_Status.UnbuiltCities > 0)
        {
            Trade trade = new Trade(m_GameBoard);
            trade.From = m_Status.HeldResources;
            trade.To = m_GameBoard.ResourceBank;
            trade.Giving = CITY_COST;

            if (trade.TryExecute())
            {
                target.IsCity = true;
                m_Status.UnbuiltSettlements++;
                m_Status.UnbuiltCities--;
                m_Status.VictoryPoints++;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Attempt to buy a development card
    /// </summary>
    protected bool TryGetDevCard()
    {
        if (m_GameBoard.DevelopmentCards.Count < 0)
            return false;

        Trade trade = new Trade(m_GameBoard);
        trade.From = m_Status.HeldResources;
        trade.To = null;
        trade.Giving = DEVELOPMENT_CARD_COST;

        if (trade.TryExecute())
        {
            m_Status.DevelopmentCards.Add(m_GameBoard.DevelopmentCards.Dequeue());
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
    protected TurnState m_TurnState;

    /// <summary>
    /// Stores trade whilst being created
    /// </summary>
    private Trade m_CurrentTrade;

    /// <summary>
    /// Reference to the game board
    /// </summary>
    protected Catan m_GameBoard { get; private set; }

    // Actively selected elements by player
    // Cleared at end of turn

    protected Node m_SelectedNode;
    protected Edge m_SelectedEdge;
    protected Tile m_SelectedTile;
    private int m_SelectedCard;

    protected class EdgeContainer
    {
        public EdgeContainer(Edge edge, ref List<NodeContainer> nodes)
        {
            RefEdge = edge;
            ConnectedNodes = new List<NodeContainer>();

            int nodeCount = 0;
            foreach (NodeContainer node in nodes)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (node.RefNode == RefEdge.Nodes[i])
                    {
                        node.ConnectedEdges.Add(this);
                        ConnectedNodes.Add(node);

                        if (++nodeCount > 1)
                            return;
                        
                        break;
                    }
                }
            }
        }

        public List<Edge> TraverseLongest(in Player player)
        {
            if (Traversed)
                return new List<Edge>();
            
            Traversed = true;

            List<Edge> longest = new List<Edge>();

            foreach (NodeContainer node in ConnectedNodes)
                longest.AddRange(node.TraverseLongest(player));

            longest.Add(RefEdge);
            return longest;
        }

        public Edge RefEdge { get; private set; }
        public bool Traversed = false;
        public List<NodeContainer> ConnectedNodes { get; private set; }
    }
    protected List<EdgeContainer> m_OwnedEdges;

    protected class NodeContainer
    {
        public NodeContainer(Node node, ref List<EdgeContainer> edges)
        {
            RefNode = node;
            ConnectedEdges = new List<EdgeContainer>();

            int edgeCount = 0;
            foreach (EdgeContainer edge in edges)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (edge.RefEdge == RefNode.Edges[i])
                    {
                        edge.ConnectedNodes.Add(this);
                        ConnectedEdges.Add(edge);
                        
                        if (++edgeCount > 2)
                            return;
                        
                        break;
                    }
                }
            }
        }

        public List<Edge> TraverseLongest(in Player player)
        {
            if (Traversed || (RefNode.Owner != player && RefNode.Owner != null))
                return new List<Edge>();

            Traversed = true;

            List<Edge> longest = new List<Edge>();

            foreach(EdgeContainer edge in ConnectedEdges)
            {
                List<Edge> current = edge.TraverseLongest(player);

                if (current.Count > longest.Count)
                    longest = current;
            }

            return longest;
        }

        public Node RefNode { get; private set; }
        public List<EdgeContainer> ConnectedEdges { get; private set; }
        public bool Traversed = false;

        public static void LinkNewEdge(Edge newEdge, ref List<NodeContainer> nodes, ref List<EdgeContainer> edges)
        {
            bool exists = false;
            
            int index = 0;
            while (!exists && index < edges.Count)
            {
                exists = edges[index++].RefEdge == newEdge;
            }

            if (exists)
                return;
            
            bool[] existingNodes = new bool[]{ false, false };

            index = 0;
            while ((!existingNodes[0] || !existingNodes[1]) && index < nodes.Count)
            {
                existingNodes[0] = existingNodes[0] || (nodes[index].RefNode == newEdge.Nodes[0]);
                existingNodes[1] = existingNodes[1] || (nodes[index].RefNode == newEdge.Nodes[1]);

                index++;
            }

            for (int i = 0; i < 2; i++)
                if (!existingNodes[i])
                    nodes.Add(new NodeContainer(newEdge.Nodes[i], ref edges));
            
            edges.Add(new EdgeContainer(newEdge, ref nodes));
        }
    }
    protected List<NodeContainer> m_ControlledNodes;

    /// <summary>
    /// Container describing current player state
    /// </summary>
    public struct PlayerStatus : ICloneable
    {
        public PlayerStatus()
        {
            UnbuiltSettlements = 5;
            UnbuiltCities = 4;
            UnbuiltRoads = 15;

            DevelopmentCards = new List<DevelopmentCard>();

            VictoryPoints = 0;

            HeldResources = new Resources();
            ExchangeRate = new Resources(4, 4, 4, 4, 4);
        }

        public readonly object Clone()
        {
            PlayerStatus clone = new PlayerStatus();

            clone.UnbuiltSettlements = UnbuiltSettlements;
            clone.UnbuiltCities = UnbuiltCities;
            clone.UnbuiltRoads = UnbuiltRoads;

            clone.DevelopmentCards = new List<DevelopmentCard>(DevelopmentCards);

            clone.VictoryPoints = VictoryPoints;

            clone.HeldResources = (Resources)HeldResources.Clone();
            clone.ExchangeRate = (Resources)ExchangeRate.Clone();

            return clone;
        }

        public int UnbuiltSettlements;
        public int UnbuiltCities;
        public int UnbuiltRoads;

        public List<DevelopmentCard> DevelopmentCards;

        public int VictoryPoints;

        public Resources HeldResources;
        public Resources ExchangeRate;
    }
    protected PlayerStatus m_Status;

    public PlayerStatus GetStatus()
    {
        return (PlayerStatus)m_Status.Clone();
    }

    // Static variables showing costs for different elements
    public static readonly Resources ROAD_COST = new Resources(1, 1, 0, 0, 0);
    public static readonly Resources SETTLEMENT_COST = new Resources(1, 1, 1, 1, 0);
    public static readonly Resources CITY_COST = new Resources(0, 0, 2, 0, 3);
    public static readonly Resources DEVELOPMENT_CARD_COST = new Resources(0, 0, 1, 1, 1);
}