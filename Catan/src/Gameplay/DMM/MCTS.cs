using System;
using System.Collections.Generic;
using System.Linq;
using Catan.Action;
using Grid.Hexagonal;
using ImGuiNET;

namespace Catan.Behaviour;


/// <summary>
/// Controller implementation utilizing a Monte Carlo Tree search
/// </summary>
public class MCTS : Controller
{
    public override int ChooseAction(GameState gameState, List<IAction> actions)
    {
        // Attempt to traverse tree to see if current gamestate has already been explored
        List<IAction> playedActions = gameState.PlayedActions;
        for (int i = m_ActionContinueIndex; i < playedActions.Count && m_RootNode != null; i++)
            m_RootNode.Children.TryGetValue(playedActions[i], out m_RootNode);

        // Save index for last known position in action list
        m_ActionContinueIndex = playedActions.Count;

        // Ensure root node is set and remove references to any prior nodes
        m_RootNode ??= new TreeNode(gameState, actions, ChooseAction);
        m_RootNode.Parent = null;

        // Avoid performing lengthy simulation if only one action is possible,
        // This may impact the playing performance but I believe this is minimal,
        // As the only possible single actions are:
        // - Dice Rolling: With high span, so the majority of the tree is often discarded anyway
        // - Turn end: This agent has shown to be ineffective at exploring into its next turn without very large maximum iteration counts
        //          and is likely to have to build the tree from scratch regardless.
        if (actions.Count == 1)
            return 0;

        m_Iterations = 0;
        m_Thinking = true;

        do
        {
            TreeNode selected = m_RootNode.Select();

            if (!selected.Terminal)
            {
                TreeNode child = selected.Expand();
                float reward = Simulate(child);
                child.BackPropogate(reward);
            }
        }
        while (m_Iterations++ < MaxIterationCount);
        m_Thinking = false;

        // Assume most visited child to be best
        TreeNode best = m_RootNode.Children.Values.First();
        foreach (TreeNode child in m_RootNode.Children.Values.Skip(1))
            if (child.Visits > best.Visits)
                best = child;

        // Slow search, could be improved?
        return actions.IndexOf(best.Action);
    }

    public override void ImDraw()
    {
        ImGui.Text($"Max iterations: {MaxIterationCount}");

        TreeNode root = m_RootNode;
        if (root != null)
        {
            ImGui.Text($"Simulation count: {root.Visits}");
        }

        if (m_Thinking)
        {
            ImGui.Text($"Iterations: {m_Iterations}");
        }
    }

    /// <summary>
    /// Simulate a single rollout
    /// </summary>
    /// <param name="node">Expanded node</param>
    /// <returns>Result of simulation as reward value</returns>
    private float Simulate(TreeNode node)
    {
        GameState gameState = node.GameState.Clone();
        float reward = 0f;
        int depth = 1;
        Random random = new();

        while (gameState.GetWinner() == -1 && depth < MAX_SIMULATION_DEPTH)
        {
            List<IAction> actions = gameState.GetValidActions();
            actions[random.Next(actions.Count)].Execute(gameState);

            reward += (float)gameState.Players[OwnerID].GetTotalVP() / depth;
            depth++;
        }

        // TODO: Use better reward function
        return (gameState.GetWinner() == OwnerID ? 100f : 0f) + reward;
    }

    public virtual (IAction action, float heuristic) ChooseAction(List<IAction> actions, GameState gameState)
    {
        (IAction action, float heuristic) bestAction = GetActionHeuristic(actions.First(), gameState);

        foreach (IAction action in actions.Skip(1))
        {
            (IAction action, float heuristic) current = GetActionHeuristic(action, gameState);

            if (current.heuristic > bestAction.heuristic)
                bestAction = current;
        }

        return bestAction;
    }

    protected virtual (IAction action, float heuristic) GetActionHeuristic(IAction action, GameState gameState)
    {
        (IAction action, float heuristic) result = (action, 1f);

        if (action is KnightAction)
        {
            Axial robberPos = gameState.RobberPos;
            if (gameState.Board.TryGetHex(robberPos, out Tile hex))
            {
                if (hex.Resource != Resources.Type.Empty)
                    for (Vertex.Key nodePos = new(){Position = robberPos, Side = Vertex.Side.W}; nodePos.Side < Vertex.Side.SW + 1; nodePos.Side++)
                        if (gameState.Board.TryGetVertex(nodePos, out Node node))
                            if (node.OwnerID == action.OwnerID)
                            {
                                result.heuristic += 10;
                                break;
                            }
            }
        }

        else if (action is BuildCityAction || action is BuildSettlementAction)
            result.heuristic += 1000;
        
        else if (action is BuildRoadAction)
        {
            result.heuristic += gameState.GetCurrentPlayer().Roads / Rules.MAX_ROADS;
        }

        else if (action is RollDiceAction)
        {
            RollDiceAction rollDice = action as RollDiceAction;

            if (rollDice.RolledSum != 0)
                result.heuristic += (6f - MathF.Abs(rollDice.RolledSum - 7)) / 36f;
        }

        return result;
    }

