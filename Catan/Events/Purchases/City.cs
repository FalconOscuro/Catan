namespace Catan.Event;

class City : Purchase
{
    public City(int playerID, int nodeID):
        base(playerID)
    {
        NodeID = nodeID;
    }

    public override string FormatMessage()
    {
        return base.FormatMessage() + string.Format("city at Node {0}", NodeID);
    }

    public int NodeID { get; private set; }
}