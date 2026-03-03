using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GreyGui.Demo;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private GreyGuiElement root;
    private Point _drawPos = new Point(50, 200);
    private RenderContext renderContext = new();
    private GuiBatch guiBatch;
    private Text rootText;
    private Button experimentingButton;
    private Texture2D _buttonTexture;

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
        _graphics.PreferredBackBufferWidth = 1300;
        _graphics.PreferredBackBufferHeight = 800;
        _graphics.ApplyChanges();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _buttonTexture = Content.Load<Texture2D>("SampleImage/ButtonSample");
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        GreyGui.Initialize(GraphicsDevice);
        GreyGui.TextSystem.LoadFont("huninn", "huninn.ttf");

        guiBatch = new GuiBatch(GraphicsDevice);

        root = GenerateButtonPanel();


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
            _drawPos.X -= 1;
            root.Size = root.Size with { X = root.Size.X + 2f };
        }
        else if (keyboardState.IsKeyDown(Keys.A))
        {
            _drawPos.X += 1;
            root.Size = root.Size with { X = root.Size.X - 2f };
        }
        if (keyboardState.IsKeyDown(Keys.W))
        {
            _drawPos.Y += 1;
            root.Size = root.Size with { Y = root.Size.Y - 2f };
        }
        else if (keyboardState.IsKeyDown(Keys.S))
        {
            _drawPos.Y -= 1;
            root.Size = root.Size with { Y = root.Size.Y + 2f };
        }

        if (oneTimeTicket && keyboardState.IsKeyDown(Keys.R))
        {
            // oneTimeTicket = false;
            // GreyGui.TextSystem.ReserveChars("huninn", "\" !#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~");
            rootText.FontSize = 48;
            rootText.FontSizeScalingMode = FontSizeScalingMode.None;
            rootText.DisplayText = "中文字測試，中文字測試，中文字測試，中文字測試，中文字測試，中文字測試。";
            // root = GenerateTextPanel();
        }

        GuiUpdate.StartFrame(Mouse.GetState(), Keyboard.GetState());
        GuiUpdate.Update(root);

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
        RowPanel panel = new RowPanel(colorMask: new(10, 10, 10), borderColor: Color.Gray, size: new Vector2(740, 435), paddingSide: 7, paddingTop: 7, paddingBottom: 7, borderRadius: 10, childGap: 10, layoutMode: RowLayoutMode.Justify).SetChildren([
            new ListPanel(colorMask: new(80, 80, 80), borderColor: Color.Gray, size: new Vector2(0, 405), useWidthRatio: true, widthRatio: .2f, useHeightRatio:true, heightRatio:1f, paddingTop: 3, paddingSide: 3,  zIndex: 10, borderRadius: 10,

            layoutMode: RowLayoutMode.Justify, childGap: 15f, rowGap: 6f).SetChildren([
                new RowPanel(colorMask: new(150 , 230, 255), borderColor: Color.Gray, size: new Vector2(0, 30), useWidthRatio: true, widthRatio:1f, zIndex: 10, borderRadius: 7, layoutMode: RowLayoutMode.Justify, childGap: 15f),

                new ListPanel(colorMask: new(40, 40, 40), useWidthRatio: true,size: new(0, 150), widthRatio:1f, borderRadius:7, childGap:3, rowGap:3, layoutMode: RowLayoutMode.Justify).SetChildren(PanelItemGen()),

                new RowPanel(colorMask: new(50 , 170, 100), borderColor: Color.Gray, size: new Vector2(0, 200), useWidthRatio: true, widthRatio:1f, zIndex: 9, borderRadius: 10, childGap: 15f),
            ]),

            new RowPanel(colorMask: new(80, 80, 80), borderColor: Color.Gray, size: new Vector2(0, 405), paddingSide: 10, useWidthRatio: true, widthRatio: .4f, useHeightRatio:true, heightRatio:1f,paddingTop: 10, zIndex: 10, borderRadius: 10, layoutMode: RowLayoutMode.Justify, childGap: 15f),
            new RowPanel(colorMask: Color.DarkGoldenrod, borderColor: Color.Gold, size: new Vector2(0, 405), paddingSide: 10, useWidthRatio: true, widthRatio: .4f, useHeightRatio:true, heightRatio:1f,paddingTop: 10, zIndex: 10, borderRadius: 10, layoutMode: RowLayoutMode.Justify, childGap: 15f),
        ]);
        return panel;
    }

    private GreyGuiElement GenerateTextPanel(RowLayoutMode alignMode)
    {
        rootText = new Text(colorMask: Color.PaleGoldenrod, size: new(800, 24), displayText: "aaaaa   ccccc aaaaa  ccccc  aaaaa   ccccc aaaaa ccccc aaaaa", useWidthRatio: true, widthRatio: 1f, alignMode: alignMode, textYOffset: -6, autoEndLine: true, useTextHeight: true, fontSize: 32);


        ListPanel rowPanel = new ListPanel(colorMask: Color.Black, size: new(1200, 400), layoutMode: RowLayoutMode.Left, paddingTop: 10, paddingSide: 10, borderRadius: 10, rowGap: 10).SetChildren([
            new ListPanel(Color.White, useWidthRatio: true, widthRatio: 1f, size: new(0, 24)),

            rootText,

            new Text(colorMask: new Color(107, 182, 232), size: new (200, 40), fontSize: 40, displayText: "This text section uses height as the font size scaling factor.", useWidthRatio: true, widthRatio: 1f, alignMode: alignMode, textYOffset: -6, fontSizeScalingMode: FontSizeScalingMode.UseHeightRatio, useHeightRatio: true, heightRatio: .05f),

            new Text(colorMask: new (180, 115, 250), size: new (200, 24), displayText: "This text section's font size doesn't scale with its width or height, so you can see auto-endline happening.", useWidthRatio: true, widthRatio: 1f, alignMode: alignMode, textYOffset: -6, autoEndLine: true, useTextHeight: true),

            new Text(colorMask: new (215, 252, 167), size: new (200, 24), displayText: "This section uses parent height to only scale height but not the font size.", useWidthRatio:true, widthRatio: 1f, alignMode: alignMode, textYOffset: -6, useHeightRatio: true, heightRatio: .15f),
            new Text(colorMask: new (167, 252, 245), displayText:"Final line with fixed rendering settings.",textYOffset: -6, size:new (1,1))
        ]);
        return rowPanel;
    }
    private GreyGuiElement GenerateText2()
    {
        return new Text(colorMask: Color.PaleGoldenrod, size: new(1000, 100));
    }

    private GreyGuiElement GenerateButtonPanel()
    {

        return new ListPanel(size: new(500, 135), borderRadius: 40, colorMask:Color.White, borderColor: Color.Black, borderWidth: 5).SetChildren([
            GenerateButton(),
        ]);
    }
    private Button GenerateButton()
    {
        float timer = 0f;
        Action<Button, Point, RenderContext, Rectangle> buttonDrawMethod = (button, position, renderContext, scissor) =>
        {
            button.OnScreenPos = position;
            timer = (button.State, timer) switch
            {
                (GreyGuiButtonState.Active, _) => -.3f,
                (GreyGuiButtonState.Hovered, < 1) => timer + .1f,
                (GreyGuiButtonState.Hovered, >= 1) => 1,
                (GreyGuiButtonState.Normal, > 0) => timer - .1f,
                (GreyGuiButtonState.Normal, <= 0) => 0,
                _ => 0
            };
            button.ZIndex = (button.State == GreyGuiButtonState.Normal) ? 0 : 1;

            Color c = button.ColorMask * (0.3f * timer + 1f);
            Vector2 minify = new(timer * -30, timer * -30);
            button.PaddingSide = (int)(timer * -15);
            button.PaddingVertical = (int)(timer * -7.5f);
            foreach (GreyGuiElement child in button.Children)
            {
                child.IsSizeDirty = true;
            }
            Vector2 size = button.Size - minify;
            size.Round();
            renderContext.RenderTexture(
                button.ImageTexture,
                new Rectangle(position + (minify / 2).ToPoint(), size.ToPoint()),
                button.ImageSrcRect,
                new Color(c, button.ColorMask.A),
                button.BorderColor,
                button.BorderRadius,
                button.BorderWidth,
                scissor
            );
        };
        Text buttonText = new Text( fontSize: 32, widthRatio: 1, useWidthRatio: true, displayText: "0", fontSizeScalingMode: FontSizeScalingMode.UseWidthRatio, size: new(230, 50), alignMode: RowLayoutMode.Center, useTextHeight: true);

        Button resultButton = new( useWidthRatio: true, widthRatio: .5f, useHeightWidthRatio: true, heightWidthRatio: .2f, borderColor: Color.Black, borderRadius:5, borderWidth:5);
        resultButton.DrawMethod = buttonDrawMethod;
        resultButton.AppendChild(buttonText);
        resultButton.OnLeftClicked += () =>
        {
            buttonText.DisplayText = (int.Parse(buttonText.DisplayText) + 1).ToString();
        };
        resultButton.OnRightClicked += () =>
        {
            buttonText.DisplayText = (int.Parse(buttonText.DisplayText) - 1).ToString();
        };

        return resultButton;
    }

    private GreyGuiElement GenerateImageButtonPanel()
    {
        return new ListPanel(colorMask: new(20, 20, 20), size: new(500, 135), borderRadius: 15, borderColor: Color.White, borderWidth: 0).SetChildren([
            // new Button(colorMask: Color.White, useWidthRatio: true, widthRatio: .5f, useHeightWidthRatio: true, heightWidthRatio: 1f, imageTexture: _buttonTexture, imageSrcRect: new(0, 0, 8, 8)),
            new Button(colorMask:Color.DarkGreen, useWidthRatio: true, widthRatio: .5f, useHeightWidthRatio: true, heightWidthRatio: 1f),
        ]);
    }
}
