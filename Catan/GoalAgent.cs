using System.Collections.Generic;

namespace Catan;

class GoalAgent : Player
{
    public GoalAgent(Catan game, int playerID):
        base(game, playerID)
    {
        m_ActionQueue = new Queue<Action>();
    }

    public override void StartGame()
    {
        Resources resourceRarity = new Resources();

        foreach(Tile tile in GameBoard.BoardState.Tiles)
            resourceRarity.AddType(tile.Type, tile.GetProbability());

        Node[] nodes = GameBoard.BoardState.Nodes;

        for (int i = 0; i < 54; i++)
            m_NodeWeights[i] = NodeInfo.GetNodeInfo(nodes[i], resourceRarity);
    }

    public override void Update()
    {
        Catan.GameState gameState = GameBoard.GetGameState();
        UpdateWeights(gameState);

        if (m_ActionQueue.Count == 0)
            IteratePopulateActionQueue(gameState);
    }

    private void UpdateWeights(Catan.GameState gameState)
    {
        Node[] nodes = gameState.BoardState.Nodes;
        for (int i = 0; i < 54; i++)
        {
            Node node = nodes[i];
            m_NodeWeights[i].Occupied = node.OwnerID != -1;
        }
    }

    private void IteratePopulateActionQueue(Catan.GameState gameState)
    {
        // End goal reached
        if (gameState.Phase == Catan.State.End)
            return;

        UpdateWeights(gameState);

        // Pregame logic
        if (gameState.IsPregame())
        {
            int nodeID = GetBestNode();
            m_ActionQueue.Enqueue(new BuildSettlement(PlayerID, nodeID));
        }
    }

    private int GetBestNode()
    {
        int bestID = -1;
        float bestWeight = 0f;

        for (int i = 0; i < 54; i++)
        {
            float weight = m_NodeWeights[i].GetTotalWeight();

            if (weight > bestWeight)
            {
                bestWeight = weight;
                bestID = i;
            }
        }

        return bestID;
    }

    private struct NodeInfo
    {
        public NodeInfo()
        {}

        public ResourceWeights Probability = ResourceWeights.Zero;
        public ResourceWeights Abundance = ResourceWeights.Zero;
        public ResourceWeights Rarity = ResourceWeights.Zero;
        
        public bool Occupied = false;

        public readonly float GetTotalWeight()
        {
            if (Occupied)
                return 0f;
            
            return Probability.Sum() + Abundance.Sum() + Rarity.Sum();
        }

        public static NodeInfo GetNodeInfo(Node targetNode, Resources resourceRarity)
        {
            NodeInfo nodeInfo = new();
            
            for (int i = 0; i < 3; i++)
            {
                Tile tile = targetNode.GetTile(i);
                if (tile == null)
                    continue;

                Resources.Type type = tile.Type;
                int probability = tile.GetProbability();
                
                nodeInfo.Probability.AddResourceType(type, probability / 36f);
                nodeInfo.Abundance.AddResourceType(type, s_AbundanceWeight.GetResourceWeight(type));
                nodeInfo.Rarity.AddResourceType(type, probability / resourceRarity.GetType(type));
            }

            return nodeInfo;
        }

        private static readonly ResourceWeights s_AbundanceWeight = new(.25f, 1/3f, .25f, .25f, 1/3f);
    }
    private readonly NodeInfo[] m_NodeWeights = new NodeInfo[54];

    private Queue<Action> m_ActionQueue;
}