using ImGuiNET;

namespace Catan;

class Trade
{
    public Trade()
    {
        From = null;
        To = null;
        Giving = new Resources();
        Receiving = new Resources();

        Complete = false;
    }

    public bool TryExecute()
    {
        if (From == null || To == null || Giving == null || Receiving == null)
            return false;
        
        else if (Giving > From || Receiving > To)
            return false;
        
        From.TryTake(Giving);
        To.TryTake(Receiving);

        From.Add(Receiving);
        To.Add(Giving);

        Complete = true;

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

    public bool Complete { get; private set; }
}