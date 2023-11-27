using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.ImGuiNet;

namespace Catan;

/// <summary>
/// Core game logic
/// </summary>
public class Catan : Game
{
    private GraphicsDeviceManager m_Graphics;
    private SpriteBatch m_SpriteBatch;

    private ImGuiRenderer m_GuiRenderer;

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
        m_SpriteBatch = new SpriteBatch(GraphicsDevice);

        m_GuiRenderer.RebuildFontAtlas();

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

        // TODO: Add your drawing code here

        base.Draw(gameTime);

        ImDraw(gameTime);
    }

    /// <summary>
    /// Draw ImGUI window
    /// </summary>
    protected void ImDraw(GameTime gameTime)
    {
        m_GuiRenderer.BeginLayout(gameTime);

        ImGui.Begin("Debug tools");
        ImGui.End();

        m_GuiRenderer.EndLayout();
    }
}
