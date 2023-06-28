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

        ResourceHand = new Resources();
        m_CurrentTrade = new Trade();
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

        m_TurnState = state;
    }

    private void Roll()
    {
        m_GameBoard.RollDice();
        m_TurnState = TurnState.Main;
    }

    private void EndTurn()
    {
        m_TurnState = TurnState.End;

        DeselectNode();
        DeselectEdge();
        DeselectTile();
    }

    public bool HasWon()
    {
        return m_VictoryPoints >= 10;
    }

    public bool HasTurnEnded()
    {
        return m_TurnState == TurnState.End;
    }

    public int GetHandSize()
    {
        return ResourceHand.GetTotal();
    }

    public void SelectNode(Node node)
    {
        if (m_TurnState == TurnState.End)
            return;

        DeselectNode();

        m_SelectedNode = node;
        m_SelectedNode.Selected = true;
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

        m_SelectedEdge = edge;
        m_SelectedEdge.Selected = true;
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

        m_SelectedTile = tile;
        m_SelectedTile.Selected = true;
    }

    private void DeselectTile()
    {
        if (m_SelectedTile != null)
            m_SelectedTile.Selected = false;

        m_SelectedTile = null;
    }

    public void DebugDrawUI()
    {
        ImGui.Text(string.Format("State: {0}", m_TurnState.ToString()));
        ImGui.Text(string.Format("VP: {0}", m_VictoryPoints));
        ImGui.Text(string.Format("Resource Cards: {0}", GetHandSize()));

        ImGui.Separator();

        ResourceHand.UIDraw(true);
    }

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
                RobberUI();
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
            Trade trade = new Trade();
            trade.To = ResourceHand;
            trade.From = m_GameBoard.ResourceBank;

            for (int i = 0; i < 3; i++)
                if (m_SelectedNode.Tiles[i] != null)
                    trade.Materials.AddType(m_SelectedNode.Tiles[i].Type, 1);
            
            trade.TryExecute();
        }

        EndTurn();
    }

    private void TurnStartUI()
    {
        if (ImGui.Button("Roll"))
            Roll();
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
                ImGui.Text("Hello there");
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
            ImGui.Separator();
        }

        if (ImGui.Button("End Turn"))
            EndTurn();
    }

    private void DiscardUI()
    {
        int discardTarget = GetHandSize() / 2;

        m_CurrentTrade.Materials.UIDraw(true);
        m_CurrentTrade.To = m_GameBoard.ResourceBank;
        m_CurrentTrade.From = ResourceHand;

        if (ImGui.Button("Discard") && m_CurrentTrade.Materials.GetTotal() == discardTarget)
            if (m_CurrentTrade.TryExecute())
            {
                m_TurnState = TurnState.End;
                m_CurrentTrade = new Trade();
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
            m_TurnState = TurnState.Start;
        }
    }

    private void TryBuildSettlement()
    {
        if (m_SelectedNode == null)
            return;
        
        if (m_SelectedNode.IsAvailable(this))
        {
            Trade trade = new Trade();
            trade.From = ResourceHand;
            trade.To = m_GameBoard.ResourceBank;
            trade.Materials = SETTLEMENT_COST;

            if (trade.TryExecute())
            {
                m_SelectedNode.Owner = this;
                m_VictoryPoints++;
            }
        }
    }
    
    private void TryBuildRoad()
    {
        if (m_SelectedEdge == null)
            return;
        
        if (m_SelectedEdge.IsAvailable(this))
        {
            Trade trade = new Trade();
            trade.From = ResourceHand;
            trade.To = m_GameBoard.ResourceBank;
            trade.Materials = ROAD_COST;

            if (trade.TryExecute())
                m_SelectedEdge.Owner = this;
        }
    }

    private void TryBuildCity()
    {
        if (m_SelectedNode == null)
            return;
        
        if (m_SelectedNode.Owner == this && m_SelectedNode.IsCity == false)
        {
            Trade trade = new Trade();
            trade.From = ResourceHand;
            trade.To = m_GameBoard.ResourceBank;
            trade.Materials = CITY_COST;

            if (trade.TryExecute())
            {
                m_SelectedNode.IsCity = true;
                m_VictoryPoints++;
            }
        }
    }

    private void TryGetDevCard()
    {
        Trade trade = new Trade();
        trade.From = ResourceHand;
        trade.To = m_GameBoard.ResourceBank;
        trade.Materials = DEVELOPMENT_CARD_COST;

        trade.TryExecute();
    }

    public enum TurnState {
        PreGame1,
        Pregame2,
        Start,
        Main,
        Discard,
        Robber,
        End
    }

    public Color Colour { get; private set; }

    private TurnState m_TurnState;

    public Resources ResourceHand;
    private Trade m_CurrentTrade;

    private Board m_GameBoard;

    private Node m_SelectedNode;
    private Edge m_SelectedEdge;
    private Tile m_SelectedTile;

    private int m_VictoryPoints;

    private bool[] m_Tabs = { true, false };

    private static readonly Resources ROAD_COST = new Resources(1, 1, 0, 0, 0);
    private static readonly Resources SETTLEMENT_COST = new Resources(1, 1, 1, 1, 0);
    private static readonly Resources CITY_COST = new Resources(0, 0, 2, 0, 3);
    private static readonly Resources DEVELOPMENT_CARD_COST = new Resources(0, 0, 1, 1, 1);
}