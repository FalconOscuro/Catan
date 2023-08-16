using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using ImGuiNET;


namespace Catan;

/// <summary>
/// Base game class
/// </summary>
class Catan
{
    //  Maximum board size as a percentage of an axis
    private static readonly float BOARD_SCREEN_FILL_PC = .9f;

    /// <summary>
    /// Construct using screen dimensions to determine asset scaling
    /// </summary>
    /// <param name="font">font used for text elements</param>
    public Catan(int screenWidth, int screenHeight, SpriteFont font)
    {
        m_Font = font;

        float scaleY = screenHeight * BOARD_SCREEN_FILL_PC / 8f;
        float scaleX = screenWidth * BOARD_SCREEN_FILL_PC / 10f;

        // Use scale for smallest axis to prevent overspill
        m_Scale = MathHelper.Min(scaleX, scaleY);
        m_Offset = new Vector2(screenWidth, screenHeight) / 2f;

        BoardState = new Board();
        BoardState.Init();

        for (int i = 0; i < 4; i++)
            Players[i] = new PlayerAgent(this, i);

        Players[0].SetState(Player.TurnState.PreGame1);
        m_State = State.Setup;

        GenerateBoard();
        CreateDevCardDeck();
    }

    /// <summary>
    /// Create and shuffle development cards
    /// </summary>
    private void CreateDevCardDeck()
    {
        DevelopmentCard[] tempArray = new DevelopmentCard[25];

        for (int i = 0; i < 14; i++)
            tempArray[i] = new Knight();

        for (int i = 14; i < 19; i++)
            tempArray[i] = new VictoryPoint();

        tempArray[19] = new Monopoly();
        tempArray[20] = new Monopoly();
        tempArray[21] = new YearOfPlenty();
        tempArray[22] = new YearOfPlenty();
        tempArray[23] = new RoadBuilding();
        tempArray[24] = new RoadBuilding();

        Random rand = new();

        rand.ShuffleArray(tempArray, 2);
        DevelopmentCards = new Queue<DevelopmentCard>(tempArray);
    }

    /// <summary>
    /// Arrange resources and tokens
    /// </summary>
    /// <param name="useDefault">Use default layout or randomize</param>
    public void GenerateBoard(bool useDefault = true)
    {
        m_DensityMap = new List<List<Tile>>
        {
            Capacity = 10
        };
        for (int i = 0; i < 10; i++)
            m_DensityMap.Add(new List<Tile>());

        Resources.Type[] resourceSpread = Tile.DEFAULT_RESOURCE_SPREAD;
        int[] numSpread = Tile.DEFAULT_NUMBER_SPREAD;

        if (!useDefault)
        {
            Random rand = new();
            rand.ShuffleArray(resourceSpread, 2);
            rand.ShuffleArray(numSpread, 2);
        }

        int n = 0;
        for (int i = 0; i < 19; i++)
        {
            BoardState.Tiles[i].Type = resourceSpread[i];
            BoardState.Tiles[i].Robber = false;

            if (resourceSpread[i] == Resources.Type.Empty)
            {
                BoardState.Tiles[i].Value = 0;
                BoardState.Tiles[i].Robber = true;
                BoardState.RobberPos = i;
                continue;
            }

            m_DensityMap[RollToArrayPos(numSpread[n])].Add(BoardState.Tiles[i]);
            BoardState.Tiles[i].Value = numSpread[n++];
        }
    }

