namespace Catan.Event;

/// <summary>
/// Abstract event involving one player
/// </summary>
abstract class Player : Event
{
    protected Player(int playerID)
    {
        PlayerID = playerID;
    }

    public override string FormatMessage()
    {
        return FormatPlayerName(PlayerID);
    }

    protected static string FormatPlayerName(int playerID)
    {
        if (playerID == -1)
            return "Bank";

        return string.Format("Player {0}", playerID);
    }

    public int PlayerID { get; private set; }
}