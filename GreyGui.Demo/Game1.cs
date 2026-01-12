using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GreyGui.Demo;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private GreyGuiElement root;
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
        _graphics.PreferredBackBufferWidth = 1024;
        _graphics.PreferredBackBufferHeight = 1024;
        _graphics.ApplyChanges();


        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        GreyGui.Initialize(GraphicsDevice, 1024, 1024);

        guiBatch = new GuiBatch(GraphicsDevice);

        root = new RowPanel(Color.White, size: new(32, 32));
        GreyGui.TextSystem.LoadFont("Huninn", "Content/NotoSansTC-Regular.ttf");
        GreyGui.TextSystem.ReserveChars("Huninn", "」的尷尬情況（到底算上一個邊還是下一個邊？）。稍微偏移一點點，就能保證射線乾淨俐落地穿過邊線，而不是擦過頂點。".AsSpan());

        // TODO: use this.Content to load your game content here
    }

    bool exported = false;
    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here
        var keyboardState = Keyboard.GetState();
        if (keyboardState.IsKeyDown(Keys.D))
        {
            root.Size = root.Size with { X = root.Size.X + 3f };
        }
        else if (keyboardState.IsKeyDown(Keys.A))
        {
            root.Size = root.Size with { X = root.Size.X - 3f };
        }
        if (keyboardState.IsKeyDown(Keys.W))
        {
            root.Size = root.Size with { Y = root.Size.Y - 3f };
        }
        else if (keyboardState.IsKeyDown(Keys.S))
        {
            root.Size = root.Size with { Y = root.Size.Y + 3f };
        }

        if (keyboardState.IsKeyDown(Keys.R) && !exported)
        {
            root = GeneratePanelWireFrame();
            // string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output.png");
            // using (FileStream fileStream = File.OpenWrite(outputPath))
            // {
            //     GreyGui.Atlas.SaveAsPng(fileStream, GreyGui.Atlas.Width, GreyGui.Atlas.Height);
            // }
            // Console.WriteLine("Export Atlas Test Succeed");
            // exported = true;
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        if (!IsActive)
            return;
        GraphicsDevice.Clear(new Color(50, 50, 50));
        _spriteBatch.Begin();
        _spriteBatch.Draw(GreyGui.Atlas, new Rectangle(0, 0, 1024, 1024), Color.White);
        _spriteBatch.End();
        // Point point = Mouse.GetState().Position;
        renderContext.RenderText(new(0, 500), "就能保證射線乾淨俐落地穿過邊線，而不是擦過頂點", 160);
        guiBatch.Flush(renderContext);

        // Measure draw calls
        // var metrics = GraphicsDevice.Metrics;
        // Console.WriteLine(metrics.DrawCount);

        base.Draw(gameTime);
    }

    private ListPanel GeneratePanel1()
    {
        static ListPanel[] PanelGen()
        {
            ListPanel[] panels = new ListPanel[10];
            for (int i = 0; i < 10; ++i)
            {
                panels[i] = new ListPanel(colorMask: new(0 + i * 20, 153 + i * 10, 204 + i * 5), size: new(120 - i * 3, 30), paddingSide: 5, zIndex: 10, borderRadius: 10);
            }
            return panels;
        }
        ListPanel panel = new ListPanel(colorMask: new(10, 10, 10), borderColor: Color.Gray, size: new Vector2(150, 400), paddingSide: 10, paddingTop: 10, zIndex: 10, borderRadius: 10, layoutMode: PanelLayoutMode.Right, childGap: 15f, rowGap: 6f).SetChildren(
            PanelGen()
        );
        return panel;
    }
    private RowPanel GeneratePanelWireFrame()
    {
        static ListPanel[] PanelItemGen()
        {
            ListPanel[] panels = new ListPanel[10];
            for (int i = 0; i < 10; ++i)
            {
                panels[i] = new ListPanel(colorMask: new(20 + i * 10, 100 + i * 5, 100 + i * 3), size: new(25, 25), paddingSide: 5, borderRadius: 7);
            }
            return panels;
        }
        RowPanel panel = new RowPanel(colorMask: new(10, 10, 10), borderColor: Color.Gray, size: new Vector2(740, 435), paddingSide: 7, paddingTop: 7, paddingBottom: 7, borderRadius: 10, childGap: 10, layoutMode: PanelLayoutMode.Spread);
        return panel;
    }
}
