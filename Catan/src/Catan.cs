using Grid.Hexagonal;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.ImGuiNet;
using System;
using System.Collections.ObjectModel;
using Type = Catan.Resources.Type;

namespace Catan;

/// <summary>
/// Core game logic
/// </summary>
public class Catan : Game
{
    private GraphicsDeviceManager m_Graphics;
    private Canvas m_Canvas;
    private ImGuiRenderer m_GuiRenderer;

    private static readonly int HIST_LEN = 500;
    private readonly float[] m_FrameTimes = new float[HIST_LEN];
    private int m_FrameIndex;
    private float m_TimeTotal;

    //private Board m_Board;
    private HexGrid m_HexGrid;

    public static SpriteFont s_Font {get; private set;}

    private Axial m_HexPos;

    private readonly int GRID_SIZE = 5;

    public static readonly ReadOnlyCollection<Type> DEFAULT_RESOURCE_SPREAD = new(new Type[]
        {Type.Wool, Type.Grain, Type.Brick,
            Type.Wool, Type.Grain, Type.Ore, Type.Lumber,
                Type.Ore, Type.Lumber, Type.Empty, Type.Lumber, Type.Grain,
                    Type.Brick, Type.Wool, Type.Brick, Type.Grain,
                        Type.Lumber, Type.Wool, Type.Ore
    });

    public static readonly ReadOnlyCollection<int> DEFAULT_NUMBER_SPREAD = new(new int[]
        {11, 6, 5, 
            5, 4, 3, 8, 
                8, 3, 11, 9, 
                    10, 4, 6, 12, 
                        9, 2, 10}
    );

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

        CreateGrid();
    }

    private void CreateGrid()
    {
        Vector2 viewSize = new(Window.ClientBounds.Width, Window.ClientBounds.Height);
        Vector2 offset = viewSize / 2;
        float height = viewSize.Y / 6;

        HexGrid.Builder builder = new();
        builder.pHexFactory = new TileFactory();

        m_HexGrid = builder.BuildHexGrid();

        m_HexGrid.Height = height;
        m_HexGrid.Offset = offset;
        m_HexGrid.Rotation = MathF.PI * 0.5f;


        int qStart = - GRID_SIZE / 2;
        int qEnd = qStart + GRID_SIZE;

        for (Axial pos = new(){q = qStart}; pos.q < qEnd; pos.q++)
        {
            int rStart = Math.Max(qStart, qStart - pos.q);
            int rEnd = rStart + GRID_SIZE - Math.Abs(pos.q);

            for (pos.r = rStart; pos.r < rEnd; pos.r++)
                m_HexGrid.CreateHex(pos);
        }
        
        SetupGrid();
    }

    private void SetupGrid()
    {
        int resourceCount = 0;
        int valueCount = 0;

        int qStart = - GRID_SIZE / 2;
        int qEnd = qStart + GRID_SIZE;

        for (Axial pos = new(){q = qStart}; pos.q < qEnd; pos.q++)
        {
            int rStart = Math.Max(qStart, qStart - pos.q);
            int rEnd = rStart + GRID_SIZE - Math.Abs(pos.q);

            for (pos.r = rStart; pos.r < rEnd; pos.r++)
            {
                m_HexGrid.TryGetHex(pos, out Hex hex);

                Tile tile = (Tile)hex;
                tile.Resource = DEFAULT_RESOURCE_SPREAD[resourceCount++];

                if (tile.Resource != Type.Empty)
                    tile.Value = DEFAULT_NUMBER_SPREAD[valueCount++];
            }
        }
    }

    protected override void Update(GameTime gameTime)
    {
        Vector2 mousePos = Mouse.GetState().Position.ToVector2().FlipY(GraphicsDevice.Viewport.Height);

        m_HexGrid.FindHex(mousePos, out m_HexPos);

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        m_Canvas.ScreenSize = new(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

        GraphicsDevice.Clear(Color.CornflowerBlue);
        m_Canvas.Begin();

        //m_Board.Draw();
        m_HexGrid.Draw(m_Canvas);

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
        
        ImGui.Text(string.Format("Current Hex: q={0} r={1}", m_HexPos.q, m_HexPos.r));

        ImGui.End();

        m_GuiRenderer.EndLayout();
    }
}
