using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using ImGuiNET;


namespace Catan;

class Catan
{
    //  Maximum board size as a percentage of an axis
    private static readonly float BOARD_SCREEN_FILL_PC = .9f;

    public Catan(int screenWidth, int screenHeight, SpriteFont font)
    {
        m_Font = font;

        float scaleY = (screenHeight * BOARD_SCREEN_FILL_PC) / 8f;
        float scaleX = (screenWidth * BOARD_SCREEN_FILL_PC) / 10f;

        // Use scale for smallest axis to prevent overspill
        m_Scale = MathHelper.Min(scaleX, scaleY);

        Board = new Board();
        Board.Init();
        
        for (int i = 0; i < 4; i++)
            Players[i] = new PlayerAgent(this, i);

        Players[0].SetState(Player.TurnState.PreGame1);
        m_State = GameState.Setup;

        ResourceBank = new Resources(19, 19, 19, 19, 19);
        m_Offset = new Vector2(screenWidth, screenHeight) / 2f;

        GenerateBoard();
        CreateDevCardDeck();
    }

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
            Board.Tiles[i].Type = resourceSpread[i];
            Board.Tiles[i].Robber = false;

            if (resourceSpread[i] == Resources.Type.Empty)
            {
                Board.Tiles[i].Value = 0;
                Board.Tiles[i].Robber = true;
                Board.RobberPos = i;
                continue;
            }

