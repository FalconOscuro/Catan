using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ImGuiNET;

namespace Catan;

// TODO:
// Bank Trading
// Player Trading
// Port weighting
// Same tile/No weighting
// Cities
// Development cards
// Longest road prioritization


/// <summary>
/// Autonomus player
/// </summary>
class PlayerAgent : Player
{
    public PlayerAgent(Catan board, int ID):
        base(board, ID)
    {
        m_ResourceRarity = new Resources();
        m_Predictions = new Dictionary<Player, Resources>();
        m_Behaviour = new Behaviour();
    }

    public override void StartGame()
    {
        m_Predictions = new Dictionary<Player, Resources>();
        base.StartGame();

        foreach (Player player in GameBoard.Players)
        {
            if (player == this)
                continue;

            m_Predictions.Add(player, new Resources());
        }

        CalculateRarity();
    }

    public override void OnTradeComplete(Trade trade)
    {
        /*
        base.OnTradeComplete(trade);

        if (!m_Behaviour.TrackResources)
            return;

        if (trade.From != m_Status.HeldResources && trade.From != null)
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
        */
    }

    public override void Update()
    {
        GetResourceWeights();
        GetNodeWeights();
        GetDistances();
        
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
        
        case TurnState.Discard:
            Discard();
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
        Node targetNode = GetHighestValueNode();

        if (targetNode != null && m_Status.UnbuiltSettlements > 0)
        {
            if (TryBuildSettlement(targetNode))
                return;

            List<Edge> expandPath = AStar(targetNode);
            if (expandPath != null)
                if (expandPath.Count > 0)
                    if (TryBuildRoad(expandPath[^1]))
                        return;
        }

        Node cityTarget = GetHighestValueCity();
        if (cityTarget != null)
        {
            if (TryBuildCity(cityTarget))
                return;
        }

        /*
        Need to weight options

        In theory all potential goals end with gaining VP
        Potential Goals:
        - Longest road: Building Roads, Connecting routes, Interrupting networks (Also involves further settlement wieghting)
        - Settlements/Cities (Account for weighting changes)
        - Largest Army
        - Buying VP
        */

        EndTurn();
    }

