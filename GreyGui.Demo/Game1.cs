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

        panel = GeneratePanel1();

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

        if (keyboardState.IsKeyDown(Keys.R))
        {
            panel = GeneratePanelWireFrame();
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(50, 50, 50));
        Point point = Mouse.GetState().Position;
        guiBatch.Draw(panel, renderContext, new Point(30, 30));
        guiBatch.Flush(renderContext);
        // TODO: Add your drawing code here

        base.Draw(gameTime);
    }

    private Panel GeneratePanel1()
    {
        static Panel[] PanelGen()
        {
            Panel[] panels = new Panel[10];
            for (int i = 0; i < 10; ++i)
            {
                panels[i] = new Panel(colorMask: new(0 + i * 20, 153 + i * 10, 204 + i * 5), size: new(120 - i * 3, 30), paddingSide: 5, zIndex: 10, borderRadius: 10);
            }
            return panels;
        }
        panel = new Panel(colorMask: new(10, 10, 10), borderColor: Color.Gray, size: new Vector2(150, 400), paddingSide: 10, paddingTop: 10, zIndex: 10, borderRadius: 10, layoutMode: PanelLayoutMode.Spread, childGap: 15f, rowGap: 6f).SetChildren(
            PanelGen()
        );
        return panel;
    }
    private Panel GeneratePanelWireFrame()
    {
        static Panel[] PanelItemGen()
        {
            Panel[] panels = new Panel[10];
            for (int i = 0; i < 10; ++i)
            {
                panels[i] = new Panel(colorMask: new(20 + i * 10, 100 + i * 5, 100 + i * 3), size: new(25, 25), paddingSide: 5, zIndex: 10, borderRadius: 10);
            }
            return panels;
        }
        Panel panel = new Panel(colorMask: new(10, 10, 10), borderColor: Color.Gray, size: new Vector2(740, 435), paddingSide: 10, paddingTop: 10, zIndex: 10, borderRadius: 10, layoutMode: PanelLayoutMode.Spread, rowGap: 6f).SetChildren([
            new Panel(colorMask: new(80, 80, 80), borderColor: Color.Gray, size: new Vector2(0, 405), usePercentWidth: true,
            paddingTop:3, paddingSide: 3, widthPercent:.2f, zIndex: 10, borderRadius: 10, layoutMode: PanelLayoutMode.Spread, childGap: 15f, rowGap: 6f).SetChildren([
                new Panel(colorMask: new(150 , 230, 255), borderColor: Color.Gray, size: new Vector2(0, 30), usePercentWidth: true, widthPercent:1f, zIndex: 10, borderRadius: 7, layoutMode: PanelLayoutMode.Spread, childGap: 15f, rowGap: 6f),
                new Panel(colorMask: new(0, 0, 0, 0), usePercentWidth: true,size: new(0, 150), widthPercent:1f, rowGap:3, childGap:3, layoutMode: PanelLayoutMode.Left).SetChildren(PanelItemGen()),
                new Panel(colorMask: new(50 , 170, 100), borderColor: Color.Gray, size: new Vector2(0, 200), usePercentWidth: true, widthPercent:1f, zIndex: 9, borderRadius: 10, childGap: 15f, rowGap: 6f),
            ]),

            new Panel(colorMask: new(80, 80, 80), borderColor: Color.Gray, size: new Vector2(0, 405), paddingSide: 10, usePercentWidth: true, widthPercent: .79f, paddingTop: 10, zIndex: 10, borderRadius: 10, layoutMode: PanelLayoutMode.Spread, childGap: 15f, rowGap: 6f),
        ]);
        return panel;
    }
}
