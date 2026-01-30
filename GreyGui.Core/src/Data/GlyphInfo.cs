using Microsoft.Xna.Framework;
namespace GreyGui;
public readonly record struct GlyphInfo
{
    public readonly Rectangle SrcRect { get; init; }
    public float AdvanceWidth { get; init; }
    public Vector2 Origin { get; init; }
    public float GlyphRange { get; init; }
}