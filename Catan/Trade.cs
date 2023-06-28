namespace Catan;

class Trade
{
    public Trade()
    {
        From = null;
        To = null;
        Materials = new Resources();
    }

    public bool TryExecute()
    {
        if (From == null || To == null || Materials == null)
            return false;
        
        if (From.TryTake(Materials))
        {
            To.Add(Materials);
            return true;
        }
        
        return false;
    }

    public Resources From;
    public Resources To;

    public Resources Materials;
}