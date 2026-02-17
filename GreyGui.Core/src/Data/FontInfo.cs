using SimpleSdf;
using Typography.OpenFont;
namespace GreyGui;

public class FontInfo
{
    public Typeface Typeface { get; private set; }
    public Dictionary<char, GlyphInfo> GlyphInfoMap { get; private set; } = [];
    public float FontSizeOneSpaceWidth { get; set; }
    public Dictionary<char, int> GlyphInfoIndexMap { get; private set; } = [];

    public FontInfo(string ttfPath)
    {
        using FileStream fileStream = File.OpenRead(ttfPath);
        {
            Typeface = SimpleSdf.SimpleSdf.GetTypeface(ttfPath) ?? throw new Exception($"Cannot load {ttfPath}.");
            ushort spaceGlyphIndex = Typeface.GetGlyphIndex(' ');
            FontSizeOneSpaceWidth = Typeface.GetHAdvanceWidthFromGlyphIndex(spaceGlyphIndex);
        }
    }
    public int GetCharIndex(char c) { return GlyphInfoIndexMap[c]; }
}