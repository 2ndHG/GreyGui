using Microsoft.Xna.Framework;
public record struct GlyphInfo
{
    public Rectangle SrcRect { get; set; }
    public float AdvanceWidth { get; set; }
    public Vector2 Offset { get; set; }
    public float WidthHeightRatio { get; set; }
    public float GlyphRange { get; set; }
}