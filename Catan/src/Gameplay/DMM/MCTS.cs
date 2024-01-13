using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catan.Action;
using ImGuiNET;

namespace Catan.Behaviour;

public class MCTS : DMM
{
    public override int GetNextAction(GameState gameState, List<IAction> actions)
    {
        List<IAction> playedActions = gameState.PlayedActions;
        for (int i = m_ActionContinueIndex; i < playedActions.Count && m_RootNode != null; i++)
            if (!m_RootNode.Children.TryGetValue(playedActions[i], out m_RootNode))
                Console.WriteLine($"Fail Depth: {i - m_ActionContinueIndex} / {playedActions.Count - m_ActionContinueIndex}\n{playedActions[i]}");

        m_ActionContinueIndex = playedActions.Count;
        if (m_RootNode == null)
            Console.WriteLine("Entering unexplored space!");

        m_RootNode ??= new Node(gameState, actions);
        m_RootNode.Parent = null;

        m_End = DateTime.Now.AddSeconds(MaxThinkTime);
        m_Thinking = true;

        do
        {
            Node selected = m_RootNode.Select();

            if (!selected.Terminal)
            {
                Node child = selected.Expand();
                float reward = Simulate(child);
                child.BackPropogate(reward);
            }
        }
        while (m_End.CompareTo(DateTime.Now) > 0);

        m_Thinking = false;
        Node best = m_RootNode.Children.Values.First();

        foreach (Node child in m_RootNode.Children.Values.Skip(1))
            if (child.Visits > best.Visits)
                best = child;

        return actions.IndexOf(best.Action);
    }

    public override void ImDraw()
    {
        ImGui.Text($"Max think time: {MaxThinkTime}s");

        Node root = m_RootNode;
        if (root != null)
        {
            ImGui.Text($"Simulation count: {root.Visits}");
        }

        if (m_Thinking)
        {
            ImGui.Text($"Time Remaining: {m_End.Subtract(DateTime.Now).TotalSeconds}s");
        }
    }

    private float Simulate(Node node)
    {
        GameState gameState = node.GameState.Clone();
        //float cumulativeReward = 0f;
        int depth = 1;

        while (gameState.GetWinner() == -1 && depth < m_MaxSimulationDepth)
        {
            ChooseAction(gameState).Execute(gameState);
            depth++;
        }

        // TODO: Use better reward function
        return (gameState.GetWinner() == OwnerID ? 100f : 0f) + (float)gameState.Players[OwnerID].GetTotalVP() / depth;
    }

    private static IAction ChooseAction(GameState gameState)
    {
        Random rand = new();
        List<IAction> actions = gameState.GetValidActions();
        return actions[rand.Next(actions.Count)];
    }

    private Node m_RootNode;

    private DateTime m_End;
    private bool m_Thinking = false;

    public double MaxThinkTime = 30d;
    private const int m_MaxSimulationDepth = 400;

    private int m_ActionContinueIndex;
}

class Node
{
    public int Visits = 0;
    public float Probability = 1f;
    public float TotalReward;
    public float Reward { get { return Probability * TotalReward / Visits; }}

    public IAction Action;
    public Node Parent;
    public GameState GameState;
    public List<IAction> ValidActions = new();

    public Dictionary<IAction, Node> Children = new();

    public bool Expanded { get { return Children.Values.Count == ValidActions.Count; }}
    public bool Terminal { get { return GameState.GetWinner() != -1; }}

    public Node(GameState gameState, List<IAction> validActions)
    {
        GameState = gameState.Clone();
        SetValidActions(validActions);
        Parent = null;
        Action = null;
    }

    public Node(GameState gameState, IAction action, Node parent)
    {
        Action = action.Clone();
        GameState = action.Execute(gameState.Clone());
        SetValidActions(GameState.GetValidActions());

        if (action is RollDiceAction rollDice)
        {
            Probability = (6 - Math.Abs(rollDice.RolledSum - 7)) / 36f;
        }

        // TIDY ME
        else if (action is RobberAction robberAction)
        {
            if (robberAction.TargetID != -1)
            {
                Player player = gameState.Players[robberAction.TargetID];
                int handSize = player.Hand.Count();

                if (handSize > 0)
                {
                    Probability = player.Hand[robberAction.Stolen] / handSize;
                }
            }
        }

        Parent = parent;
    }

    protected void SetValidActions(List<IAction> actions)
    {
        // TIDY ME
        foreach (IAction action in actions)
        {
            if (action is RollDiceAction rollDice)
            {
                for (int i = 2; i < 13; i++)
                    ValidActions.Add(new RollDiceAction(){
                        OwnerID = rollDice.OwnerID,
                        Rolled = (i, 0)
                    });
            }

            else if (action is RobberAction robberAction)
            {
                if (robberAction.TargetID == -1)
                {
                    ValidActions.Add(action.Clone());
                    continue;
                }
                
                Player player = GameState.Players[robberAction.TargetID];
                int handSize = player.Hand.Count();

                if (handSize == 0)
                {
                    ValidActions.Add(action.Clone());
                    continue;
                }

                for (Resources.Type type = Resources.Type.Brick; type < Resources.Type.Wool + 1; type++)
                    if (player.Hand[type] > 0)
                    {
                        RobberAction newAction = (RobberAction)robberAction.Clone();
                        newAction.Stolen = type;

                        ValidActions.Add(newAction);
                    }
            }

            else
                ValidActions.Add(action.Clone());
        }
    }

    public virtual Node Select()
    {
        if (!Expanded || Terminal)
            return this;
        
        return GetBestChild().Select();
    }

    public virtual Node Expand()
    {
        if (Terminal)
            return this;
        
        List<IAction> unexplored = new();
        foreach (IAction action in ValidActions)
            if (!Children.ContainsKey(action))
                unexplored.Add(action);
        
        if (unexplored.Count == 0)
            return this;
        
        Random rand = new();
        IAction chosenAction = unexplored[rand.Next(unexplored.Count)];

        Node node = new (GameState, chosenAction, this);
        Children[chosenAction] = node;
        return node;
    }

    public void BackPropogate(float reward)
    {
        Visits++;
        TotalReward += reward;
        Parent?.BackPropogate(reward * Probability);
    }

    public Node GetBestChild()
    {
        Node bestChild = Children.Values.First();
        float bestUCB = UCB(bestChild);

        foreach (Node child in Children.Values.Skip(1))
        {
            float uCB = UCB(child);

            if (uCB > bestUCB)
            {
                bestChild = child;
                bestUCB = uCB;
            }
        }

        return bestChild;
    }

    public float UCB(Node child)
    {
        return child.Reward + (0.8f * MathF.Sqrt(MathF.Log((float)Visits / child.Visits)));
    }
}