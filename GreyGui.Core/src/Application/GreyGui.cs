using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GreyGui;

public static class GreyGui
{
    public static void Initialize(GraphicsDevice device)
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
    }

    public static Texture2D Pixel { get; private set; }
    public static Effect Shader { get; private set; }
}