using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GreyGui;

public static class GreyGui
{
    public static void Initialize(GraphicsDevice device, int atlasWidth = 2048, int atlasHeight = 2048)
    {
        Atlas = new Texture2D(device, atlasWidth, atlasHeight);
        Color[] defaultColor = new Color[atlasWidth * atlasHeight];
        Array.Fill(defaultColor, Color.Black);
        defaultColor[0] = Color.White;
        Atlas.SetData(defaultColor);
        PixelUV = new Vector2(0.5f / atlasWidth, 0.5f / atlasHeight);

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

        using MemoryStream ms = new();
        stream.CopyTo(ms);
        Shader = new Effect(device, ms.ToArray());

        TextSystem = new TextSystem(device);
    }

    public static Texture2D Atlas { get; private set; }
    public static Effect Shader { get; private set; }
    public static TextSystem TextSystem { get; private set; }
    public static Vector2 PixelUV { get; private set; }
}