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
        m_Behaviour = new Behaviour();
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

        CalculateRarity();
    }

    public override void OnTradeComplete(Trade trade)
    {
        base.OnTradeComplete(trade);

        if (!m_Behaviour.TrackResources)
            return;

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
        GetResourceWeights();
        GetNodeWeights();
        
        if (!m_Continue)
            return;

        switch (m_TurnState)
        {
        case TurnState.PreGame1:
        case TurnState.Pregame2:
            Pregame();
            break;

        case TurnState.Start:
            Roll(); // Acount for robber
            break;

        case TurnState.Robber:
            Robber();
            break;

        case TurnState.Main:
            TurnMain();
            break;
        }
        m_Continue = !m_Step;
    }

    private void Pregame()
    {
        Node node = GetHighestValueNode();
        TryBuildSettlement(node);
        TryBuildRoad(FindBestEdge(node));
        EndTurn();
    }

    private void TurnMain()
    {
        Node target = CanBuildSettlement();

        if (TryBuildSettlement(target))
            return;

        EndTurn();
    }

    private void Robber()
    {
        m_RobberWeights = new float[19];
        Node targetNode = null;
        Tile targetTile = null;
        float maxWeight = 0f;

        for (int i = 0; i < 19; i++)
        {
            Tile tile = m_GameBoard.Tiles[i];

            if (tile.Robber)
                continue;

            float weight = GetTileWeight(m_ResourceRarity, m_Behaviour, TOTAL_WEIGHT, tile);
            float maxNodeWeight = 0f;
            Node maxNode = null;

            foreach (Node node in tile.Nodes)
            {
                float deltaWeight = weight;

                if (node.Owner == null)
                    continue;
                
                else if (node.Owner == this)
                    deltaWeight *= -m_Behaviour.RobberAvoidance;
                
                else
                {
                    deltaWeight *= m_Behaviour.RobberAggression;

                    if (m_Behaviour.EvaluateGain && m_Behaviour.TrackResources)
                    {
                        deltaWeight += 
                            (m_ResourceWeights.GetResourcesWeight(m_Predictions[node.Owner]) / node.Owner.GetHandSize()) * m_Behaviour.RobberThievery;
                    }

                    else
                        deltaWeight += (node.Owner.GetHandSize() / 7 * m_Behaviour.RobberThievery);
                }

                if (node.IsCity)
                    deltaWeight *= m_Behaviour.RobberCityMult;

                if (deltaWeight > maxNodeWeight)
                {
                    maxNodeWeight = deltaWeight;
                    maxNode = node;
                }

                m_RobberWeights[i] += deltaWeight;    
            }

            if (m_RobberWeights[i] > maxWeight)
            {
                maxWeight = m_RobberWeights[i];
                targetTile = m_GameBoard.Tiles[i];
                targetNode = maxNode;
            }
        }

        TryMoveRobber(targetTile, targetNode);

        m_TurnState = TurnState.Main;
    }

    public override void SpriteDraw(SpriteBatch spriteBatch, SpriteFont font, float windowHeight)
    {
        if (m_ShowWeights == WeightDisplay.Robber)
        {
            for (int i = 0; i < 19; i++)
                spriteBatch.DrawString(
                    font, String.Format("{0:0.00}", m_RobberWeights[i] * 10),
                    m_GameBoard.Tiles[i].Position.FlipY(windowHeight),
                    Color.Yellow
                );
        }

        else if (m_ShowWeights != WeightDisplay.Disabled)
        {
            for (int i = 0; i < 54; i++)
            {
                float weight = 0f;
                switch (m_ShowWeights)
                {
                case WeightDisplay.Tile:
                    weight = m_NodeWeights[i];
                    break;
                
                case WeightDisplay.Neighbour:
                    weight = m_NeighbourWeights[i];
                    break;

                case WeightDisplay.Distance:
                    weight = m_DistanceMap[i];
                    break;

                case WeightDisplay.Mixed:
                    weight = m_NodeWeights[i] + m_NeighbourWeights[i];
                    break;
                }

                spriteBatch.DrawString(
                    font, String.Format("{0:0.00}", weight * 10), 
                    m_GameBoard.Nodes[i].Position.FlipY(windowHeight),
                    Color.Yellow
                    );
            }
        }
    }

    public override void DebugDrawUI()
    {
        base.DebugDrawUI();
        ImGui.Separator();

        if (ImGui.Checkbox("Step mode", ref m_Step))
            m_Continue = !m_Step;

        if (!m_Continue)
            m_Continue = ImGui.Button("Step");

        if (ImGui.CollapsingHeader("Behaviour"))
            m_Behaviour.DrawDebugUI();
        ImGui.Separator();

        ImGui.Text("Calculate: ");
        ImGui.SameLine();
        if (ImGui.Button("Nodes"))
            GetNodeWeights();
        
        ImGui.SameLine();
        if (ImGui.Button("Distances"))
            FindDistances();
        
        ImGui.SameLine();
        if (ImGui.Button("Rarity"))
            CalculateRarity();

        int display = (int)m_ShowWeights;
        if (ImGui.Combo("Show Weights", ref display, WEIGHT_DISPLAY, WEIGHT_DISPLAY.Length))
            m_ShowWeights = (WeightDisplay)display;

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

    private void GetResourceWeights()
    {
        if (!m_Behaviour.UseResourceWeights)
            m_ResourceWeights = new ResourceWeights();
        
        else if (m_Behaviour.AdvancedResourceWeights)
        {
            if (m_TurnState == TurnState.Start)
                m_ResourceWeights = PRIMARY_RESOURCES;
                
            else
                m_ResourceWeights = SECONDARY_RESOURCES;
        }

        else
            m_ResourceWeights = TOTAL_WEIGHT;
    }

    /// <summary>
    /// Get a weighted value showing value of each node
    /// </summary>
    private void GetNodeWeights()
    {
        for (int i = 0; i < 54; i++)
            m_NodeWeights[i] = GetWeight(m_ResourceRarity, m_Behaviour, m_ResourceWeights, m_GameBoard.Nodes[i]);
        
        m_NeighbourWeights = new float[54];
        if (!m_Behaviour.CheckNeighbours)
            return;
        
        // In early game, if using staged resource weights, neighbour should be using secondary weights,
        // requiring re-calculation
        bool reCalc = (m_TurnState == TurnState.PreGame1 || m_TurnState == TurnState.Pregame2) &&
                        m_Behaviour.AdvancedResourceWeights;

        for (int i = 0; i < 54; i++)
        {
            Node rootNode = m_GameBoard.Nodes[i];
            for (int j = 0; j < 3; j++)
                for (int k = 0; k < 3; k++)
                {
                    Node leafNode = rootNode.GetSecondOrderNode(j, k);
                    if (leafNode == null)
                        continue;

                    float weight;

                    if (reCalc)
                        weight = GetWeight(m_ResourceRarity, m_Behaviour, SECONDARY_RESOURCES, leafNode);

                    else
                        weight = m_NodeWeights[leafNode.ID];

                    if (!m_Behaviour.UseBestNeighbour)
                        m_NeighbourWeights[i] += weight;

                    else if (weight > m_NeighbourWeights[i])
                        m_NeighbourWeights[i] = weight;
                }
            
            m_NeighbourWeights[i] *= .5f * m_Behaviour.Neighbours;
        }
    }

    private static float GetWeight(Resources resourceRarity, Behaviour behaviour, ResourceWeights resourceWeights, Node node)
    {
        if (!node.IsAvailable())
            return 0f;

        float weight = 0f;

        foreach(Tile tile in node.Tiles)
        {
            if (tile == null)
                continue;

            weight += GetTileWeight(resourceRarity, behaviour, resourceWeights, tile);
        }

        return weight;
    }

    /// <summary>
    /// Find weight of single tile
    /// </summary>
    /// <param name="resourceRarity"></param>
    /// <param name="mode"></param>
    /// <param name="resourceWeights"></param>
    /// <param name="tile"></param>
    /// <returns></returns>
    private static float GetTileWeight(Resources resourceRarity, Behaviour behaviour, ResourceWeights resourceWeights, Tile tile)
    {
        float weight = 0f;

        // Desert doesn't produce anything
        if (tile.Type == Resources.Type.Empty)
            return 0f;
        
        // Abundance weights on No. of tiles
        switch (tile.Type)
        {
        case (Resources.Type.Ore):
        case (Resources.Type.Brick):
            weight += 1/3f;
            break;
                    
        case (Resources.Type.Empty):
            break;
                
        default:
            weight += .25f;
            break;
        }
        weight *= behaviour.Abundance;

        float probability = tile.GetProbability();

        // max cards uses overall probability
        weight += (probability / 36f) * behaviour.MaxCards;

        // Rarity uses probability relative to same typed resources
        weight += (probability / resourceRarity.GetType(tile.Type)) * behaviour.Rarity;

        if (behaviour.UseResourceWeights)
            weight *= resourceWeights.GetResourceWeight(tile.Type) * behaviour.ResourceWeighting;

        return weight;
    }

    private void FindDistances()
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
            if (m_NodeWeights[i] + m_NeighbourWeights[i] > m_NodeWeights[index] + m_NeighbourWeights[index])
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
    private float[] m_NeighbourWeights = new float[54];
    private float[] m_DistanceMap = new float[54];
    private float[] m_RobberWeights = new float[19];

    private int m_SearchDepth = 1;

    private struct Behaviour
    {
        public Behaviour()
        {}

        public float MaxCards = 1f;
        public float Abundance = 1f;
        public float Rarity = 1f;
        
        public bool CheckNeighbours = false;
        public float Neighbours = 1f;
        public bool UseBestNeighbour = false;

        public bool UseResourceWeights = false;
        public float ResourceWeighting = 1f;
        public bool AdvancedResourceWeights = false;

        public bool TrackResources = false;

        public float RobberAvoidance = 1f;
        public float RobberAggression = 1f;
        public float RobberCityMult = 2f;
        public float RobberThievery = 1f;
        public bool EvaluateGain = false;

        public void DrawDebugUI()
        {
            CreateSlider(ref MaxCards, "MaxCards");
            CreateSlider(ref Abundance, "Abundance");
            CreateSlider(ref Rarity, "Rarity");

            ImGui.Separator();

            ImGui.Checkbox("Check Neighbours", ref CheckNeighbours);
            CreateSlider(ref Neighbours, "Neighbour weighting");
            ImGui.Checkbox("Use best neighbour", ref UseBestNeighbour);

            ImGui.Separator();

            ImGui.Checkbox("Resource Weights", ref UseResourceWeights);
            CreateSlider(ref ResourceWeighting, "Resource Weighting");
            ImGui.Checkbox("Use advanced resource weights", ref AdvancedResourceWeights);

            ImGui.Separator();
            ImGui.Checkbox("Track Resources", ref TrackResources);
            ImGui.Separator();

            CreateSlider(ref RobberAvoidance, "Avoidance");
            CreateSlider(ref RobberAggression, "Aggression");
            CreateSlider(ref RobberCityMult, "Cities", 1f, 3f);
            CreateSlider(ref RobberThievery, "Thievery");
            ImGui.Checkbox("Evaluate gain", ref EvaluateGain);
        }

        private static void CreateSlider(ref float num, string name, float min = 0f, float max = 2f)
        {
            const float PRECISION = .0005f;
            const string FORMAT = "%.4f";
            const ImGuiSliderFlags FLAGS = ImGuiSliderFlags.AlwaysClamp;

            ImGui.DragFloat(name, ref num, PRECISION, min, max, FORMAT, FLAGS);
        }
    }
    private Behaviour m_Behaviour;

    private enum WeightDisplay
    {
        Disabled,
        Tile,
        Neighbour,
        Mixed,
        Distance,
        Robber
    }
    private WeightDisplay m_ShowWeights;
    private static readonly string[] WEIGHT_DISPLAY = Enum.GetNames(typeof(WeightDisplay));

    private bool m_Step = false;
    private bool m_Continue = true;

    private Resources m_ResourceRarity;
    private ResourceWeights m_ResourceWeights;

    private Dictionary<Player, Resources> m_Predictions;

    /// <summary>
    /// Weights when placing starting settlements
    /// </summary>
    private static readonly ResourceWeights PRIMARY_RESOURCES = new ResourceWeights(1.2950f, 1.3233f, 1.3675f, 1.3100f, 1.3233f);

    /// <summary>
    /// Weights for all other settlements
    /// </summary>
    private static readonly ResourceWeights SECONDARY_RESOURCES = new ResourceWeights(1.3100f, 1.2467f, 1.3525f, 1.2275f, 1.3433f);

    /// <summary>
    /// Weighted by overall value instead of in a single instance
    /// </summary>
    private static readonly ResourceWeights TOTAL_WEIGHT = new ResourceWeights(1.63f, 0.6933f, 1.79f, 1.09f, 1.5466f);
}