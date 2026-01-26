using Microsoft.Xna.Framework;
namespace GreyGui;
public record struct GlyphInfo
{
    public Rectangle SrcRect { get; set; }
    public float AdvanceWidth { get; set; }
    public Vector2 Origin { get; set; }
    public float GlyphRange { get; set; }
}