            m_DensityMap[RollToArrayPos(numSpread[n])].Add(Board.Tiles[i]);
            Board.Tiles[i].Value = numSpread[n++];
        }
    }

    private void QuickStart()
    {
        Board.Nodes[10].OwnerID = 0;
        Board.Nodes[10].GetEdge(2).OwnerID = 0;
        //Players[0].RegisterNode(Nodes[10]);

        Board.Nodes[13].OwnerID = 1;
        Board.Nodes[13].GetEdge(0).OwnerID = 1;
        //Players[1].RegisterNode(Nodes[13]);

        Board.Nodes[19].OwnerID = 2;
        Board.Nodes[19].GetEdge(1).OwnerID = 2;
        //Players[2].RegisterNode(Nodes[19]);

        Board.Nodes[29].OwnerID = 0;
        Board.Nodes[29].GetEdge(2).OwnerID = 0;
        //Players[0].RegisterNode(Nodes[29]);

        Board.Nodes[35].OwnerID = 2;
        Board.Nodes[35].GetEdge(0).OwnerID = 2;
        //Players[2].RegisterNode(Nodes[35]);

        Board.Nodes[40].OwnerID = 3;
        Board.Nodes[40].GetEdge(2).OwnerID = 3;
        //Players[3].RegisterNode(Nodes[40]);

        Board.Nodes[42].OwnerID = 1;
        Board.Nodes[42].GetEdge(2).OwnerID = 1;
        //Players[1].RegisterNode(Nodes[42]);

        Board.Nodes[44].OwnerID = 3;
        Board.Nodes[44].GetEdge(0).OwnerID = 3;
        //Players[3].RegisterNode(Nodes[44]);


        Trade trade = new(this)
        {
            From = null,

            Giving = new Resources(2, 0, 1, 0, 0),
            To = Players[0].GetHand()
        };
        trade.TryExecute();

        trade.Giving = new Resources(0, 0, 2, 0, 1);
        trade.To = Players[1].GetHand();
        trade.TryExecute();

        trade.Giving = new Resources(1, 1, 1, 0, 0);
        trade.To = Players[2].GetHand();
        trade.TryExecute();

        trade.Giving = new Resources(1, 1, 0, 0, 1);
        trade.To = Players[3].GetHand();
        trade.TryExecute();

        Players[m_CurrentPlayer].SetState(Player.TurnState.End);
        m_CurrentPlayer = 0;
        m_State = GameState.Main;
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
        if (Board.RobberPos != -1)
            Board.Tiles[Board.RobberPos].Robber = false;
        
        Board.RobberPos = target.ID;
        target.Robber = true;
    }

    public void RollDice()
    {
        Random rand = new();

        m_LastRoll = rand.Next(6) + 2 + rand.Next(6);

        if (m_LastRoll == 7)
        {
            m_State = GameState.Robber;
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
            if (ResourceBank.GetType(i) < requested.GetType(i))
                mask.SetType(i, 0);
        
        for (int i = 0; i < trades.Count; i++)
        {
            trades[i].From = ResourceBank;
            trades[i].Giving = trades[i].Giving * mask;

            trades[i].TryExecute();
        }
    }

    public void Update()
    {
        if (m_State == GameState.End || m_State == GameState.Setup)
            return;

        Players[m_CurrentPlayer + m_TargetPlayerOffset].Update();

        AdvanceTurn();

        bool pressed = Mouse.GetState().LeftButton.HasFlag(ButtonState.Pressed);
        Vector2 mousePos = Mouse.GetState().Position.FlipY(Game1.WindowDimensions.Y);
        foreach (Node node in Board.Nodes)
            if(node.TestCollision(mousePos, m_Offset, m_Scale))
            {
                if (pressed)
                    Players[m_CurrentPlayer + m_TargetPlayerOffset].SelectNode(node);
                return;
            }
            
        foreach (Edge edge in Board.Edges)
            if (edge.TestCollision(mousePos, m_Offset, m_Scale))
            {
                if (pressed)
                    Players[m_CurrentPlayer + m_TargetPlayerOffset].SelectEdge(edge);
                return;
            }

        foreach (Tile tile in Board.Tiles)
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
        case GameState.Main:
            if (++m_CurrentPlayer > 3)
                m_CurrentPlayer = 0;
            CheckVictory();
            break;

        case GameState.Pregame1:
            if (++m_CurrentPlayer > 3)
            {
                m_CurrentPlayer = 3;
                m_State = GameState.Pregame2;
            }
            break;
        
        case GameState.Pregame2:
            if(--m_CurrentPlayer < 0)
            {
                m_CurrentPlayer = 0;
                m_State = GameState.Main;
            }
            break;

        case GameState.Robber:
            if (m_TargetPlayerOffset == 0)
            {
                m_State = GameState.Main;
                Players[m_CurrentPlayer].SetState(Player.TurnState.Robber);
                return;
            }

            else if ((++m_TargetPlayerOffset) + m_CurrentPlayer > 3)
                m_TargetPlayerOffset = 0 - m_CurrentPlayer;
            
            break;

        case GameState.Trade:
            if ((++m_TargetPlayerOffset) + m_CurrentPlayer > 3)
                m_TargetPlayerOffset = 0 - m_CurrentPlayer;

            if (m_TargetPlayerOffset == 0 || m_ActiveTrade.Complete)
            {
                m_State = GameState.Main;
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
            m_State = GameState.End;
            Players[m_CurrentPlayer].SetState(Player.TurnState.End);
            return true;
        }

        return false;
    }

    public void PostTrade(Trade trade)
    {
        if (trade == null)
            return;
        
        m_State = GameState.Trade;
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
            To = Players[m_CurrentPlayer].GetHand()
        };

        Resources mask = new();
        mask.SetType(type, 1);

        for (int i = 0; i < 4; i++)
        {
            if (i == m_CurrentPlayer)
                continue;

            Resources targetHand = Players[i].GetHand();
            trade.Giving = targetHand * mask;
            trade.From = targetHand;
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
            Board.Ports[i].Draw(shapeBatcher, m_Offset, m_Scale);

        for (int i = 0; i < 19; i++)
            Board.Tiles[i].ShapeDraw(shapeBatcher, m_Offset, m_Scale);
        
        for (int i = 0; i < 72; i++)
            Board.Edges[i].Draw(shapeBatcher, m_Offset, m_Scale);

        for (int i = 0; i < 54; i++)
            Board.Nodes[i].Draw(shapeBatcher, m_Offset, m_Scale);
    }

    public void SpriteDraw(SpriteBatch spriteBatch, float windowHeight)
    {
        for (int i = 0; i < 19; i++)
            Board.Tiles[i].SpriteDraw(spriteBatch, m_Offset, m_Scale, m_Font, windowHeight, m_LastRoll);
        
        for (int i = 0; i < 4; i++)
            Players[i].SpriteDraw(spriteBatch, m_Font, windowHeight);
    }

    public void DebugUIDraw()
    {
        if (ImGui.CollapsingHeader("Resources"))
        {
            ImGui.Text("Bank");
            ResourceBank.UIDraw(true);
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
    }

    public void GameUIDraw()
    {
        if (m_State == GameState.Setup)
        {
            if (ImGui.Button("Start Game"))
            {
                m_State = GameState.Pregame1;
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

    public Board Board;

    private List<List<Tile>> m_DensityMap;

    public Player[] Players = new Player[4];
    private int m_CurrentPlayer = 0;
    private int m_TargetPlayerOffset = 0;

    private readonly float m_Scale;
    private Vector2 m_Offset;

    private readonly SpriteFont m_Font;

    private int m_LastRoll;

    public Resources ResourceBank;
    public Queue<DevelopmentCard> DevelopmentCards;

    private Trade m_ActiveTrade;

    private enum GameState
    {
        Pregame1 = (int)Player.TurnState.PreGame1,
        Pregame2 = (int)Player.TurnState.Pregame2,
        Main = (int)Player.TurnState.Start,
        Robber = (int)Player.TurnState.Discard,
        Trade = (int)Player.TurnState.Trade,
        End,
        Setup
    }

    private GameState m_State;
}