using SimpleSdf;
using Typography.OpenFont;
namespace GreyGui;

public class FontInfo
{
    public Typeface Typeface { get; private set; }
    public Dictionary<char, GlyphInfo> GlyphInfoMap { get; private set; } = [];
    float FontSizeOneSpaceWidth;

    public FontInfo(string ttfPath)
    {
        using FileStream fileStream = File.OpenRead(ttfPath);
        {
            Typeface = SimpleSdf.SimpleSdf.GetTypeface(ttfPath) ?? throw new Exception($"Cannot load {ttfPath}.");
            ushort spaceGlyphIndex =Typeface.GetGlyphIndex(' ');
            FontSizeOneSpaceWidth = Typeface.GetHAdvanceWidthFromGlyphIndex(spaceGlyphIndex);
            
        }
    }
}