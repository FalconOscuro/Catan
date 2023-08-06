namespace Catan.Event;

class Robber : Targeted
{
    public Robber(int playerID, int targetPlayerID, int tileID):
        base(playerID, targetPlayerID)
    {
        TileID = tileID;
    }

    public override string FormatMessage()
    {
        string message = base.FormatMessage() + string.Format(" moved the Robber to Tile {0}", TileID);

        if (TargetPlayerID != -1)
            message += string.Format(" stealing from {0}", FormatPlayerName(TargetPlayerID));

        return message;
    }

    public int TileID { get; private set; }
}