    /// <summary>
    /// DEPRECATED
    /// Initialize board using standard setup
    /// </summary>
    private void QuickStart()
    {
        BoardState.Nodes[10].OwnerID = 0;
        BoardState.Nodes[10].GetEdge(2).OwnerID = 0;
        //Players[0].RegisterNode(Nodes[10]);

        BoardState.Nodes[13].OwnerID = 1;
        BoardState.Nodes[13].GetEdge(0).OwnerID = 1;
        //Players[1].RegisterNode(Nodes[13]);

        BoardState.Nodes[19].OwnerID = 2;
        BoardState.Nodes[19].GetEdge(1).OwnerID = 2;
        //Players[2].RegisterNode(Nodes[19]);

        BoardState.Nodes[29].OwnerID = 0;
        BoardState.Nodes[29].GetEdge(2).OwnerID = 0;
        //Players[0].RegisterNode(Nodes[29]);

        BoardState.Nodes[35].OwnerID = 2;
        BoardState.Nodes[35].GetEdge(0).OwnerID = 2;
        //Players[2].RegisterNode(Nodes[35]);

        BoardState.Nodes[40].OwnerID = 3;
        BoardState.Nodes[40].GetEdge(2).OwnerID = 3;
        //Players[3].RegisterNode(Nodes[40]);

        BoardState.Nodes[42].OwnerID = 1;
        BoardState.Nodes[42].GetEdge(2).OwnerID = 1;
        //Players[1].RegisterNode(Nodes[42]);

        BoardState.Nodes[44].OwnerID = 3;
        BoardState.Nodes[44].GetEdge(0).OwnerID = 3;
        //Players[3].RegisterNode(Nodes[44]);


        Trade trade = new(this)
        {
            FromID = -1,

            Giving = new Resources(2, 0, 1, 0, 0),
            ToID = 0
        };
        trade.TryExecute();

        trade.Giving = new Resources(0, 0, 2, 0, 1);
        trade.ToID = 1;
        trade.TryExecute();

        trade.Giving = new Resources(1, 1, 1, 0, 0);
        trade.ToID = 2;
        trade.TryExecute();

        trade.Giving = new Resources(1, 1, 0, 0, 1);
        trade.ToID = 3;
        trade.TryExecute();

        Players[m_CurrentPlayer].SetState(Player.TurnState.End);
        m_CurrentPlayer = 0;
        m_State = State.Main;
        Players[0].SetState(Player.TurnState.Start);

        StartGame();
    }

    private void StartGame()
    {
        foreach (Player player in Players)
            player.StartGame();
    }

    public void MoveRobber(Tile target)
    {
        if (BoardState.RobberPos != -1)
            BoardState.Tiles[BoardState.RobberPos].Robber = false;

        BoardState.RobberPos = target.ID;
        target.Robber = true;
    }

    public void RollDice()
    {
        Random rand = new();

        int roll1 = rand.Next(6) + 1;
        int roll2 = rand.Next(6) + 1;

        Event.Log.Singleton.PostEvent(new Event.DiceRollEvent(m_CurrentPlayer, roll1, roll2));

        m_LastRoll = roll1 + roll2;

        if (m_LastRoll == 7)
        {
            m_State = State.Robber;
            m_TargetPlayerOffset = m_CurrentPlayer == 3 ? -3 : 1;
            Players[m_CurrentPlayer + m_TargetPlayerOffset].SetState((Player.TurnState)m_State);
            return;
        }

        DistributeResources();
    }

    private static int RollToArrayPos(int roll)
    {
        return roll - (roll > 7 ? 3 : 2);
    }

    private void DistributeResources()
    {
        List<Trade> trades = new();
        foreach (Tile tile in m_DensityMap[RollToArrayPos(m_LastRoll)])
            trades.AddRange(tile.Distribute(this));

        Resources requested = new();
        foreach (Trade trade in trades)
            requested += trade.Giving;

        Resources mask = new(1, 1, 1, 1, 1);
        for (Resources.Type i = 0; (int)i < 5; i++)
            if (BoardState.ResourceBank.GetType(i) < requested.GetType(i))
                mask.SetType(i, 0);

        for (int i = 0; i < trades.Count; i++)
        {
            trades[i].FromID = -1;
            trades[i].Giving = trades[i].Giving * mask;

            trades[i].TryExecute();
        }
    }

    public void Update()
    {
        if (m_State == State.End || m_State == State.Setup)
            return;

        Players[m_CurrentPlayer + m_TargetPlayerOffset].Update();

        AdvanceTurn();

        bool pressed = Mouse.GetState().LeftButton.HasFlag(ButtonState.Pressed);
        Vector2 mousePos = Mouse.GetState().Position.FlipY(Game1.WindowDimensions.Y);
        foreach (Node node in BoardState.Nodes)
            if (node.TestCollision(mousePos, m_Offset, m_Scale))
            {
                if (pressed)
                    Players[m_CurrentPlayer + m_TargetPlayerOffset].SelectNode(node);
                return;
            }

        foreach (Edge edge in BoardState.Edges)
            if (edge.TestCollision(mousePos, m_Offset, m_Scale))
            {
                if (pressed)
                    Players[m_CurrentPlayer + m_TargetPlayerOffset].SelectEdge(edge);
                return;
            }

        foreach (Tile tile in BoardState.Tiles)
            if (tile.TestCollision(mousePos, m_Offset, m_Scale))
            {
                if (pressed)
                    Players[m_CurrentPlayer + m_TargetPlayerOffset].SelectTile(tile);
                return;
            }
    }

