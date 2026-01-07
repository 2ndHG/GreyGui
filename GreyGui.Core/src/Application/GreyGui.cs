using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GreyGui;

public static class GreyGui
{
    public static void Initialize(GraphicsDevice device, int textAtlasWidth = 2048, int textAtlasHeight = 2048)
    {
        Pixel = new Texture2D(device, 1, 1);
        Pixel.SetData([Color.White]);

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

        TextSystem = new TextSystem(device, textAtlasWidth, textAtlasHeight);
    }

    public static Texture2D Pixel { get; private set; }
    public static Effect Shader { get; private set; }
    public static TextSystem TextSystem { get; private set; }
    public static string DefaultFont { get; private set; }
}