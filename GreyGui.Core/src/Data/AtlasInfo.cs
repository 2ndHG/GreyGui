namespace GreyGui;


public struct AtlasInfo
{
    /// <summary>
    /// Key 1: font name; Key 2: char; value index to GlyphInfoList 
    /// </summary>
    public Dictionary<string, Dictionary<char, ushort> > FontInfoMap { get; set; }
    public List<GlyphInfo> GlyphInfoList { get; set; }
    public int NextGlyphX { get; set; }
    public int NextGlyphY { get; set; }
    public int CurrentRowHeight { get; set; }
}