    public void CheckLargestArmy()
    {
        int largestSize = 2;
        Player largestPlayer = null;
        Player currentLargest = null;

        for (int i = 0; i < 4; i++)
        {
            if (Players[i].ArmySize > largestSize)
                largestPlayer = Players[i];

            if (Players[i].LargestArmy)
                currentLargest = Players[i];
        }

        if (largestPlayer == null)
            return;

        else if (currentLargest == largestPlayer)
            return;

        else if (currentLargest == null)
            largestPlayer.LargestArmy = true;

        else if (currentLargest.ArmySize < largestPlayer.ArmySize)
        {
            currentLargest.LargestArmy = false;
            largestPlayer.LargestArmy = true;
        }
    }

    private void AdvanceTurn()
    {
        if (!Players[m_CurrentPlayer + m_TargetPlayerOffset].HasTurnEnded() || CheckVictory())
            return;

        switch (m_State)
        {
            case State.Main:
                if (++m_CurrentPlayer > 3)
                    m_CurrentPlayer = 0;
                CheckVictory();
                break;

            case State.Pregame1:
                if (++m_CurrentPlayer > 3)
                {
                    m_CurrentPlayer = 3;
                    m_State = State.Pregame2;
                }
                break;

            case State.Pregame2:
                if (--m_CurrentPlayer < 0)
                {
                    m_CurrentPlayer = 0;
                    m_State = State.Main;
                }
                break;

            case State.Robber:
                if (m_TargetPlayerOffset == 0)
                {
                    m_State = State.Main;
                    Players[m_CurrentPlayer].SetState(Player.TurnState.Robber);
                    return;
                }

                else if ((++m_TargetPlayerOffset) + m_CurrentPlayer > 3)
                    m_TargetPlayerOffset = 0 - m_CurrentPlayer;

                break;

            case State.Trade:
                if ((++m_TargetPlayerOffset) + m_CurrentPlayer > 3)
                    m_TargetPlayerOffset = 0 - m_CurrentPlayer;

                if (m_TargetPlayerOffset == 0 || m_ActiveTrade.Complete)
                {
                    m_State = State.Main;
                    m_TargetPlayerOffset = 0;
                    m_ActiveTrade = null;
                    return;
                }

                Players[m_CurrentPlayer + m_TargetPlayerOffset].SetActiveTrade(m_ActiveTrade);
                break;
        }

        Players[m_CurrentPlayer + m_TargetPlayerOffset].SetState((Player.TurnState)m_State);
    }

    private bool CheckVictory()
    {
        if (Players[m_CurrentPlayer].HasWon())
        {
            m_State = State.End;
            Players[m_CurrentPlayer].SetState(Player.TurnState.End);

            Event.Log.Singleton.PostEvent(
                new Event.Victory(m_CurrentPlayer));

            return true;
        }

        return false;
    }

    public void PostTrade(Trade trade)
    {
        if (trade == null)
            return;

        m_State = State.Trade;
        m_TargetPlayerOffset = 1;

        if (m_CurrentPlayer + m_TargetPlayerOffset > 3)
            m_TargetPlayerOffset = 0 - m_CurrentPlayer;

        Players[m_CurrentPlayer + m_TargetPlayerOffset].SetState(Player.TurnState.Trade);
        m_ActiveTrade = trade;
        Players[m_CurrentPlayer + m_TargetPlayerOffset].SetActiveTrade(m_ActiveTrade);
    }

    public void Monopoly(Resources.Type type)
    {
        Trade trade = new(this)
        {
            ToID = m_CurrentPlayer
        };

        Resources mask = new();
        mask.SetType(type, 1);

        for (int i = 0; i < 4; i++)
        {
            if (i == m_CurrentPlayer)
                continue;

            Resources targetHand = Players[i].GetHand();
            trade.Giving = targetHand * mask;
            trade.FromID = i;
            trade.TryExecute();
        }
    }

    public void OnCompleteTrade(Trade trade)
    {
        foreach (Player player in Players)
            player.OnTradeComplete(trade);
    }

    public void CheckLongestRoad(bool reCalc)
    {
        if (reCalc)
            foreach (Player player in Players)
                player.FindLongestRoad();

        Player current = Players[0];
        Player champ = null;

        for (int i = 0; i < 4; i++)
        {
            if (Players[i].RoadLength > current.RoadLength)
                current = Players[i];

            if (Players[i].LongestRoad)
                champ = Players[i];
        }

        if (current.RoadLength < 3)
            return;

        else if (champ == null)
            current.LongestRoad = true;

        else if (current.RoadLength > champ.RoadLength)
        {
            current.LongestRoad = true;
            champ.LongestRoad = false;
        }
    }

