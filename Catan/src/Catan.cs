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

    private static readonly int HIST_LEN = 500;
    private readonly float[] m_FrameTimes = new float[HIST_LEN];
    private int m_FrameIndex;
    private float m_TimeTotal;

    //private Board m_Board;
    private HexGrid m_HexGrid;

    public static SpriteFont s_Font {get; private set;}

    private Hex m_LastHex = null;

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

        HexGrid.Builder builder = new(s_ShapeBatcher);

        //m_Board = new(this);
        m_HexGrid = builder.BuildHexGrid();

        m_HexGrid.Height = height;
        m_HexGrid.Offset = offset;
        m_HexGrid.Rotation = 0;

        int size = 5;

        int qStart = - size / 2;
        int qEnd = qStart + size;

        for (Axial pos = new(){q = qStart}; pos.q < qEnd; pos.q++)
        {
            int rStart = Math.Max(qStart, qStart - pos.q);
            int rEnd = rStart + size - Math.Abs(pos.q);

            for (pos.r = rStart; pos.r < rEnd; pos.r++)
                m_HexGrid.CreateHex(pos);
        }
    }

    protected override void Update(GameTime gameTime)
    {
        Vector2 mousePos = Mouse.GetState().Position.ToVector2().FlipY(GraphicsDevice.Viewport.Height);

        if (m_HexGrid.FindHex(mousePos, out Axial pos))
        {
            if (m_LastHex != null)
                if (m_LastHex.Position != pos)
                {
                    m_LastHex.Colour = Color.Black;
                }

            if (m_HexGrid.TryGetHex(pos, out Hex hex))
            {
                hex.Colour = Color.Red;
                m_LastHex = hex;
            }

            else
                m_LastHex = null;
        }

        else if (m_LastHex != null)
        {
            m_LastHex.Colour = Color.Black;
            m_LastHex = null;
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
        if (m_LastHex != null)
            keyText += string.Format("{0} {1}", m_LastHex.Position.q, m_LastHex.Position.r);
        
        ImGui.Text(keyText);

        ImGui.End();

        m_GuiRenderer.EndLayout();
    }
}
