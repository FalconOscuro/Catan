using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using ImGuiNET;
using MonoGame.ImGui;

namespace Catan;

public class Game1 : Game
{
    private GraphicsDeviceManager m_Graphics;
    private SpriteBatch m_SpriteBatch;

    private ShapeBatcher m_ShapeBatcher;
    private ImGuiRenderer m_GuiRenderer;

    private Board m_Board;

    private static readonly int HIST_LEN = 600;
    private int[] m_FrameTimes;
    private int m_FrameIndex;
    private int m_TimeTotal;

    public Game1()
    {
        m_Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        m_FrameTimes = new int[HIST_LEN];
        m_FrameIndex = 0;
        m_TimeTotal = 0;
    }

    protected override void Initialize()
    {
        m_GuiRenderer = new ImGuiRenderer(this).Initialize().RebuildFontAtlas();
        m_ShapeBatcher = new ShapeBatcher(this);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        m_SpriteBatch = new SpriteBatch(GraphicsDevice);

        m_Board = new Board(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, Content.Load<SpriteFont>("FontDefault"));

        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        m_ShapeBatcher.Begin();
        m_Board.ShapeDraw(m_ShapeBatcher);
        m_ShapeBatcher.End();

        m_SpriteBatch.Begin();
        m_Board.SpriteDraw(m_SpriteBatch, GraphicsDevice.Viewport.Height);
        m_SpriteBatch.End();

        base.Draw(gameTime);

        m_GuiRenderer.BeginLayout(gameTime);
        ImGui.Begin("Debug Interface");

        if (ImGui.CollapsingHeader("Performance"))
        {
            int frameTime = gameTime.ElapsedGameTime.Milliseconds;
            float frameRate = 1000f / frameTime;

            m_TimeTotal += frameTime - m_FrameTimes[m_FrameIndex];
            m_FrameTimes[m_FrameIndex++] = frameTime;

            if (m_FrameIndex >= HIST_LEN)
                m_FrameIndex = 0;

            float frameAvg = m_TimeTotal / (float)HIST_LEN;
            float fpsAvg = 1000f / frameAvg;

            ImGui.Text(String.Format("FrameTime: {0} ms", frameTime));
            ImGui.Text(String.Format("FrameRate: {0} fps", frameRate));

            ImGui.Separator();

            ImGui.Text(String.Format("FrameTimeAvg: {0} ms", frameAvg));
            ImGui.Text(String.Format("FrameRateAvg: {0} fps", fpsAvg));

            ImGui.Separator();

            bool fixedTimeStep = IsFixedTimeStep;
            ImGui.Checkbox("Use Fixed Timestep", ref fixedTimeStep);

            if (fixedTimeStep != IsFixedTimeStep)
                IsFixedTimeStep = fixedTimeStep;
        }

        if (ImGui.CollapsingHeader("Game Board"))
        {
            if (ImGui.Button("Shuffle Tiles"))
                m_Board.GenerateBoard(false);
        }

        if (ImGui.CollapsingHeader("Gameplay"))
        {
            if (ImGui.Button("Roll Dice"))
                m_Board.RollDice();
        }

        ImGui.End();
        m_GuiRenderer.EndLayout();
    }
}
