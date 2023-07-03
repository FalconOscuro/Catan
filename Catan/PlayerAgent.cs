using System;

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
            TryBuildSettlement(GetHighestValueNode());
            EndTurn();
            break;
        }

        m_Continue = !m_Step;
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
            if (ImGui.Button("Re-calculate rarity"))
                    CalculateRarity();
        
            if (ImGui.CollapsingHeader("Stats"))
            {
                ImGui.Text("Resource abundance");
                m_ResourceRarity.UIDraw();
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

    private float[] m_NodeWeights = new float[54];

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

    private static readonly ResourceWeights PRIMARY_RESOURCES = new ResourceWeights(.2950f, .3233f, .3675f, .3100f, .3233f);
}