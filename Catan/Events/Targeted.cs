namespace Catan.Event;

/// <summary>
/// Abstract event involving 2 players
/// </summary>
abstract class Targeted : Player
{
    protected Targeted(int playerID, int targetPlayerID):
        base(playerID)
    {
        TargetPlayerID = targetPlayerID;
    }

    public int TargetPlayerID { get; private set; }
}