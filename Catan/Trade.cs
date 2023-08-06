using ImGuiNET;

namespace Catan;

/// <summary>
/// A trade between two resource entities
/// </summary>
class Trade
{
    public Trade(Catan board, bool hidden = false)
    {
        Giving = new Resources();
        Receiving = new Resources();
        m_Board = board;

        Complete = false;

        Hidden = hidden;
    }

    /// <summary>
    /// Attempt to complete the trade
    /// </summary>
    /// <returns>Success state</returns>
    public bool TryExecute()
    {
        if (FromID == ToID || FromID < -1 || FromID > 3 || ToID < -1 || ToID > 3)
            return false;
        
        ref Resources from = ref GetRefResource(FromID, m_Board);
        ref Resources to = ref GetRefResource(ToID, m_Board);

        if (Giving > from || Receiving > to || Giving.GetTotal() + Receiving.GetTotal() == 0)
            return false;
        
        from.TryTake(Giving);
        to.TryTake(Receiving);

        from.Add(Receiving);
        to.Add(Giving);

        Complete = true;

        m_Board.OnCompleteTrade(this);

        Event.Trade tradeEvent = new(FromID, ToID, Giving, Receiving, Hidden);
        Event.Log.Singleton.PostEvent(tradeEvent);

        return true;
    }

    public void UIDraw(bool modify = true)
    {
        ImGui.Text("Giving");
        Giving.UIDraw(modify);

        ImGui.Separator();

        ImGui.Text("Recieving");
        Receiving.UIDraw(modify);
    }

    private static ref Resources GetRefResource(int iD, Catan game)
    {
        return ref (iD == -1 ? ref game.GetBank() : ref game.Players[iD].GetHand());
    }

    public int FromID;
    public int ToID;

    public Resources Giving;
    public Resources Receiving;

    public bool Hidden;

    private readonly Catan m_Board;

    public bool Complete { get; private set; }
}