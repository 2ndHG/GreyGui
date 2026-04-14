namespace GreyGui;


public struct AtlasInfo
{
    public Dictionary<string, FontInfo> FontInfoMap { get; set; }
    public List<GlyphInfo> GlyphInfoList { get; set; }
    public int NextGlyphX { get; set; }
    public int NextGlyphY { get; set; }
    public int CurrentRowHeight { get; set; }
}