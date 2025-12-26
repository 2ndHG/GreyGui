using System;
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

        panel = new Panel(colorMask: Color.Gray, size: new Vector2(100, 100), paddingSide: 5, zIndex: 10).SetChildren([
            new Panel(colorMask:Color.Yellow, usePercentWidth: true, widthPercent: .6f, useHeightWidthRatio:true, heightWidthRatio:.8f, paddingSide: 5, zIndex: 10),
        ]);
        panel.AppendChild(new Panel(colorMask: Color.Green, size: new(200, 200)));


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
        guiBatch.Draw(panel, renderContext, new Point(50, 50));
        guiBatch.Flush(renderContext);
        // TODO: Add your drawing code here

        base.Draw(gameTime);
    }
}
