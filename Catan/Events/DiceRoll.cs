namespace Catan.Event;

class DiceRollEvent : Player
{
    public DiceRollEvent(int playerID, int roll1, int roll2):
        base(playerID)
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