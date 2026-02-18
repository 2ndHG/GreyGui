using SimpleSdf;
using Typography.OpenFont;
namespace GreyGui;

public class FontInfo
{
    public Typeface Typeface { get; private set; }
    public Dictionary<char, GlyphInfo> GlyphInfoMap { get; private set; } = [];
    public Dictionary<char, ushort> GlyphInfoIndexMap { get; private set; } = [];

    public FontInfo(string ttfPath)
    {
        using FileStream fileStream = File.OpenRead(ttfPath);
        {
            Typeface = SimpleSdf.SimpleSdf.GetTypeface(ttfPath) ?? throw new Exception($"Cannot load {ttfPath}.");
        }
    }
    public ushort GetCharIndex(char c) { return GlyphInfoIndexMap[c]; }
}