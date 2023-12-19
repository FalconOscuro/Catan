using System.Collections.Generic;

namespace Catan;

public interface IGamePhase
{
    void OnEnter(params object[] argn);

    // Redundant?
    void OnExit();

    List<IAction> GetValidActions(GameState gameState);

    void NextPhase(GameState gameState, IAction lastAction);
}