    public void ShapeDraw(ShapeBatcher shapeBatcher)
    {
        for (int i = 0; i < 9; i++)
            BoardState.Ports[i].Draw(shapeBatcher, m_Offset, m_Scale);

        for (int i = 0; i < 19; i++)
            BoardState.Tiles[i].ShapeDraw(shapeBatcher, m_Offset, m_Scale);

        for (int i = 0; i < 72; i++)
            BoardState.Edges[i].Draw(shapeBatcher, m_Offset, m_Scale);

        for (int i = 0; i < 54; i++)
            BoardState.Nodes[i].Draw(shapeBatcher, m_Offset, m_Scale);
    }

    public void SpriteDraw(SpriteBatch spriteBatch, float windowHeight)
    {
        for (int i = 0; i < 19; i++)
            BoardState.Tiles[i].SpriteDraw(spriteBatch, m_Offset, m_Scale, m_Font, windowHeight, m_LastRoll);

        for (int i = 0; i < 4; i++)
            Players[i].SpriteDraw(spriteBatch, m_Font, windowHeight);
    }

    public void DebugUIDraw()
    {
        if (ImGui.CollapsingHeader("Resources"))
        {
            ImGui.Text("Bank");
            BoardState.ResourceBank.UIDraw(true);
            ImGui.Separator();
        }

        if (ImGui.CollapsingHeader("Players"))
        {
            if (ImGui.BeginTabBar("Players"))
            {
                for (int i = 0; i < 4; i++)
                {
                    if (ImGui.BeginTabItem(string.Format("Player {0}", i)))
                    {
                        Players[i].DebugDrawUI();
                        ImGui.EndTabItem();
                    }
                }

                ImGui.EndTabBar();
            }
        }

        if (ImGui.CollapsingHeader("Log"))
        {
            Event.Log.Singleton.DebugDrawUI();
        }
    }

    public void GameUIDraw()
    {
        if (m_State == State.Setup)
        {
            if (ImGui.Button("Start Game"))
            {
                m_State = State.Pregame1;
                StartGame();
            }

            //ImGui.SameLine();
            //if (ImGui.Button("Quick Start"))
            //    QuickStart();

            ImGui.SameLine();
            if (ImGui.Button("Shuffle Board"))
                GenerateBoard(false);
        }


        else
        {
            ImGui.Text(String.Format("Player {0}", m_CurrentPlayer + m_TargetPlayerOffset));
            ImGui.Separator();

            Players[m_CurrentPlayer + m_TargetPlayerOffset].GameDrawUI();
        }
    }

    public ref Resources GetBank()
    {
        return ref BoardState.ResourceBank;
    }

    public Board BoardState;

    private List<List<Tile>> m_DensityMap;

    public Player[] Players = new Player[4];
    private int m_CurrentPlayer = 0;
    private int m_TargetPlayerOffset = 0;

    private readonly float m_Scale;
    private Vector2 m_Offset;

    private readonly SpriteFont m_Font;

    private int m_LastRoll;
    public Queue<DevelopmentCard> DevelopmentCards;

    private Trade m_ActiveTrade;

    public enum State
    {
        Pregame1 = (int)Player.TurnState.PreGame1,
        Pregame2 = (int)Player.TurnState.Pregame2,
        Main = (int)Player.TurnState.Start,
        Robber = (int)Player.TurnState.Discard,
        Trade = (int)Player.TurnState.Trade,
        End,
        Setup
    }

    private State m_State;

    public struct GameState : ICloneable
    {
        public GameState()
        {
            BoardState = new Board();
            Phase = State.Main;
        }

        public object Clone()
        {
            GameState clone = new()
            {
                BoardState = (Board)BoardState.Clone()
            };

            for (int i = 0; i < 4; i++)
                clone.PlayerStates[i] = (Player.PlayerStatus)PlayerStates[i].Clone();

            return clone;
        }

        public readonly bool IsPregame()
        {
            return Phase == State.Pregame1 || Phase == State.Pregame2;
        }

        public Board BoardState;
        public readonly Player.PlayerStatus[] PlayerStates = new Player.PlayerStatus[4];
        public State Phase;
    }

    public GameState GetGameState()
    {
        GameState gameState = new()
        {
            BoardState = (Board)BoardState.Clone(),
            Phase = m_State
        };

        for (int i = 0; i < 4; i++)
            gameState.PlayerStates[i] = (Player.PlayerStatus)Players[i].GetStatus().Clone();

        return gameState;
    }
}