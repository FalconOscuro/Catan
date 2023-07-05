using ImGuiNET;

namespace Catan;

class Trade
{
    public Trade(Board board)
    {
        From = null;
        To = null;
        Giving = new Resources();
        Receiving = new Resources();
        m_Board = board;

        Complete = false;
    }

    public bool TryExecute()
    {
        Resources from = GetTarget(From);
        Resources to = GetTarget(To);

        if (Giving == null || Receiving == null)
            return false;
        
        else if (Giving > from || Receiving > to)
            return false;
        
        from.TryTake(Giving);
        to.TryTake(Receiving);

        from.Add(Receiving);
        to.Add(Giving);

        Complete = true;

        m_Board.OnCompleteTrade(this);

        return true;
    }

    private Resources GetTarget(Player player)
    {
        return player == null ? m_Board.ResourceBank : player.ResourceHand;
    }

    public void UIDraw(bool modify = true)
    {
        ImGui.Text("Giving");
        Giving.UIDraw(modify);

        ImGui.Separator();

        ImGui.Text("Recieving");
        Receiving.UIDraw(modify);
    }

    public Player From;
    public Player To;

    public Resources Giving;
    public Resources Receiving;

    private Board m_Board;

    public bool Complete { get; private set; }
}