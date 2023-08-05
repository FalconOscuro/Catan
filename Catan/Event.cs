namespace Catan;


abstract class Event
{
    public abstract string FormatMessage();
}

abstract class PlayerEvent : Event
{
    protected PlayerEvent(Player owner)
    {
        Owner = owner;
    }

    public override string FormatMessage()
    {
        return "Player";
    }

    public Player Owner { get; private set; }
}

class DiceRollEvent : PlayerEvent
{
    public DiceRollEvent(int roll1, int roll2, Player owner):
        base(owner)
    {
        Roll1 = roll1;
        Roll2 = roll2;
    }

    public override string FormatMessage()
    {
        return string.Format("{0} rolled a {1} ({2} + {3})", 
            base.FormatMessage(), Roll1 + Roll2, Roll1, Roll2);
    }

    public int Roll1 { get; private set; }
    public int Roll2 { get; private set; }
}

abstract class TargetedEvent : PlayerEvent
{
    protected TargetedEvent(Player owner, Player target):
        base(owner)
    {
        Target = target;
    }

    public Player Target { get; private set; }
}

class TradeEvent : TargetedEvent
{
    public TradeEvent(Resources give, Resources recieve, Player owner, Player target):
        base(owner, target)
    {
        Give = give;
        Recieve = recieve;
    }

    public override string FormatMessage()
    {
        string message = string.Format("{0} gave {1} to {2}", 1, Give, 2);

        if (Recieve.GetTotal() != 0)
            message += string.Format(" in exchange for {0}", Recieve);
        
        return message;
    }

    public Resources Give { get; private set; }
    public Resources Recieve { get; private set; }
}