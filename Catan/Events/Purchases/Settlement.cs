namespace Catan.Event;

class Settlement : Purchase
{
    public Settlement(int playerID, int nodeID):
        base(playerID)
    {
        NodeID = nodeID;
    }

    public override string FormatMessage()
    {
        return base.FormatMessage() + string.Format("Settlement at Node {0}", NodeID);
    }

    public int NodeID { get; private set; }
}