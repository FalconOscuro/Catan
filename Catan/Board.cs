using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using ImGuiNET;


namespace Catan;

class Board
{
    //  Maximum board size as a percentage of an axis
    private static readonly float BOARD_SCREEN_FILL_PC = .9f;

    public Board(int screenWidth, int screenHeight, SpriteFont font)
    {
        m_Font = font;

        float scaleY = (screenHeight * BOARD_SCREEN_FILL_PC) / 8f;
        float scaleX = (screenWidth * BOARD_SCREEN_FILL_PC) / 10f;

        // Use scale for smallest axis to prevent overspill
        m_Scale = MathHelper.Min(scaleX, scaleY);

        // x^2 + (.5x)^2 = x^2 + .25x^2 = 1.25x^2
        m_EdgeDist = MathF.Sqrt((m_Scale * m_Scale) * 1.25f);

        for (int i = 0; i < 19; i++)
            Tiles[i] = new Tile(i);

        for (int i = 0; i < 54; i++)
            Nodes[i] = new Node(i);
        
        for (int i = 0; i < 72; i++)
            m_Edges[i] = new Edge();
        
        Players[0] = new PlayerAgent(this, Color.Red);
        Players[1] = new PlayerAgent(this, Color.Orange);
        Players[2] = new PlayerAgent(this, Color.White);
        Players[3] = new PlayerAgent(this, Color.Blue);

        Players[0].SetState(Player.TurnState.PreGame1);
        m_State = GameState.Setup;

        ResourceBank = new Resources(19, 19, 19, 19, 19);

        Vector2 centrePos = new Vector2(screenWidth, screenHeight) / 2f;
        PositionObjects(centrePos);
        MapObjects();

        PositionEdges();

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

        Random rand = new Random();

        rand.ShuffleArray(tempArray, 2);
        DevelopmentCards = new Queue<DevelopmentCard>(tempArray);
    }

    /// <summary>
    /// Position all board elements
    /// </summary>
    /// <param name="centrePos">Board centre position</param>
    private void PositionObjects(Vector2 centrePos)
    {
        PositionTiles(centrePos);
        
        PositionNodes();
    }

    /// <summary>
    /// Position all tiles
    /// </summary>
    private void PositionTiles(Vector2 centrePos)
    {
        Vector2 hexDistTL = new Vector2(-ShapeBatcher.SIN_60, 1.5f) * m_Scale;
        Vector2 hexDistTL2 = hexDistTL * 2;

        Vector2 hexDistTR = new Vector2(-hexDistTL.X, hexDistTL.Y);
        Vector2 hexDistTR2 = hexDistTR * 2;

        Tiles[0].Position = hexDistTL2;
        Tiles[1].Position = hexDistTL + hexDistTR;
        Tiles[2].Position = hexDistTR2;
        Tiles[3].Position = hexDistTL2 - hexDistTR;
        Tiles[4].Position = hexDistTL;
        Tiles[5].Position = hexDistTR;
        Tiles[6].Position = hexDistTR2 - hexDistTL;
        Tiles[7].Position = hexDistTL2 - hexDistTR2;
        Tiles[8].Position = hexDistTL - hexDistTR;
        Tiles[9].Position = Vector2.Zero;
        Tiles[10].Position = hexDistTR - hexDistTL;
        Tiles[11].Position = hexDistTR2 - hexDistTL2;
        Tiles[12].Position = hexDistTL - hexDistTR2;
        Tiles[13].Position = -hexDistTR;
        Tiles[14].Position = -hexDistTL;
        Tiles[15].Position = hexDistTR - hexDistTL2;
        Tiles[16].Position = -hexDistTR2;
        Tiles[17].Position = -hexDistTL - hexDistTR;
        Tiles[18].Position = -hexDistTL2;

        for(int i = 0; i < 19; i++)
            Tiles[i].Position += centrePos;
    }

