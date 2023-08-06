namespace Catan.Event;

class EndTurn : Player
{
    public EndTurn(int playerID):
        base(playerID)
    {}

    public override string FormatMessage()
    {
        return base.FormatMessage() + " ended their turn";
    }
}