    /// <summary>
    /// Root node of the tree
    /// </summary>
    /// <remarks>
    /// Represents the current <see cref="GameState"/>
    /// </remarks>
    private TreeNode m_RootNode;

    /// <summary>
    /// Iterations executed for last <see cref="ChooseAction"/> call.
    /// </summary>
    /// <remarks>
    /// Defined as a member variable instead of locally to allow display through <see cref="ImDraw"/>
    /// </remarks>
    private int m_Iterations = 0;

    /// <summary>
    /// True if <see cref="ChooseAction"/> already running.
    /// </summary>
    private bool m_Thinking = false;

    private const int MAX_SIMULATION_DEPTH = 400;

    /// <summary>
    /// Maximum MCTS iterations per call of <see cref="ChooseAction"/>
    /// </summary>
    public int MaxIterationCount = 1000;

    /// <summary>
    /// Last known index for actions executed.
    /// </summary>
    /// <remarks>
    /// Used on subsequent <see cref="ChooseAction"/> calls to check if new <see cref="GameState"/> is already explored.
    /// </remarks>
    private int m_ActionContinueIndex;
}

/// <summary>
/// A single node of a <see cref="GameState"/> tree used in the <see cref="MCTS"/> controller.
/// </summary>
class TreeNode
{
    /// <summary>
    /// Times this node has been simulated from.
    /// </summary>
    public int Visits = 0;

    /// <summary>
    /// Probability of node occuring from <see cref="Parent"/> node.
    /// </summary>
    /// <remarks>
    /// Used for random actions such as a <see cref="RollDiceAction"/> or <see cref="RobberAction"/>
    /// </remarks>
    public float Probability = 1f;

    /// <summary>
    /// Reward accumulated from all simulations resulting from this node
    /// </summary>
    public float TotalReward;

    /// <summary>
    /// The mean reward, accounting for <see cref="Vists"/> and <see cref="Probability"/>.
    /// </summary>
    public float Reward { get { return Probability * TotalReward / Visits; }}

    /// <summary>
    /// The action connecting <see cref="Parent"/> to this.
    /// </summary>
    public IAction Action;

    /// <summary>
    /// Parent node within tree.
    /// </summary>
    public TreeNode Parent;
    
    public GameState GameState;
    
    /// <summary>
    /// All valid actions capable of expansion
    /// </summary>
    public List<IAction> ValidActions = new();

    public Dictionary<IAction, TreeNode> Children = new();

    public bool Expanded { get { return Children.Values.Count == ValidActions.Count; }}
    public bool Terminal { get { return GameState.GetWinner() != -1; }}

    private readonly Func<List<IAction>, GameState, (IAction action, float heuristic)> m_ActionSelector;

    public TreeNode(GameState gameState, List<IAction> validActions, Func<List<IAction>, GameState, (IAction action, float heuristic)> actionSelector)
    {
        GameState = gameState.Clone();
        SetValidActions(validActions);
        Parent = null;
        Action = null;

        m_ActionSelector = actionSelector;
    }

    public TreeNode(GameState gameState, IAction action, TreeNode parent)
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
        m_ActionSelector = parent.m_ActionSelector;
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

    public virtual TreeNode Select()
    {
        if (!Expanded || Terminal)
            return this;
        
        return GetBestChild().Select();
    }

    public virtual TreeNode Expand()
    {
        if (Terminal)
            return this;
        
        List<IAction> unexplored = new();
        foreach (IAction action in ValidActions)
            if (!Children.ContainsKey(action))
                unexplored.Add(action);
        
        if (unexplored.Count == 0)
            return this;
        
        (IAction action, float heuristic) chosen = m_ActionSelector(unexplored, GameState);

        TreeNode node = new (GameState, chosen.action, this){
            TotalReward = chosen.heuristic
        };

        Children[chosen.action] = node;
        return node;
    }

    public void BackPropogate(float reward)
    {
        Visits++;
        TotalReward += reward;
        Parent?.BackPropogate(reward * Probability);
    }

    public TreeNode GetBestChild()
    {
        TreeNode bestChild = Children.Values.First();
        float bestUCB = UCB(bestChild);

        foreach (TreeNode child in Children.Values.Skip(1))
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

    public float UCB(TreeNode child)
    {
        return child.Reward + (0.8f * MathF.Sqrt(MathF.Log((float)Visits / child.Visits)));
    }
}