    private void Robber()
    {
        Node targetNode = null;
        Tile targetTile = null;
        float maxWeight = 0f;

        for (int i = 0; i < 19; i++)
        {
            m_RobberWeights[i] = 0f;

            if (i == GameBoard.Board.RobberPos)
                continue;

            Tile tile = GameBoard.Board.Tiles[i];

            float weight = GetTileWeight(m_ResourceRarity, m_Behaviour, TOTAL_WEIGHT, tile);
            float maxNodeWeight = 0f;
            Node maxNode = null;

            for (int j = 0; j < 6; j++)
            {
                Node node = tile.GetNode(j);

                float deltaWeight = weight;

                if (node.OwnerID == -1)
                    continue;
                
                else if (node.OwnerID == m_Status.PlayerID)
                    deltaWeight *= -m_Behaviour.RobberAvoidance;
                
                else
                {
                    deltaWeight *= m_Behaviour.RobberAggression;

                    /*
                    if (m_Behaviour.EvaluateGain && m_Behaviour.TrackResources)
                    {
                        deltaWeight += 
                            m_ResourceWeights.GetResourcesWeight(m_Predictions[node.Owner]) / node.Owner.GetHandSize() * m_Behaviour.RobberThievery;
                    }
                    */

                   deltaWeight += GameBoard.Players[node.OwnerID].GetHandSize() / 7 * m_Behaviour.RobberThievery;
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
                targetTile = GameBoard.Board.Tiles[i];
                targetNode = maxNode;
            }
        }

        TryMoveRobber(targetTile, targetNode);

        m_TurnState = TurnState.Main;
    }

    private void Discard()
    {
        Resources removing = new();

        // Simple method, get rid of most numerous cards
        for (int i = 0; i < GetHandSize() / 2; i++)
        {
            Resources remaining = m_Status.HeldResources - removing;

            int max = 0;
            Resources.Type type = Resources.Type.Empty;
            for (Resources.Type j = 0; (int)j < 5; j++)
            {
                int typeNum = remaining.GetType(j);

                if (typeNum > max)
                {
                    max = typeNum;
                    type = j;
                }
            }

            removing.AddType(type, 1);
        }

        Trade trade = new(GameBoard)
        {
            FromID = m_Status.PlayerID,
            ToID = -1,
            Giving = removing
        };
        trade.TryExecute();

        EndTurn();
    }

    public override void SpriteDraw(SpriteBatch spriteBatch, SpriteFont font, float windowHeight)
    {
        if (m_ShowWeights == WeightDisplay.Robber)
        {
            for (int i = 0; i < 19; i++)
                spriteBatch.DrawString(
                    font, String.Format("{0:0.00}", m_RobberWeights[i] * 10),
                    GameBoard.Board.Tiles[i].Position.FlipY(windowHeight),
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
                    GameBoard.Board.Nodes[i].Position.FlipY(windowHeight),
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
            GetDistances();
        
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
            m_NodeWeights[i] = GetNodeWeight(m_ResourceRarity, m_Behaviour, m_ResourceWeights, GameBoard.Board.Nodes[i]);

        if (!m_Behaviour.CheckNeighbours)
            return;
        
        // In early game, if using staged resource weights, neighbour should be using secondary weights,
        // requiring re-calculation
        bool reCalc = (m_TurnState == TurnState.PreGame1 || m_TurnState == TurnState.Pregame2) &&
                        m_Behaviour.AdvancedResourceWeights;

        for (int i = 0; i < 54; i++)
        {
            Node rootNode = GameBoard.Board.Nodes[i];
            m_NeighbourWeights[i] = 0f;

            for (int j = 0; j < 3; j++)
                for (int k = 0; k < 3; k++)
                {
                    Node leafNode = rootNode.GetSecondOrderNode(j, k);
                    if (leafNode == null)
                        continue;

                    float weight;

                    if (reCalc)
                        weight = GetNodeWeight(m_ResourceRarity, m_Behaviour, SECONDARY_RESOURCES, leafNode);

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

    private static float GetNodeWeight(Resources resourceRarity, Behaviour behaviour, ResourceWeights resourceWeights, Node node, bool ignoreOwned = false)
    {
        if (!node.IsAvailable() && !ignoreOwned)
            return 0f;

        float weight = 0f;

       for (int i = 0; i < 3; i++)
        {
            Tile tile = node.GetTile(i);

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

    private void GetDistances()
    {
        float step = 1f / (m_Behaviour.SearchDepth + 1);

        foreach (NodeContainer node in m_ControlledNodes)
            if (m_DistanceMap[node.RefNode.ID] != 1f)
                ApplyDistances(node.RefNode.ID, step);
    }

    private Edge FindBestEdge(Node node)
    {
        float maxWeight = 0f;
        Edge maxEdge = null;

        for (int i = 0; i < 3; i++)
        {
            float weight = 0f;
            Edge edge = node.GetEdge(i);
            if (edge == null)
                continue;

            else if (edge.OwnerID != -1)
                continue;

            Node neighbour = node.GetNeighbourNode(i);
            
            for (int j = 0; j < 3; j++)
            {
                Edge edge1 = neighbour.GetEdge(j);

                if (edge1 == null)
                    continue;
                
                Node search = neighbour.GetNeighbourNode(j);
                if (edge1.OwnerID != -1 || search.OwnerID != -1 || search == node)
                    continue;
                
                weight += m_NodeWeights[search.ID]; 
            }
            weight /= 2;

            if (weight > maxWeight || maxEdge == null)
            {
                maxWeight = weight;
                maxEdge = edge;
            }
        }

        return maxEdge;
    }

    private List<Edge> AStar(Node node, List<Edge> path = null)
    {
        if (node.OwnerID == m_Status.PlayerID)
            return path;

        else if (node.OwnerID != -1)
            return null;

        else path ??= new List<Edge>();

        List<Tuple<Node, int>> neighbours = new();

        for (int i = 0; i < 3; i++)
        {
            Node neighbour = node.GetNeighbourNode(i);
            Edge edge = node.GetEdge(i);

            if (neighbour == null || path.Contains(edge))
                continue;

            else if (edge.OwnerID == m_Status.PlayerID)
                return path;
            
            else if (edge.OwnerID != -1 || m_DistanceMap[neighbour.ID] == 0f)
                continue;
            
            int index = 0;
            for (; index < neighbours.Count; index++)
                if (m_DistanceMap[neighbours[index].Item1.ID] < m_DistanceMap[neighbour.ID])
                    break;
            
            neighbours.Insert(index, new Tuple<Node, int>(neighbour, i));
        }

        List<Edge> foundPath = null;

        foreach (Tuple<Node, int> neighbour in neighbours)
        {
            List<Edge> current = new(path)
            {
                node.GetEdge(neighbour.Item2)
            };

            current = AStar(neighbour.Item1, current);

            if (current == null)
                continue;

            else if (current.Count <= MathF.Ceiling((1f - m_DistanceMap[neighbour.Item1.ID]) * (m_Behaviour.SearchDepth + 1)))
                return current;
            
            else if (foundPath == null)
                foundPath = current;
            
            else if (current.Count < foundPath.Count)
                foundPath = current;
        }

        return foundPath;
    }

    private void ApplyDistances(int index, float step, float value = 1f)
    {
        if (m_DistanceMap[index] >= value)
            return;
        
        m_DistanceMap[index] = value;
        value -= step;

        if (value < step - (step / 2))
            return;

        Node node = GameBoard.Board.Nodes[index];
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

        foreach(Tile tile in GameBoard.Board.Tiles)
            m_ResourceRarity.AddType(tile.Type, tile.GetProbability());
    }

    private Node GetHighestValueNode()
    {
        int index = 0;
        float max = m_NodeWeights[0] + m_NeighbourWeights[0];
        bool setup = IsSetup();

        if (!setup)
            max *= m_DistanceMap[0];

        for (int i = 1; i < 54; i++)
        {
            float weight = m_NeighbourWeights[i] + m_NodeWeights[i];
            if (!setup)
                weight *= m_DistanceMap[i];

            if (weight > max)
            {
                index = i;
                max = weight;
            }
        }
        
        if (m_NodeWeights[index] == 0f)
            return null;

        return GameBoard.Board.Nodes[index];
    }

    private Node GetHighestValueCity()
    {
        float maxWeight = 0f;
        Node target = null;

        foreach (NodeContainer node in m_ControlledNodes)
        {
            if (node.RefNode.OwnerID != m_Status.PlayerID || node.RefNode.IsCity)
                continue;

            float weight = GetNodeWeight(m_ResourceRarity, m_Behaviour, CITY_RESOURCES, node.RefNode, true);

            if (weight > maxWeight)
            {
                maxWeight = weight;
                target = node.RefNode;
            }
        }

        return target;
    }

    private bool IsSetup()
    {
        return m_TurnState == TurnState.PreGame1 || m_TurnState == TurnState.Pregame2;
    }

    private readonly float[] m_NodeWeights = new float[54];
    private readonly float[] m_NeighbourWeights = new float[54];
    private readonly float[] m_DistanceMap = new float[54];
    private readonly float[] m_RobberWeights = new float[19];

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

        public int SearchDepth = 2;

        public void DrawDebugUI()
        {
            if (ImGui.Button("Shuffle"))
                Shuffle();
            ImGui.Separator();

            CreateSlider("MaxCards", ref MaxCards);
            CreateSlider("Abundance", ref Abundance);
            CreateSlider("Rarity", ref Rarity);

            ImGui.Separator();

            ImGui.Checkbox("Check Neighbours", ref CheckNeighbours);
            CreateSlider("Neighbour weighting", ref Neighbours);
            ImGui.Checkbox("Use best neighbour", ref UseBestNeighbour);

            ImGui.Separator();

            ImGui.Checkbox("Resource Weights", ref UseResourceWeights);
            CreateSlider("Resource Weighting", ref ResourceWeighting);
            ImGui.Checkbox("Use advanced resource weights", ref AdvancedResourceWeights);

            ImGui.Separator();
            ImGui.Checkbox("Track Resources", ref TrackResources);
            ImGui.Separator();

            CreateSlider("Avoidance", ref RobberAvoidance);
            CreateSlider("Aggression", ref RobberAggression);
            CreateSlider("Cities", ref RobberCityMult, 1f, 3f);
            CreateSlider("Thievery", ref RobberThievery);
            ImGui.Checkbox("Evaluate gain", ref EvaluateGain);

            ImGui.Separator();
            ImGui.DragInt("Search Depth", ref SearchDepth, .5f, 2, 11, "%i", ImGuiSliderFlags.AlwaysClamp);
        }

        private void Shuffle()
        {
            Random rand = new();

            MaxCards = rand.NextFloat(2f);
            Abundance = rand.NextFloat(2f);
            Rarity = rand.NextFloat(2f);

            CheckNeighbours = rand.NextBool();
            Neighbours = rand.NextFloat(2f);
            UseBestNeighbour = rand.NextBool() && CheckNeighbours;

            UseResourceWeights = rand.NextBool();
            ResourceWeighting = rand.NextFloat(2f);
            AdvancedResourceWeights = rand.NextBool() && UseResourceWeights;

            TrackResources = rand.NextBool();

            RobberAvoidance = rand.NextFloat(2f);
            RobberAggression = rand.NextFloat(2f);
            RobberCityMult = rand.NextFloat(3f, 1f);
            RobberThievery = rand.NextFloat(2f);
            EvaluateGain = rand.NextBool();

            SearchDepth = rand.Next(2, 12);
        }

        private static void CreateSlider(string name, ref float num, float min = 0f, float max = 2f)
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
    private static readonly ResourceWeights PRIMARY_RESOURCES = new(1.2950f, 1.3233f, 1.3675f, 1.3100f, 1.3233f);

    /// <summary>
    /// Weights for all other settlements
    /// </summary>
    private static readonly ResourceWeights SECONDARY_RESOURCES = new(1.3100f, 1.2467f, 1.3525f, 1.2275f, 1.3433f);

    private static readonly ResourceWeights CITY_RESOURCES = new(1.2650f, 1.3233f, 1.4700f, 1.3525f, 1.4800f); 

    /// <summary>
    /// Weighted by overall value instead of in a single instance
    /// </summary>
    private static readonly ResourceWeights TOTAL_WEIGHT = new(1.63f, 0.6933f, 1.79f, 1.09f, 1.5466f);
}