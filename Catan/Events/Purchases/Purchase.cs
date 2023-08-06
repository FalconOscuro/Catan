namespace Catan.Event;

abstract class Purchase : Player
{
    protected Purchase(int playerID):
        base(playerID)
    {}

    public override string FormatMessage()
    {
        return base.FormatMessage() + " purchased a ";
    }
}