    /// <summary>
    /// Position all Nodes, relative to tiles
    /// </summary>
    private void PositionNodes()
    {
        Vector2 up = new Vector2(0, m_Scale);
        Vector2 pointDistTL = new Vector2(-ShapeBatcher.SIN_60, .5f) * m_Scale;
        Vector2 pointDistTR = new Vector2(-pointDistTL.X, pointDistTL.Y);

        for (int i = 0; i < 3; i++)
        {
            Nodes[i * 2].Position = Tiles[i].Position + pointDistTL;
            Nodes[(i * 2) + 1].Position = Tiles[i].Position + up;
        }
        Nodes[6].Position = Tiles[2].Position + pointDistTR;

        for (int i = 3; i < 7; i++)
        {
            Nodes[(i * 2) + 1].Position = Tiles[i].Position + pointDistTL;
            Nodes[(i + 1) * 2].Position = Tiles[i].Position + up;
        }
        Nodes[15].Position = Tiles[6].Position + pointDistTR;

        for (int i = 7; i < 12; i++)
        {
            Nodes[(i + 1) * 2].Position = Tiles[i].Position + pointDistTL;
            Nodes[(i * 2) + 3].Position = Tiles[i].Position + up;
        }
        Nodes[26].Position = Tiles[11].Position + pointDistTR;

        Nodes[27].Position = Tiles[7].Position - pointDistTR;
        for (int i = 12; i < 16; i++)
        {
            Nodes[(i + 2) * 2].Position = Tiles[i].Position + pointDistTL;
            Nodes[(i * 2) + 5].Position = Tiles[i].Position + up;
        }
        Nodes[36].Position = Tiles[15].Position + pointDistTR;
        Nodes[37].Position = Nodes[36].Position + pointDistTR;

        Nodes[38].Position = Nodes[28].Position - up;
        for (int i = 16; i < 19; i++)
        {
            Nodes[(i * 2) + 7].Position = Tiles[i].Position + pointDistTL;
            Nodes[(i + 4) * 2].Position = Tiles[i].Position + up;
        }
        Nodes[45].Position = Nodes[44].Position - pointDistTL;
        Nodes[46].Position = Nodes[45].Position + pointDistTR;

        for (int i = 16; i < 19; i++)
        {
            Nodes[(i * 2) + 15].Position = Tiles[i].Position - pointDistTR;
            Nodes[(i + 8) * 2].Position = Tiles[i].Position - up;
        }
        Nodes[53].Position = Nodes[52].Position + pointDistTR;
    }

    private void PositionEdges()
    {
        for (int i = 0; i < 72; i++)
            m_Edges[i].CalculatePosition();
    }

    private void MapObjects()
    {
        MapNodes();
        MapEdges();
        MapPorts();
    }

    private void MapNodes()
    {
        // Row 1
        for (int i = 0; i < 3; i++)
        {
            MapAboveTile(i, i * 2);
            MapBelowTile(i, (i * 2) + 8);
        }

        // Row 2
        for (int i = 3; i < 7; i++)
        {
            MapAboveTile(i, (i * 2) + 1);
            MapBelowTile(i, (i * 2) + 11);
        }

        // Row 3
        for (int i = 7; i < 12; i++)
        {
            MapAboveTile(i, (i * 2) + 2);
            MapBelowTile(i, (i * 2) + 13);
        }

        // Row 4
        for (int i = 12; i < 16; i++)
        {
            MapAboveTile(i, (i * 2) + 4);
            MapBelowTile(i, (i * 2) + 14);
        }

        // Row 5
        for (int i = 16; i < 19; i++)
        {
            MapAboveTile(i, (i * 2) + 7);
            MapBelowTile(i, (i * 2) + 15);
        }
    }

    private void MapPorts()
    {
        m_Ports[0] = new Port(Nodes[1], Nodes[0], Port.TradeType.Versatile);
        m_Ports[1] = new Port(Nodes[4], Nodes[3], Port.TradeType.Grain);
        m_Ports[2] = new Port(Nodes[15], Nodes[14], Port.TradeType.Ore);
        m_Ports[3] = new Port(Nodes[7], Nodes[17], Port.TradeType.Lumber);
        m_Ports[4] = new Port(Nodes[37], Nodes[26], Port.TradeType.Versatile);
        m_Ports[5] = new Port(Nodes[28], Nodes[38], Port.TradeType.Brick);
        m_Ports[6] = new Port(Nodes[45], Nodes[46], Port.TradeType.Wool);
        m_Ports[7] = new Port(Nodes[47], Nodes[48], Port.TradeType.Versatile);
        m_Ports[8] = new Port(Nodes[50], Nodes[51], Port.TradeType.Versatile);
    }

    private void MapAboveTile(int tileIndex, int nodeIndex)
    {
        Nodes[nodeIndex].Tiles[1] = Tiles[tileIndex];
        Tiles[tileIndex].Nodes[0] = Nodes[nodeIndex++];

        Nodes[nodeIndex].Tiles[2] = Tiles[tileIndex];
        Tiles[tileIndex].Nodes[1] = Nodes[nodeIndex++];

        Nodes[nodeIndex].Tiles[2] = Tiles[tileIndex];
        Tiles[tileIndex].Nodes[2] = Nodes[nodeIndex++];
    }

