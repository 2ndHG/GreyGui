using Typography.OpenFont;

namespace SimpleSdf;

public static class Extensions
{
    static (float X, float Y) ToBitmapSpace(this GlyphPointF p, float scale)
    {
        return (p.X * scale, p.Y * scale);
    }
}