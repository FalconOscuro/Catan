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

    public Game1()
    {
        m_Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
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

        m_ShapeBatcher.End();

        base.Draw(gameTime);

        m_GuiRenderer.BeginLayout(gameTime);
        ImGui.Begin("Debug Interface");

        ImGui.End();
        m_GuiRenderer.EndLayout();
    }
}
