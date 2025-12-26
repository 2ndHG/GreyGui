using System;
using System.Linq;
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

        static Panel[] PanelGen()
        {
            Panel[] panels = new Panel[10];
            for (int i = 0; i < 10; ++i)
            {
                panels[i] = new Panel(colorMask: new(0 + i * 20, 153 + i * 10, 204 + i * 5), size: new(120 - i * 3, 30), paddingSide: 5, zIndex: 10, borderRadius: 10);
            }
            return panels;
        }
        panel = new Panel(colorMask: new(10, 10, 10), size: new Vector2(150, 400), paddingSide: 10, paddingTop: 10, zIndex: 10, borderRadius: 10).SetChildren(
            PanelGen()
        );


        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here
        var keyboardState = Keyboard.GetState();
        if (keyboardState.IsKeyDown(Keys.D))
        {
            panel.Size = panel.Size with { X = panel.Size.X + 2 };
        }
        else if (keyboardState.IsKeyDown(Keys.A))
        {
            panel.Size = panel.Size with { X = panel.Size.X - 2 };
        }
        if (keyboardState.IsKeyDown(Keys.W))
        {
            panel.Size = panel.Size with { Y = panel.Size.Y - 2 };
        }
        else if (keyboardState.IsKeyDown(Keys.S))
        {
            panel.Size = panel.Size with { Y = panel.Size.Y + 2 };
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        Point point = Mouse.GetState().Position;
        guiBatch.Draw(panel, renderContext, point);
        guiBatch.Flush(renderContext);
        // TODO: Add your drawing code here

        base.Draw(gameTime);
    }
}
