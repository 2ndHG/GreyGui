using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GreyGui.Demo;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private GreyGuiElement root;
    private Point _drawPos = new Point(550, 300);
    private RenderContext renderContext = new();
    private GuiBatch guiBatch;

    private bool oneTimeTicket = true;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        _graphics.PreferredBackBufferWidth = 1400;
        _graphics.PreferredBackBufferHeight = 1000;
        _graphics.ApplyChanges();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        GreyGui.Initialize(GraphicsDevice);
        GreyGui.TextSystem.LoadFont("huninn", "huninn.ttf");

        guiBatch = new GuiBatch(GraphicsDevice);

        root = GenerateTextPanel();

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

        if (oneTimeTicket && keyboardState.IsKeyDown(Keys.R))
        {
            oneTimeTicket = false;
            GreyGui.TextSystem.ReserveChars("huninn", "\" !#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~");
            // root = GenerateTextPanel();
        }

        base.Update(gameTime);
    }
    protected override void Draw(GameTime gameTime)
    {
        if (!IsActive)
            return;
        GraphicsDevice.Clear(new Color(50, 50, 50));
        // Point point = Mouse.GetState().Position;
        _spriteBatch.Begin();
        _spriteBatch.Draw(GreyGui.Atlas, new Rectangle(0, 0, 1024, 1024), Color.White);
        _spriteBatch.End();

        guiBatch.Draw(root, renderContext, _drawPos);
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
        ListPanel panel = new ListPanel(colorMask: new(10, 10, 10), borderColor: Color.Gray, size: new Vector2(150, 400), paddingSide: 10, paddingTop: 10, zIndex: 10, borderRadius: 10, layoutMode: RowLayoutMode.Right, childGap: 15f, rowGap: 6f).SetChildren(
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
        RowPanel panel = new RowPanel(colorMask: new(10, 10, 10), borderColor: Color.Gray, size: new Vector2(740, 435), paddingSide: 7, paddingTop: 7, paddingBottom: 7, borderRadius: 10, childGap: 10, layoutMode: RowLayoutMode.Spread).SetChildren([
            new ListPanel(colorMask: new(80, 80, 80), borderColor: Color.Gray, size: new Vector2(0, 405), useWidthRatio: true, widthRatio: .2f, useHeightRatio:true, heightRatio:1f, paddingTop: 3, paddingSide: 3,  zIndex: 10, borderRadius: 10,

            layoutMode: RowLayoutMode.Spread, childGap: 15f, rowGap: 6f).SetChildren([
                new RowPanel(colorMask: new(150 , 230, 255), borderColor: Color.Gray, size: new Vector2(0, 30), useWidthRatio: true, widthRatio:1f, zIndex: 10, borderRadius: 7, layoutMode: RowLayoutMode.Spread, childGap: 15f),

                new ListPanel(colorMask: new(40, 40, 40), useWidthRatio: true,size: new(0, 150), widthRatio:1f, borderRadius:7, childGap:3, rowGap:3, layoutMode: RowLayoutMode.Spread).SetChildren(PanelItemGen()),

                new RowPanel(colorMask: new(50 , 170, 100), borderColor: Color.Gray, size: new Vector2(0, 200), useWidthRatio: true, widthRatio:1f, zIndex: 9, borderRadius: 10, childGap: 15f),
            ]),

            new RowPanel(colorMask: new(80, 80, 80), borderColor: Color.Gray, size: new Vector2(0, 405), paddingSide: 10, useWidthRatio: true, widthRatio: .4f, useHeightRatio:true, heightRatio:1f,paddingTop: 10, zIndex: 10, borderRadius: 10, layoutMode: RowLayoutMode.Spread, childGap: 15f),
            new RowPanel(colorMask: Color.DarkGoldenrod, borderColor: Color.Gold, size: new Vector2(0, 405), paddingSide: 10, useWidthRatio: true, widthRatio: .4f, useHeightRatio:true, heightRatio:1f,paddingTop: 10, zIndex: 10, borderRadius: 10, layoutMode: RowLayoutMode.Spread, childGap: 15f),
        ]);
        return panel;
    }

    private GreyGuiElement GenerateTextPanel()
    {
        ListPanel rowPanel = new ListPanel(colorMask: Color.Transparent, size: new(200, 150), layoutMode: RowLayoutMode.Left).SetChildren([
            new ListPanel(Color.Khaki, useWidthRatio: true, widthRatio: 1f, size: new(0, 24)),
            new Text(colorMask: Color.PaleGoldenrod, size: new (200, 24), displayText: "a bb ccc  DDDD  eeeee ffffff ggggggg", useWidthRatio:true, widthRatio: 1f, alignMode: RowLayoutMode.Right, textYOffset: -6),
            // new Text(colorMask: Color.PaleGoldenrod, size: new (300, 24), displayText: "SomeText", useWidthRatio:true, widthRatio: .5f, alignMode: RowLayoutMode.Left, textYOffset: -8),
        ]);
        return rowPanel;
    }
}
