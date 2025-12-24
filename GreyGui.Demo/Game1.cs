using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GreyGui.Demo;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Panel panel;
    private RenderContext renderContext = new();
    private GuiBatch guiBatch;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        GreyGui.Initialize(GraphicsDevice);

        guiBatch = new GuiBatch(GraphicsDevice);
        panel = new()
        {
            Size = new(100, 100),
            BorderRadius = 20,
            BorderWidth=5,
            colorMask=Color.Black
        };


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
        guiBatch.Draw(panel, renderContext, new Point(50, 50));
        guiBatch.Draw(panel, renderContext, new Point(75, 75));
        guiBatch.Flush(renderContext);
        // TODO: Add your drawing code here

        base.Draw(gameTime);
    }
}
