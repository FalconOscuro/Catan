namespace Catan.Event;

class DevCard : Purchase
{
    public DevCard(int playerID, string name):
        base(playerID)
    {
        DevCardName = name;
    }

    public override string FormatMessage()
    {
        return base.FormatMessage() + string.Format("{0} development card", DevCardName);
    }

    public string DevCardName { get; private set; }
}