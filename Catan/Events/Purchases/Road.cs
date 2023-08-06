namespace Catan.Event;

class Road : Purchase
{
    public Road(int playerID, int edgeID):
        base(playerID)
    {
        EdgeID = edgeID;
    }

    public override string FormatMessage()
    {
        return base.FormatMessage() + string.Format("Road at Edge {0}", EdgeID);
    }

    public int EdgeID { get; private set; }
}