    private void MapBelowTile(int tileIndex, int nodeIndex)
    {
        Nodes[nodeIndex].Tiles[1] = Tiles[tileIndex];
        Tiles[tileIndex].Nodes[3] = Nodes[nodeIndex++];

        Nodes[nodeIndex].Tiles[0] = Tiles[tileIndex];
        Tiles[tileIndex].Nodes[4] = Nodes[nodeIndex++];

        Nodes[nodeIndex].Tiles[0] = Tiles[tileIndex];
        Tiles[tileIndex].Nodes[5] = Nodes[nodeIndex++];
    }

    private void MapEdges()
    {
        // Row 1
        for (int i = 0; i < 6; i++)
        {
            m_Edges[i].Nodes[0] = Nodes[i];
            m_Edges[i].Nodes[1] = Nodes[i + 1];
        }

        // Row 2
        for (int i = 6; i < 10; i++)
        {
            m_Edges[i].Nodes[0] = Nodes[(i - 6) * 2];
            m_Edges[i].Nodes[1] = Nodes[((i - 2) * 2)];
        }

        // Row 3
        for (int i = 10; i < 18; i++)
        {
            m_Edges[i].Nodes[0] = Nodes[i - 3];
            m_Edges[i].Nodes[1] = Nodes[i - 2];
        }

        // Row 4
        for (int i = 18; i < 23; i++)
        {
            m_Edges[i].Nodes[0] = Nodes[((i - 15) * 2) + 1];
            m_Edges[i].Nodes[1] = Nodes[((i - 10) * 2) + 1];
        }

        // Row 5
        for (int i = 23; i < 33; i++)
        {
            m_Edges[i].Nodes[0] = Nodes[i - 7];
            m_Edges[i].Nodes[1] = Nodes[i - 6];
        }

        // Row 6
        for (int i = 33; i < 39; i++)
        {
            m_Edges[i].Nodes[0] = Nodes[(i - 25) * 2];
            m_Edges[i].Nodes[1] = Nodes[((i - 20) * 2) + 1];
        }

        // Row 7
        for (int i = 39; i < 49; i++)
        {
            m_Edges[i].Nodes[0] = Nodes[i - 12];
            m_Edges[i].Nodes[1] = Nodes[i - 11];
        }

        // Row 8
        for (int i = 49; i < 54; i++)
        {
            m_Edges[i].Nodes[0] = Nodes[(i - 35) * 2];
            m_Edges[i].Nodes[1] = Nodes[(i - 30) * 2];
        }

        // Row 9
        for (int i = 54; i < 62; i++)
        {
            m_Edges[i].Nodes[0] = Nodes[i - 16];
            m_Edges[i].Nodes[1] = Nodes[i - 15];
        }

        // Row 10
        for (int i = 62; i < 66; i++)
        {
            m_Edges[i].Nodes[0] = Nodes[((i - 43) * 2) + 1];
            m_Edges[i].Nodes[1] = Nodes[((i - 39) * 2) + 1];
        }

        // Row 11
        for (int i = 66; i < 72; i++)
        {
            m_Edges[i].Nodes[0] = Nodes[i - 19];
            m_Edges[i].Nodes[1] = Nodes[i - 18];
        }

        foreach (Edge edge in m_Edges)
            for (int i = 0; i < 2; i++)
            {
                int n = -1;
                while (edge.Nodes[i].Edges[++n] != null);

                edge.Nodes[i].Edges[n] = edge;
            }
    }

    /// <summary>
    /// Arrange resources and tokens
    /// </summary>
    /// <param name="useDefault">Use default layout or randomize</param>
    public void GenerateBoard(bool useDefault = true)
    {
        m_DensityMap = new List<List<Tile>>();
        m_DensityMap.Capacity = 10;
        for (int i = 0; i < 10; i++)
            m_DensityMap.Add(new List<Tile>());

        Resources.Type[] resourceSpread = Tile.DEFAULT_RESOURCE_SPREAD;
        int[] numSpread = Tile.DEFAULT_NUMBER_SPREAD;

        if (!useDefault)
        {
            Random rand = new Random();
            rand.ShuffleArray(resourceSpread, 2);
            rand.ShuffleArray(numSpread, 2);
        }

        int n = 0;
        for (int i = 0; i < 19; i++)
        {
            Tiles[i].Type = resourceSpread[i];
            Tiles[i].Robber = false;

            if (resourceSpread[i] == Resources.Type.Empty)
            {
                Tiles[i].Value = 0;
                Tiles[i].Robber = true;
                m_RobberPos = Tiles[i];
                continue;
            }

            m_DensityMap[RollToArrayPos(numSpread[n])].Add(Tiles[i]);
            Tiles[i].Value = numSpread[n++];
        }
    }

