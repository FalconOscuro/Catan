using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using ImGuiNET;


namespace Catan;

// TODO:
// Power cards
// Trading
// Bank

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
            m_Tiles[i] = new Tile();

        for (int i = 0; i < 54; i++)
            m_Nodes[i] = new Node();
        
        for (int i = 0; i < 72; i++)
            m_Edges[i] = new Edge();
        
        m_Players[0] = new Player(this, Color.Red);
        m_Players[1] = new Player(this, Color.White);
        m_Players[2] = new Player(this, Color.Orange);
        m_Players[3] = new Player(this, Color.Blue);

        m_Players[0].SetState(Player.TurnState.PreGame1);
        m_State = GameState.Pregame1;

        ResourceBank = new Resources(19, 19, 19, 19, 19);

        Vector2 centrePos = new Vector2(screenWidth, screenHeight) / 2f;
        PositionObjects(centrePos);
        MapObjects();

        PositionEdges();

        GenerateBoard();
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

        m_Tiles[0].Position = hexDistTL2;
        m_Tiles[1].Position = hexDistTL + hexDistTR;
        m_Tiles[2].Position = hexDistTR2;
        m_Tiles[3].Position = hexDistTL2 - hexDistTR;
        m_Tiles[4].Position = hexDistTL;
        m_Tiles[5].Position = hexDistTR;
        m_Tiles[6].Position = hexDistTR2 - hexDistTL;
        m_Tiles[7].Position = hexDistTL2 - hexDistTR2;
        m_Tiles[8].Position = hexDistTL - hexDistTR;
        m_Tiles[9].Position = Vector2.Zero;
        m_Tiles[10].Position = hexDistTR - hexDistTL;
        m_Tiles[11].Position = hexDistTR2 - hexDistTL2;
        m_Tiles[12].Position = hexDistTL - hexDistTR2;
        m_Tiles[13].Position = -hexDistTR;
        m_Tiles[14].Position = -hexDistTL;
        m_Tiles[15].Position = hexDistTR - hexDistTL2;
        m_Tiles[16].Position = -hexDistTR2;
        m_Tiles[17].Position = -hexDistTL - hexDistTR;
        m_Tiles[18].Position = -hexDistTL2;

        for(int i = 0; i < 19; i++)
            m_Tiles[i].Position += centrePos;
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
            m_Nodes[i * 2].Position = m_Tiles[i].Position + pointDistTL;
            m_Nodes[(i * 2) + 1].Position = m_Tiles[i].Position + up;
        }
        m_Nodes[6].Position = m_Tiles[2].Position + pointDistTR;

        for (int i = 3; i < 7; i++)
        {
            m_Nodes[(i * 2) + 1].Position = m_Tiles[i].Position + pointDistTL;
            m_Nodes[(i + 1) * 2].Position = m_Tiles[i].Position + up;
        }
        m_Nodes[15].Position = m_Tiles[6].Position + pointDistTR;

        for (int i = 7; i < 12; i++)
        {
            m_Nodes[(i + 1) * 2].Position = m_Tiles[i].Position + pointDistTL;
            m_Nodes[(i * 2) + 3].Position = m_Tiles[i].Position + up;
        }
        m_Nodes[26].Position = m_Tiles[11].Position + pointDistTR;

        m_Nodes[27].Position = m_Tiles[7].Position - pointDistTR;
        for (int i = 12; i < 16; i++)
        {
            m_Nodes[(i + 2) * 2].Position = m_Tiles[i].Position + pointDistTL;
            m_Nodes[(i * 2) + 5].Position = m_Tiles[i].Position + up;
        }
        m_Nodes[36].Position = m_Tiles[15].Position + pointDistTR;
        m_Nodes[37].Position = m_Nodes[36].Position + pointDistTR;

        m_Nodes[38].Position = m_Nodes[28].Position - up;
        for (int i = 16; i < 19; i++)
        {
            m_Nodes[(i * 2) + 7].Position = m_Tiles[i].Position + pointDistTL;
            m_Nodes[(i + 4) * 2].Position = m_Tiles[i].Position + up;
        }
        m_Nodes[45].Position = m_Nodes[44].Position - pointDistTL;
        m_Nodes[46].Position = m_Nodes[45].Position + pointDistTR;

        for (int i = 16; i < 19; i++)
        {
            m_Nodes[(i * 2) + 15].Position = m_Tiles[i].Position - pointDistTR;
            m_Nodes[(i + 8) * 2].Position = m_Tiles[i].Position - up;
        }
        m_Nodes[53].Position = m_Nodes[52].Position + pointDistTR;
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

    private void MapAboveTile(int tileIndex, int nodeIndex)
    {
        m_Nodes[nodeIndex].Tiles[1] = m_Tiles[tileIndex];
        m_Tiles[tileIndex].Nodes[0] = m_Nodes[nodeIndex++];

        m_Nodes[nodeIndex].Tiles[2] = m_Tiles[tileIndex];
        m_Tiles[tileIndex].Nodes[1] = m_Nodes[nodeIndex++];

        m_Nodes[nodeIndex].Tiles[2] = m_Tiles[tileIndex];
        m_Tiles[tileIndex].Nodes[2] = m_Nodes[nodeIndex++];
    }

    private void MapBelowTile(int tileIndex, int nodeIndex)
    {
        m_Nodes[nodeIndex].Tiles[1] = m_Tiles[tileIndex];
        m_Tiles[tileIndex].Nodes[3] = m_Nodes[nodeIndex++];

        m_Nodes[nodeIndex].Tiles[0] = m_Tiles[tileIndex];
        m_Tiles[tileIndex].Nodes[4] = m_Nodes[nodeIndex++];

        m_Nodes[nodeIndex].Tiles[0] = m_Tiles[tileIndex];
        m_Tiles[tileIndex].Nodes[5] = m_Nodes[nodeIndex++];
    }

    private void MapEdges()
    {
        // Row 1
        for (int i = 0; i < 6; i++)
        {
            m_Edges[i].Nodes[0] = m_Nodes[i];
            m_Edges[i].Nodes[1] = m_Nodes[i + 1];
        }

        // Row 2
        for (int i = 6; i < 10; i++)
        {
            m_Edges[i].Nodes[0] = m_Nodes[(i - 6) * 2];
            m_Edges[i].Nodes[1] = m_Nodes[((i - 2) * 2)];
        }

        // Row 3
        for (int i = 10; i < 18; i++)
        {
            m_Edges[i].Nodes[0] = m_Nodes[i - 3];
            m_Edges[i].Nodes[1] = m_Nodes[i - 2];
        }

        // Row 4
        for (int i = 18; i < 23; i++)
        {
            m_Edges[i].Nodes[0] = m_Nodes[((i - 15) * 2) + 1];
            m_Edges[i].Nodes[1] = m_Nodes[((i - 10) * 2) + 1];
        }

        // Row 5
        for (int i = 23; i < 33; i++)
        {
            m_Edges[i].Nodes[0] = m_Nodes[i - 7];
            m_Edges[i].Nodes[1] = m_Nodes[i - 6];
        }

        // Row 6
        for (int i = 33; i < 39; i++)
        {
            m_Edges[i].Nodes[0] = m_Nodes[(i - 25) * 2];
            m_Edges[i].Nodes[1] = m_Nodes[((i - 20) * 2) + 1];
        }

        // Row 7
        for (int i = 39; i < 49; i++)
        {
            m_Edges[i].Nodes[0] = m_Nodes[i - 12];
            m_Edges[i].Nodes[1] = m_Nodes[i - 11];
        }

        // Row 8
        for (int i = 49; i < 54; i++)
        {
            m_Edges[i].Nodes[0] = m_Nodes[(i - 35) * 2];
            m_Edges[i].Nodes[1] = m_Nodes[(i - 30) * 2];
        }

        // Row 9
        for (int i = 54; i < 62; i++)
        {
            m_Edges[i].Nodes[0] = m_Nodes[i - 16];
            m_Edges[i].Nodes[1] = m_Nodes[i - 15];
        }

        // Row 10
        for (int i = 62; i < 66; i++)
        {
            m_Edges[i].Nodes[0] = m_Nodes[((i - 43) * 2) + 1];
            m_Edges[i].Nodes[1] = m_Nodes[((i - 39) * 2) + 1];
        }

        // Row 11
        for (int i = 66; i < 72; i++)
        {
            m_Edges[i].Nodes[0] = m_Nodes[i - 19];
            m_Edges[i].Nodes[1] = m_Nodes[i - 18];
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
            m_Tiles[i].Type = resourceSpread[i];

            if (resourceSpread[i] == Resources.Type.Empty)
            {
                m_Tiles[i].Value = 7;
                m_Tiles[i].Robber = true;
                m_RobberPos = m_Tiles[i];
                continue;
            }

            m_DensityMap[RollToArrayPos(numSpread[n])].Add(m_Tiles[i]);
            m_Tiles[i].Value = numSpread[n++];
        }
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
            m_Players[m_CurrentPlayer + m_TargetPlayerOffset].SetState((Player.TurnState)m_State);
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
            trades.AddRange(tile.Distribute());

        Resources requested = new Resources();
        foreach (Trade trade in trades)
            requested = requested + trade.Materials;
        
        Resources mask = new Resources(1, 1, 1, 1, 1);
        for (Resources.Type i = 0; (int)i < 5; i++)
            if (ResourceBank.GetType(i) < requested.GetType(i))
                mask.SetType(i, 0);
        
        for (int i = 0; i < trades.Count; i++)
        {
            trades[i].From = ResourceBank;
            trades[i].Materials = trades[i].Materials * mask;

            trades[i].TryExecute();
        }
    }

    public void Update()
    {
        if (m_State == GameState.End)
            return;

        AdvanceTurn();

        bool pressed = Mouse.GetState().LeftButton.HasFlag(ButtonState.Pressed);
        Vector2 mousePos = Mouse.GetState().Position.FlipY(Game1.WindowDimensions.Y);
        foreach (Node node in m_Nodes)
            if(node.TestCollision(mousePos))
            {
                if (pressed)
                    m_Players[m_CurrentPlayer + m_TargetPlayerOffset].SelectNode(node);
                return;
            }
            
        foreach (Edge edge in m_Edges)
            if (edge.TestCollision(mousePos))
            {
                if (pressed)
                    m_Players[m_CurrentPlayer + m_TargetPlayerOffset].SelectEdge(edge);
                return;
            }

        foreach (Tile tile in m_Tiles)
            if (tile.TestCollision(mousePos, m_Scale))
            {
                if (pressed)
                    m_Players[m_CurrentPlayer + m_TargetPlayerOffset].SelectTile(tile);
                return;
            }
    }

    private void AdvanceTurn()
    {
        if (!m_Players[m_CurrentPlayer + m_TargetPlayerOffset].HasTurnEnded() || CheckVictory())
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
                m_Players[m_CurrentPlayer].SetState(Player.TurnState.Robber);
                return;
            }

            else if ((++m_TargetPlayerOffset) + m_CurrentPlayer > 3)
                m_TargetPlayerOffset = 0 - m_CurrentPlayer;
            
            break;
        }

        m_Players[m_CurrentPlayer + m_TargetPlayerOffset].SetState((Player.TurnState)m_State);
    }

    private bool CheckVictory()
    {
        if (m_Players[m_CurrentPlayer].HasWon())
        {
            m_State = GameState.End;
            m_Players[m_CurrentPlayer].SetState(Player.TurnState.End);
            return true;
        }

        return false;
    }

    public void ShapeDraw(ShapeBatcher shapeBatcher)
    {
        for (int i = 0; i < 19; i++)
            m_Tiles[i].ShapeDraw(shapeBatcher, m_Scale);
        
        for (int i = 0; i < 72; i++)
            m_Edges[i].Draw(shapeBatcher);

        for (int i = 0; i < 54; i++)
            m_Nodes[i].Draw(shapeBatcher);
    }

    public void SpriteDraw(SpriteBatch spriteBatch, float windowHeight)
    {
        for (int i = 0; i < 19; i++)
            m_Tiles[i].SpriteDraw(spriteBatch, m_Font, windowHeight, m_LastRoll);
    }

    public void DebugUIDraw()
    {
        ImGui.Text("Bank");
        ResourceBank.UIDraw(true);
        ImGui.Separator();

        if (ImGui.Button("Shuffle Tiles"))
            GenerateBoard(false);

        ImGui.Separator();

        if (ImGui.BeginTabBar("Players"))
        {
            if (ImGui.BeginTabItem("Player 0"))
            {
                m_Players[0].DebugDrawUI();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Player 1"))
            {
                m_Players[1].DebugDrawUI();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Player 2"))
            {
                m_Players[2].DebugDrawUI();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Player 3"))
            {
                m_Players[3].DebugDrawUI();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }

    public void GameUIDraw()
    {
        ImGui.Text(String.Format("Player {0}", m_CurrentPlayer + m_TargetPlayerOffset));
        ImGui.Separator();

        m_Players[m_CurrentPlayer + m_TargetPlayerOffset].GameDrawUI();
    }

    private Tile[] m_Tiles = new Tile[19];
    private Tile m_RobberPos;

    private Node[] m_Nodes = new Node[54];

    private Edge[] m_Edges = new Edge[72];

    private List<List<Tile>> m_DensityMap;

    private Player[] m_Players = new Player[4];
    private int m_CurrentPlayer = 0;
    private int m_TargetPlayerOffset = 0;

    private float m_Scale;

    private float m_EdgeDist;

    private SpriteFont m_Font;

    private int m_LastRoll;

    public Resources ResourceBank;

    private enum GameState
    {
        Pregame1 = (int)Player.TurnState.PreGame1,
        Pregame2 = (int)Player.TurnState.Pregame2,
        Main = (int)Player.TurnState.Start,
        Robber = (int)Player.TurnState.Discard,
        End
    }

    private GameState m_State;
}