using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ImGuiNET;

namespace Catan;

/// <summary>
/// Autonomus player
/// </summary>
class PlayerAgent : Player
{
    public PlayerAgent(Board board, Color colour):
        base(board, colour)
    {
        m_ResourceRarity = null;
        m_Predictions = new Dictionary<Player, Resources>();
    }

    public override void StartGame()
    {
        m_Predictions = new Dictionary<Player, Resources>();
        base.StartGame();

        foreach (Player player in m_GameBoard.Players)
        {
            if (player == this)
                continue;

            m_Predictions.Add(player, new Resources());
        }
    }

    public override void OnTradeComplete(Trade trade)
    {
        base.OnTradeComplete(trade);

        if (trade.From != this && trade.From != null)
        {
            Resources prediction = m_Predictions[trade.From];
            prediction = (prediction + trade.Receiving) - trade.Giving;

            m_Predictions[trade.From] = prediction;
        }

        if (trade.To != this && trade.To != null)
        {
            Resources prediction = m_Predictions[trade.To];
            prediction = (prediction + trade.Giving) - trade.Receiving;

            m_Predictions[trade.To] = prediction;
        }
    }

    public override void Update()
    {
        CalculateWeights();

        if (m_ResourceRarity == null)
            CalculateRarity();
        
        if (!m_Continue)
            return;

        switch (m_TurnState)
        {
        case TurnState.PreGame1:
        case TurnState.Pregame2:
            Node node = GetHighestValueNode();
            TryBuildSettlement(node);
            TryBuildRoad(FindBestEdge(node));
            EndTurn();
            break;

        case TurnState.Start:
            Roll();
            break;

        case TurnState.Main:
            TurnMain();
            break;
        }
        m_Continue = !m_Step;
    }

    private void TurnMain()
    {
        Node target = CanBuildSettlement();

        if (TryBuildSettlement(target))
            return;

        EndTurn();
    }

    public override void SpriteDraw(SpriteBatch spriteBatch, SpriteFont font, float windowHeight)
    {
        if (m_ShowWeights)
            for (int i = 0; i < 54; i++)
                spriteBatch.DrawString(
                    font, String.Format("{0:0.00}", m_NodeWeights[i] * 10), 
                    m_GameBoard.Nodes[i].Position.FlipY(windowHeight),
                    Color.Yellow
                    );
    }

    public override void DebugDrawUI()
    {
        base.DebugDrawUI();

        if (ImGui.Checkbox("Step mode", ref m_Step))
            m_Continue = !m_Step;

        if (!m_Continue)
            m_Continue = ImGui.Button("Step");

        if (ImGui.CollapsingHeader("Weights"))
        {

            ImGui.Checkbox("Show Weights", ref m_ShowWeights);

            ImGui.Checkbox("Resource weights", ref m_WeightResources);

            int placement = (int)m_PlacementStrategy;
            if (ImGui.Combo("Method", ref placement, PLACEMENT_STRATEGIES, 3))
                m_PlacementStrategy = (PlacementStrategy)placement;
        
            int neighbourSearch = (int)m_NeighbourSearch;
            if (ImGui.Combo("Neighbour Search", ref neighbourSearch, NEIGHBOUR_SEARCH, 3))
                m_NeighbourSearch = (NeighbourSearch)neighbourSearch;

            if (ImGui.Button("Re-calculate weights"))
                CalculateWeights();

            ImGui.SameLine();
            if (ImGui.Button("Calculate distances"))
                CalculateDistances();

            ImGui.SameLine();
            if (ImGui.Button("Re-calculate rarity"))
                    CalculateRarity();
        
            if (ImGui.CollapsingHeader("Stats"))
            {
                ImGui.Text("Resource abundance");
                m_ResourceRarity.UIDraw();
            }
        }

        if (ImGui.CollapsingHeader("Predictions"))
        {
            if (ImGui.BeginTabBar("Players"))
            {
                int i = 0;
                foreach (KeyValuePair<Player, Resources> prediction in m_Predictions)
                {
                    if (ImGui.BeginTabItem(string.Format("Prediction {0}", i++)))
                    {
                        prediction.Value.UIDraw();
                        ImGui.EndTabItem();
                    }
                }

                ImGui.EndTabBar();
            }
        }
    }

    private void CalculateWeights()
    {
        ResourceWeights weights = m_WeightResources ? PRIMARY_RESOURCES : new ResourceWeights();

        for (int i = 0; i < 54; i++)
            m_NodeWeights[i] = m_GameBoard.Nodes[i].GetWeight(
                m_ResourceRarity, m_PlacementStrategy, weights, m_NeighbourSearch);
    }

