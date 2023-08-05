namespace Catan;

class BuildSettlement : Action
{
    // Need to target a specific node

    public override bool CheckPreConditions(Player.PlayerStatus playerStatus)
    {
        return false;
    }
}