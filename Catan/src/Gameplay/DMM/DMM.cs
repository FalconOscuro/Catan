using System.Collections.Generic;

namespace Catan.Behaviour;

/// <summary>
/// A decision making module, responsible for controlling a player
/// </summary>
public abstract class DMM
{
    public abstract void PromptDecision();

    public enum DecisionType {
        TurnStart,
        TurnMain
    }

    protected List<IAction> GetAllActions() {
        return null;
    }
}