    private void CalculateDistances()
    {
        m_DistanceMap = new float[54];
        float step = 1f / (m_SearchDepth + 1);

        for (int i = 0; i < 54; i++)
        {
            Node node  = m_GameBoard.Nodes[i];

            if(node.IsAvailable(this))
               ApplyDistances(i, step);

            else if (!node.IsAvailable())
            {
                for (int j = 0; j < 3; j++)
                {
                   Node neighbour = node.GetNeighbourNode(j);
                   
                    if (neighbour == null)
                        continue;
                    
                    else if (neighbour.Owner == this)
                        ApplyDistances(i, step);
                }
            }
        }
    }

    private Edge FindBestEdge(Node node)
    {
        float maxWeight = 0f;
        Edge maxEdge = null;

        for (int i = 0; i < 3; i++)
        {
            float weight = 0f;
            Edge edge = node.Edges[i];
            if (edge == null)
                continue;

            else if (edge.Owner != null)
                continue;

            Node neighbour = node.GetNeighbourNode(i);
            
            for (int j = 0; j < 3; j++)
            {
                Edge edge1 = neighbour.Edges[j];

                if (edge1 == null)
                    continue;
                
                Node search = neighbour.GetNeighbourNode(i);
                if (edge1.Owner != null || search.Owner != null || search == node)
                    continue;
                
                weight += m_NodeWeights[search.ID]; 
            }
            weight /= 2;

            if (weight > maxWeight)
            {
                maxWeight = weight;
                maxEdge = edge;
            }
        }

        return maxEdge;
    }

    private void ApplyDistances(int index, float step, float value = 1f)
    {
        if (m_DistanceMap[index] >= value)
            return;
        
        m_DistanceMap[index] = value;
        value -= step;

        if (value < step - (step / 2))
            return;

        Node node = m_GameBoard.Nodes[index];
        for (int i = 0; i < 3; i++)
        {
            Node neighbour = node.GetNeighbourNode(i);
            if (neighbour == null)
                continue;
            
            ApplyDistances(neighbour.ID, step, value);
        }
    }

    private void CalculateRarity()
    {
        m_ResourceRarity = new Resources();

        foreach(Tile tile in m_GameBoard.Tiles)
            m_ResourceRarity.AddType(tile.Type, tile.GetProbability());
    }

    private Node GetHighestValueNode()
    {
        int index = 0;

        for (int i = 1; i < 54; i++)
            if (m_NodeWeights[i] > m_NodeWeights[index])
                index = i;
        
        if (m_NodeWeights[index] == 0f)
            return null;

        return m_GameBoard.Nodes[index];
    }

    private Node CanBuildSettlement()
    {
        if (ResourceHand < SETTLEMENT_COST)
            return null;
        
        float targetWeight = 0f;
        Node target = null;
        foreach (Node node in m_GameBoard.Nodes)
        {
            if (node.IsAvailable(this) && m_NodeWeights[node.ID] > targetWeight)
            {
                targetWeight = m_NodeWeights[node.ID];
                target = node;
            }
        }

        return target;
    }

    private float[] m_NodeWeights = new float[54];
    private float[] m_DistanceMap = new float[54];

    private int m_SearchDepth = 1;

    private bool m_ShowWeights = false;

    public enum PlacementStrategy
    {
        MaxCards,
        Abundance,
        Rarity
    }
    private PlacementStrategy m_PlacementStrategy;
    private static readonly string[] PLACEMENT_STRATEGIES = 
        new string[]{PlacementStrategy.MaxCards.ToString(), 
            PlacementStrategy.Abundance.ToString(), 
            PlacementStrategy.Rarity.ToString()};

    public enum NeighbourSearch
    {
        Disabled,
        All,
        Best
    }
    private NeighbourSearch m_NeighbourSearch;
    private static readonly string[] NEIGHBOUR_SEARCH = 
        new string[]{NeighbourSearch.Disabled.ToString(),
            NeighbourSearch.All.ToString(),
            NeighbourSearch.Best.ToString()};

    private bool m_WeightResources = false;

    private bool m_Step = false;
    private bool m_Continue = true;

    private Resources m_ResourceRarity;

    private Dictionary<Player, Resources> m_Predictions;

    private static readonly ResourceWeights PRIMARY_RESOURCES = new ResourceWeights(.2950f, .3233f, .3675f, .3100f, .3233f);
}