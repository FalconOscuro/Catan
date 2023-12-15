namespace Catan;

/// <summary>
/// Structure for Command patter of actions executable by a player
/// </summary>
public interface IAction
{
    void Execute(ref GameState gameState);
}