using System;
using ImGuiNET;

using Grid.Hexagonal;
using Utility.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Utility;
using Catan.Action;
using Catan.Behaviour;
using System.Threading.Tasks;

namespace Catan;
using Type = Resources.Type;

/// <summary>
/// Controls a singular <see cref="GameState"/>
/// </summary>
/// <remarks>
/// For use with monogame.
/// </remarks>
public class Game
{
    public GameState GameState;

    private List<IAction> m_ValidActions = new();

    private Task<int> m_Task = null;
    private AsyncGameStateUpdate m_Delegate = null;

    private Game()
    {}

    /// <summary>
    /// Execute single update tick
    /// </summary>
    public void Update()
    {
        if (m_Task != null)
        {
            if (!m_Task.IsCompleted)
                return;
            
            int chosenAction = m_Task.Result;
            m_ValidActions[chosenAction].Execute(GameState);

            m_Task = null;
        }

        if (GameState.GetWinner() != -1)
            return;
        
        m_ValidActions = GameState.GetValidActions();

        // deep clone act list
        List<IAction> clonedActions = new(m_ValidActions.Count);
        foreach (IAction action in m_ValidActions)
            clonedActions.Add(action.Clone());

        //m_Delegate = new AsyncGameStateUpdate(GameState.GetCurrentPlayer().DMM.GetNextAction);
        m_Task = Task<int>.Factory.StartNew(() => {return GameState.GetCurrentPlayer().DMM.GetNextAction(GameState.Clone(), clonedActions);});
    }

    /// <summary>
    /// Draw all game elements
    /// </summary>
    public void Draw(Canvas canvas) {
        GameState.Board.Draw(canvas);
    }

    /// <summary>
    /// Draw debug elements with ImGui
    /// </summary>
    public void ImDraw()
    {
        GameState.ImDraw();
    }

    // Default start
    // Random map

    /// <summary>
    /// Create new game using default map
    /// </summary>
    public static Game NewDefaultMapGame(DMM[] dMMs) {
        return NewGame(dMMs, Rules.DEFAULT_RESOURCE_SPREAD, Rules.DEFAULT_VALUE_SPREAD);
    }

    /// <summary>
    /// Create new game specifying resource and value layout
    /// </summary>
    public static Game NewGame(DMM[] dMMs, Type[] resourceMap, int[] valueMap, Random random = null)
    {
        random ??= new();

        GameState gameState = new(){
            Bank = Rules.BANK_START.Clone()
        };

        // Need offset and rot?
        int resourceIndex = 0;
        int valueIndex = 0;

        int qStart = -Rules.BOARD_WIDTH / 2;
        int qEnd = qStart + Rules.BOARD_WIDTH;

        // iterate across columns
        for (Axial pos = new(){Q = qStart}; pos.Q < qEnd; pos.Q++)
        {
            int rStart = Math.Max(qStart, qStart - pos.Q);
            int rEnd = rStart + Rules.BOARD_WIDTH - Math.Abs(pos.Q);

            // iterate across rows
            for (pos.R = rStart; pos.R < rEnd; pos.R++)
            {
                // Create tile
                Tile tile = new(){
                    Resource = resourceMap[resourceIndex++]
                };

                // If desert make robber
                if (tile.Resource == Type.Empty)
                {
                    tile.Robber = true;
                    gameState.RobberPos = pos;
                }

                // Else assign value
                else
                {
                    tile.Value = valueMap[valueIndex++];

                    // Map value to tile pos
                    if (!gameState.TileValueMap.ContainsKey(tile.Value))
                        gameState.TileValueMap[tile.Value] = new();

                    gameState.TileValueMap[tile.Value].Add(pos);
                }

                gameState.Board.InsertHex(pos, tile);

                // Ensure surrounding paths are created
                for (Edge.Key key = new(){Position = pos}; key.Side < Edge.Side.SW + 1; key.Side++)
                    if (!gameState.Board.TryGetEdge<Edge>(key, out _))
                        gameState.Board.InsertEdge(key, new Path());

                // Ensure surrounding intersections are created
                for (Vertex.Key key = new(){Position = pos}; key.Side < Vertex.Side.SW + 1; key.Side++)
                    if (!gameState.Board.TryGetVertex<Vertex>(key, out _))
                        gameState.Board.InsertVertex(key, new Node());
            }

            // TODO: Ports
            // TODO: Players
        }

        // Setup Dev card deck
        // Knights are added first as they are normally the most numerous & first set of insertions do not require shuffling
        
        DevCards.Type[] devCardDeck = Array.Empty<DevCards.Type>();
        
        devCardDeck = InsertDevCards(devCardDeck, DevCards.Type.Knight, Rules.DEV_CARD_KNIGHT_COUNT).ToArray();
        devCardDeck = InsertDevCards(devCardDeck, DevCards.Type.VictoryPoint, Rules.DEV_CARD_VICTORY_POINT_COUNT).ToArray();
        devCardDeck = InsertDevCards(devCardDeck, DevCards.Type.RoadBuilding, Rules.DEV_CARD_ROAD_BUILDING_COUNT).ToArray();
        devCardDeck = InsertDevCards(devCardDeck, DevCards.Type.YearOfPlenty, Rules.DEV_CARD_YEAR_OF_PLENTY_COUNT).ToArray();
        devCardDeck = InsertDevCards(devCardDeck, DevCards.Type.Monopoly, Rules.DEV_CARD_MONOPOLY_COUNT).ToArray();

        // Shuffle
        random.Shuffle(devCardDeck);

        gameState.DevCardDeck = new (devCardDeck);

        // Shuffle DMMs to shuffle turn order, then assign to players
        random.Shuffle(dMMs);

        // Num players should be determined here not in rules
        for (int i = 0; i < Rules.NUM_PLAYERS; i++)
        {
            dMMs[i].OwnerID = i;
            gameState.Players[i].DMM = dMMs[i];
        }

        return new Game(){
            GameState = gameState
        };
    }

    private static IEnumerable<DevCards.Type> InsertDevCards(IEnumerable<DevCards.Type> deck, DevCards.Type type, int count)
    {
        DevCards.Type[] newCards = new DevCards.Type[count];
        for (int i = 0; i < count; i++)
            newCards[i] = type;
        
        return deck.Concat(newCards);
    }

    private delegate int AsyncGameStateUpdate(GameState gameState, List<IAction> actions);
}