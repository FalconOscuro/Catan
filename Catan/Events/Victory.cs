namespace Catan.Event;

class Victory : Player
{
    public Victory(int playerID):
        base(playerID)
    {}

    public override string FormatMessage()
    {
        return base.FormatMessage() + " wins!";
    }
}