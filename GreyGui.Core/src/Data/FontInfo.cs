using Typography;

namespace GreyGui.Core;

public class FontInfo
{
    public TypefaceWrapper TypefaceWrapper { get; private set; }
    public Dictionary<ushort, GlyphInfo> GlyphInfoMap { get; private set; } = [];

    public FontInfo(string ttfPath)
    {
        using FileStream fileStream = File.OpenRead(ttfPath);
        {
            TypefaceWrapper = TypefaceWrapper.Load(fileStream);
        }
    }
}