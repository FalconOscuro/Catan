using ImGuiNET;

namespace Catan;

abstract class DevelopmentCard
{
    protected DevelopmentCard(string name)
    {
        Name = name;
    }

    public abstract void Activate(Player owner);

    public virtual bool Playable { get; set; }

    public string Name { get; private set; }
}

class VictoryPoint : DevelopmentCard
{
    public VictoryPoint():
        base("Victory Point")
    {}

    public override void Activate(Player owner)
    {
        return;
    }

    public override bool Playable { get => false; set => base.Playable = false; }
}

class Knight : DevelopmentCard
{
    public Knight():
        base("Knight")
    {}

    public override void Activate(Player owner)
    {
        owner.SetState(Player.TurnState.Robber);
        owner.ArmySize++;
    }
}

class RoadBuilding : DevelopmentCard
{
    public RoadBuilding():
        base("Road Building")
    {}

    public override void Activate(Player owner)
    {
        owner.SetState(Player.TurnState.RoadBuilding);
    }
}

class YearOfPlenty : DevelopmentCard
{
    public YearOfPlenty():
        base("Year of Plenty")
    {}

    public override void Activate(Player owner)
    {
        owner.SetState(Player.TurnState.YearOfPlenty);
    }
}

class Monopoly : DevelopmentCard
{
    public Monopoly():
        base("Monopoly")
    {}

    public override void Activate(Player owner)
    {
        owner.SetState(Player.TurnState.Monopoly);
    }
}