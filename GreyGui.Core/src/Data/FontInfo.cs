using SimpleSdf;
using Typography.OpenFont;
namespace GreyGui;

public class FontInfo
{
    public Typeface Typeface { get; private set; }
    public Dictionary<char, SimpleSdfResult> GlyphInfoMap { get; private set; } = [];

    public FontInfo(string ttfPath)
    {
        using FileStream fileStream = File.OpenRead(ttfPath);
        {
            Typeface = SimpleSdf.SimpleSdf.TryGetTypeface(ttfPath) ?? throw new Exception($"Cannot load {ttfPath}.");
        }
    }
}