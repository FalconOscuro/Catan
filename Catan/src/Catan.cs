using System;
using System.Reflection.Metadata;
using Catan.Behaviour;
using Grid.Hexagonal;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.ImGuiNet;

using Utility;
using Utility.Graphics;

namespace Catan;

/// <summary>
/// Core game logic
/// </summary>
public class Catan : Microsoft.Xna.Framework.Game
{
    private GraphicsDeviceManager m_Graphics;
    private Canvas m_Canvas;
    private ImGuiRenderer m_GuiRenderer;

    private static readonly int HIST_LEN = 500;
    private readonly float[] m_FrameTimes = new float[HIST_LEN];
    private int m_FrameIndex;
    private float m_TimeTotal;

    private bool m_Step;
    private bool m_Paused = true;

    public static SpriteFont s_Font {get; private set;}

    private Axial m_HexPos;

    private Game m_Game;

    

    public Catan()
    {
        m_Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        m_GuiRenderer = new ImGuiRenderer(this);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        m_Canvas.spriteBatch = new SpriteBatch(GraphicsDevice);

        m_GuiRenderer.RebuildFontAtlas();
        m_Canvas.shapeBatcher = new(this);

        s_Font = Content.Load<SpriteFont>("Default");

        InitGame();
    }

    private void InitGame()
    {
        DMM[] dMMs = new DMM[]{
            new RandomDMM(),
            new RandomDMM(),
            new MCTS(){
                MaxThinkTime = 5
            },
            new MCTS()
        };
        m_Game = Game.NewDefaultMapGame(dMMs);

        // Position grid
        Vector2 screenSize = GraphicsDevice.Viewport.GetSizeVec();
        HexGrid hexGrid = m_Game.GameState.Board;
        hexGrid.Offset = screenSize / 2;
        hexGrid.Height = screenSize.Y / 6;
        hexGrid.Rotation = MathF.PI * 0.5f;
    }

    protected override void Update(GameTime gameTime)
    {
        if (m_Step || !m_Paused)
        {
            m_Game.Update();
            m_Step = false;
        }

        Vector2 mousePos = Mouse.GetState().Position.ToVector2().FlipY(GraphicsDevice.Viewport.Height);
        m_Game.GameState.Board.FindHex(mousePos, out m_HexPos);

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        m_Canvas.ScreenSize = GraphicsDevice.Viewport.GetSizeVec();

        GraphicsDevice.Clear(Color.CornflowerBlue);
        m_Canvas.Begin();

        m_Game.Draw(m_Canvas);

        m_Canvas.End();

        base.Draw(gameTime);

        ImDraw(gameTime);
    }

    /// <summary>
    /// Draw ImGUI window
    /// </summary>
    protected void ImDraw(GameTime gameTime)
    {
        m_GuiRenderer.BeginLayout(gameTime);

        ImGui.Begin("Debug Tools");

        if (ImGui.CollapsingHeader("Performance"))
        {
            float frameTime = gameTime.ElapsedGameTime.Milliseconds;
            float frameRate = 1000f / frameTime;

            m_TimeTotal += frameTime - m_FrameTimes[m_FrameIndex];
            m_FrameTimes[m_FrameIndex++] = frameTime;

            ImGui.PlotLines("Frame Times", ref m_FrameTimes[0], HIST_LEN, m_FrameIndex);

            if (m_FrameIndex >= HIST_LEN)
                m_FrameIndex = 0;

            float frameAvg = m_TimeTotal / HIST_LEN;
            float fpsAvg = 1000f / frameAvg;

            ImGui.Text(string.Format("FrameTime: {0} ms", frameTime));
            ImGui.Text(string.Format("FrameRate: {0} fps", frameRate));

            ImGui.Separator();

            ImGui.Text(string.Format("FrameTimeAvg: {0} ms", frameAvg));
            ImGui.Text(string.Format("FrameRateAvg: {0} fps", fpsAvg));

            ImGui.Separator();

            bool fixedTimeStep = IsFixedTimeStep;
            if (ImGui.Checkbox("Use Fixed Timestep", ref fixedTimeStep))
                IsFixedTimeStep = fixedTimeStep;
        }

        ImGui.Checkbox("Paused", ref m_Paused);

        if (m_Paused)
            m_Step = ImGui.Button("Step");
        
        ImGui.Text(string.Format("Current Hex: q={0} r={1}", m_HexPos.Q, m_HexPos.R));

        ImGui.End();

        ImGui.Begin("Game");

        if (ImGui.Button("Reset"))
            InitGame();

        m_Game.ImDraw();
        ImGui.End();

        //m_GameMaster.ImDraw();

        m_GuiRenderer.EndLayout();
    }
}
