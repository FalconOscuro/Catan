namespace Catan;

abstract class Action
{
    public abstract bool CheckPreConditions(Player.PlayerStatus playerStatus);
}