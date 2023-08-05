using ImGuiNET;

namespace Catan;

class Trade
{
    public Trade(Catan board)
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
        if (Giving == null || Receiving == null || From == null || To == null)
            return false;
        
        else if (Giving > From || Receiving > To)
            return false;
        
        From.TryTake(Giving);
        To.TryTake(Receiving);

        From.Add(Receiving);
        To.Add(Giving);

        Complete = true;

        m_Board.OnCompleteTrade(this);

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

    public Resources From;
    public Resources To;

    public Resources Giving;
    public Resources Receiving;

    private Catan m_Board;

    public bool Complete { get; private set; }
}