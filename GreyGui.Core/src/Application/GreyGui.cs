using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GreyGui;

public static class GreyGui
{
    /// <summary>
    /// Initialize GreyGui. Need to be called before any usage of this library. 
    /// </summary>
    /// <remarks>
    /// You can specify the width and height of the backing atlas that used by all GreyGuiElement
    /// </remarks>
    /// <param name="game">The Running Game instance</param>
    /// <param name="atlasWidth">Width of the backing atlas</param>
    /// <param name="atlasHeight">Height of the backing atlas</param>
    /// <exception cref="Exception"></exception>
    public static void Initialize(Game game, int atlasWidth = 4096, int atlasHeight = 4096)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string[] resourceNames = assembly.GetManifestResourceNames();
        foreach (var name in resourceNames)
            Console.WriteLine(name);
        string resourcePath = "GreyGui.Core.resource.UI_SDF.mgfx";
        using Stream stream = assembly.GetManifestResourceStream(resourcePath);
        if (stream == null)
        {
            throw new Exception($"Cannot find {resourcePath}");
        }

        GameInstance = game;

        using MemoryStream ms = new();
        stream.CopyTo(ms);
        Shader = new Effect(game.GraphicsDevice, ms.ToArray());
        Shader.Parameters["antiAliasingFactor"].SetValue(6f); // hardcoded. But I found 6f is kind of good

        // Texture and TextSystem
        Atlas = new Texture2D(game.GraphicsDevice, atlasWidth, atlasHeight);
        Color[] defaultColor = new Color[atlasWidth * atlasHeight];
        Array.Fill(defaultColor, Color.Transparent);

        // make a (0, 0, 2, 2) white rectangle at the top-left corner of atlas
        defaultColor[0] = defaultColor[1] = defaultColor[atlasWidth + 0] = defaultColor[atlasWidth + 1] = Color.White;
        Atlas.SetData(defaultColor);
        AtlasPixelUv = new Vector2(0.5f / atlasWidth, 0.5f / atlasHeight);
        TextSystem = new TextSystem();

        // Initialize GuiUpdate
        GuiUpdate.Initialize(game);

        Rectangle gameViewportRect = game.GraphicsDevice.Viewport.Bounds;
        NullParentWidth = gameViewportRect.Width;
        NullParentHeight = gameViewportRect.Height;
    }
    
    /// <summary>
    /// Set the size of the virtual parent, i.e., <see cref="NullParentWidth"/> and <see cref="NullParentHeight"/>.
    /// </summary>
    /// <remarks>
    /// In GreyGui, a lot of elements can scale itself according to their parent's size, they will reference to <see cref="NullParentWidth"/> and <see cref="NullParentHeight"/> when parent is null.
    /// </remarks>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public static void SetVirtualParentSize(int width, int height)
    {
        NullParentWidth = width;
        NullParentHeight = height;
    }

    public static Texture2D Atlas { get; private set; }
    public static Effect Shader { get; private set; }
    public static TextSystem TextSystem { get; private set; }
    public static Vector2 AtlasPixelUv { get; private set; }
    public static Game GameInstance { get; private set; }
    public static int NullParentWidth { get; private set; }
    public static int NullParentHeight { get; private set; }
}