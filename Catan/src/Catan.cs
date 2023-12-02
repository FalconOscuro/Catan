using Grid.Hexagonal;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.ImGuiNet;
using System;

namespace Catan;

/// <summary>
/// Core game logic
/// </summary>
public class Catan : Game
{
    private GraphicsDeviceManager m_Graphics;
    public static SpriteBatch s_SpriteBatch {get; private set;}
    public static ShapeBatcher s_ShapeBatcher {get; private set;}
    private ImGuiRenderer m_GuiRenderer;

    private static readonly int HIST_LEN = 150;
    private readonly float[] m_FrameTimes = new float[HIST_LEN];
    private int m_FrameIndex;
    private float m_TimeTotal;

    private Board m_Board;
    private HexGrid<Hex> m_HexGrid;

    public static SpriteFont s_Font {get; private set;}

    private (int, int)? m_LastKey = null;

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
        s_SpriteBatch = new SpriteBatch(GraphicsDevice);

        m_GuiRenderer.RebuildFontAtlas();
        s_ShapeBatcher = new(this);

        s_Font = Content.Load<SpriteFont>("Default");

        Vector2 viewSize = new(Window.ClientBounds.Width, Window.ClientBounds.Height);
        Vector2 offset = viewSize / 2;
        float height = viewSize.Y / 6;

        //m_Board = new(this);
        m_HexGrid = new(s_ShapeBatcher)
        {
            Height = height,
            Offset = offset,
            Rotation = MathF.PI / 2
        };

        for (int row = -2; row < 3; row++)
        {
            int colCount = 5 - Math.Abs(row);
            int colStart = -colCount / 2;

            for (int col = colStart; col < colStart + colCount; col++)
                m_HexGrid[row, col] = new();
        }
    }

    protected override void Update(GameTime gameTime)
    {
        Vector2 mousePos = Mouse.GetState().Position.ToVector2().FlipY(GraphicsDevice.Viewport.Height);

        if (m_HexGrid.FindHex(mousePos, out int x, out int y))
        {
            if (m_LastKey.HasValue)
                if (m_LastKey.Value != (x, y))
                    m_HexGrid[m_LastKey.Value.Item1, m_LastKey.Value.Item2].Colour = Color.Black;

            m_HexGrid[x, y].Colour = Color.Red;
            m_LastKey = (x, y);
        }

        else if (m_LastKey.HasValue)
        {
            m_HexGrid[m_LastKey.Value.Item1, m_LastKey.Value.Item2].Colour = Color.Black;
            m_LastKey = null;
        }

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        s_SpriteBatch.Begin();
        s_ShapeBatcher.Begin();

        //m_Board.Draw();
        m_HexGrid.Draw();

        s_ShapeBatcher.End();
        s_SpriteBatch.End();

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

        string keyText = "Current Hex: ";
        if (m_LastKey.HasValue)
            keyText += string.Format("{0} {1}", m_LastKey.Value.Item1, m_LastKey.Value.Item2);
        
        ImGui.Text(keyText);

        ImGui.End();

        m_GuiRenderer.EndLayout();
    }
}
