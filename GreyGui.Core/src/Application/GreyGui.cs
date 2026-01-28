using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GreyGui;

public static class GreyGui
{
    public static void Initialize(GraphicsDevice device, int atlasWidth = 2048, int atlasHeight = 2048)
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

        using MemoryStream ms = new();
        stream.CopyTo(ms);
        Shader = new Effect(device, ms.ToArray());
        Shader.Parameters["antiAliasingRange"].SetValue(0.15f);

        // Texture and TextSystem
        Atlas = new Texture2D(device, atlasWidth, atlasHeight);
        Color[] defaultColor = new Color[atlasWidth * atlasHeight];
        Array.Fill(defaultColor, Color.Transparent);
        
        defaultColor[0] = Color.White;
        Atlas.SetData(defaultColor);
        AtlasPixelUv = new Vector2(0.5f / atlasWidth, 0.5f / atlasHeight);
        TextSystem = new TextSystem();
    }

    public static Texture2D Atlas { get; private set; }
    public static Effect Shader { get; private set; }
    public static TextSystem TextSystem { get; private set; }
    public static Vector2 AtlasPixelUv { get; private set; }
}