namespace Catan.Event;

/// <summary>
/// Trading event
/// </summary>
class Trade : Targeted
{
    public Trade(int playerID, int targetPlayerID, Resources given, Resources recieved, bool hidden):
        base(playerID, targetPlayerID)
    {
        Given = given;
        Recieved = recieved;

        Hidden = hidden;
    }

    public override string FormatMessage()
    {
        string message = Hidden ? "(Hidden) " : "";
        message += string.Format("{0} gave {1} to {2}", base.FormatMessage(), Given, FormatPlayerName(TargetPlayerID));

        if (Recieved.GetTotal() != 0)
            message += string.Format(" in exchange for {0}", Recieved);
        
        return message;
    }

    public Resources Given { get; private set; }
    public Resources Recieved { get; private set; }

    public bool Hidden;
}