    private void QuickStart()
    {
        Nodes[10].Owner = Players[0];
        Nodes[10].Edges[2].Owner = Players[0];
        //Players[0].RegisterNode(Nodes[10]);

        Nodes[13].Owner = Players[1];
        Nodes[13].Edges[0].Owner = Players[1];
        //Players[1].RegisterNode(Nodes[13]);

        Nodes[19].Owner = Players[2];
        Nodes[19].Edges[1].Owner = Players[2];
        //Players[2].RegisterNode(Nodes[19]);

        Nodes[29].Owner = Players[0];
        Nodes[29].Edges[2].Owner = Players[0];
        //Players[0].RegisterNode(Nodes[29]);

        Nodes[35].Owner = Players[2];
        Nodes[35].Edges[0].Owner = Players[2];
        //Players[2].RegisterNode(Nodes[35]);

        Nodes[40].Owner = Players[3];
        Nodes[40].Edges[2].Owner = Players[3];
        //Players[3].RegisterNode(Nodes[40]);

        Nodes[42].Owner = Players[1];
        Nodes[42].Edges[2].Owner = Players[1];
        //Players[1].RegisterNode(Nodes[42]);

        Nodes[44].Owner = Players[3];
        Nodes[44].Edges[0].Owner = Players[3];
        //Players[3].RegisterNode(Nodes[44]);


        Trade trade = new Trade(this);
        trade.From = null;

        trade.Giving = new Resources(2, 0, 1, 0, 0);
        trade.To = Players[0];
        trade.TryExecute();

        trade.Giving = new Resources(0, 0, 2, 0, 1);
        trade.To = Players[1];
        trade.TryExecute();

        trade.Giving = new Resources(1, 1, 1, 0, 0);
        trade.To = Players[2];
        trade.TryExecute();

        trade.Giving = new Resources(1, 1, 0, 0, 1);
        trade.To = Players[3];
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
        if (m_RobberPos != null)
            m_RobberPos.Robber = false;
        
        m_RobberPos = target;
        m_RobberPos.Robber = true;
    }

    public void RollDice()
    {
        Random rand = new Random();

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
        List<Trade> trades = new List<Trade>();
        foreach (Tile tile in m_DensityMap[RollToArrayPos(m_LastRoll)])
            trades.AddRange(tile.Distribute(this));

        Resources requested = new Resources();
        foreach (Trade trade in trades)
            requested = requested + trade.Giving;
        
        Resources mask = new Resources(1, 1, 1, 1, 1);
        for (Resources.Type i = 0; (int)i < 5; i++)
            if (ResourceBank.GetType(i) < requested.GetType(i))
                mask.SetType(i, 0);
        
        for (int i = 0; i < trades.Count; i++)
        {
            trades[i].From = null;
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
        foreach (Node node in Nodes)
            if(node.TestCollision(mousePos))
            {
                if (pressed)
                    Players[m_CurrentPlayer + m_TargetPlayerOffset].SelectNode(node);
                return;
            }
            
        foreach (Edge edge in m_Edges)
            if (edge.TestCollision(mousePos))
            {
                if (pressed)
                    Players[m_CurrentPlayer + m_TargetPlayerOffset].SelectEdge(edge);
                return;
            }

        foreach (Tile tile in Tiles)
            if (tile.TestCollision(mousePos, m_Scale))
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
        Trade trade = new Trade(this);
        trade.To = Players[m_CurrentPlayer];

        Resources mask = new Resources();
        mask.SetType(type, 1);

        for (int i = 0; i < 4; i++)
        {
            if (i == m_CurrentPlayer)
                continue;

            trade.Giving = Players[i].ResourceHand * mask;
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
            m_Ports[i].Draw(shapeBatcher);

        for (int i = 0; i < 19; i++)
            Tiles[i].ShapeDraw(shapeBatcher, m_Scale);
        
        for (int i = 0; i < 72; i++)
            m_Edges[i].Draw(shapeBatcher);

        for (int i = 0; i < 54; i++)
            Nodes[i].Draw(shapeBatcher);
    }

    public void SpriteDraw(SpriteBatch spriteBatch, float windowHeight)
    {
        for (int i = 0; i < 19; i++)
            Tiles[i].SpriteDraw(spriteBatch, m_Font, windowHeight, m_LastRoll);
        
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

    public Tile[] Tiles = new Tile[19];
    private Tile m_RobberPos;

    public Node[] Nodes = new Node[54];

    private Edge[] m_Edges = new Edge[72];

    private Port[] m_Ports = new Port[9];

    private List<List<Tile>> m_DensityMap;

    public Player[] Players = new Player[4];
    private int m_CurrentPlayer = 0;
    private int m_TargetPlayerOffset = 0;

    private float m_Scale;

    private float m_EdgeDist;

    private SpriteFont m_Font;

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