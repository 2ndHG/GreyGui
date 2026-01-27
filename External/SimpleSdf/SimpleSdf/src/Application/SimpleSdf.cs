using Typography.OpenFont;
using System.Numerics;

namespace SimpleSdf;

public class SimpleSdf
{
    public static Typeface? GetTypeface(string fontPath)
    {
        OpenFontReader fontReader = new();
        using FileStream file = File.OpenRead(fontPath);
        Typeface? typeface = fontReader.Read(file);
        return typeface;

    }

    public static SimpleSdfResult GenerateSdfBitmap(Typeface typeface, char c, float fontSize, int padding, float range)
    {
        int unitsPerEm = typeface.UnitsPerEm;
        ushort glyphIndex = typeface.GetGlyphIndex(c);
        Glyph glyph = typeface.GetGlyph(glyphIndex);

        Bounds bounds = glyph.Bounds;

        float scale = fontSize / unitsPerEm;

        float contentW = (bounds.XMax - bounds.XMin) * scale;
        float contentH = (bounds.YMax - bounds.YMin) * scale;

        int bitmapWidth = (int)Math.Ceiling(contentW) + padding * 2;
        int bitmapHeight = (int)Math.Ceiling(contentH) + padding * 2;

        float translateX = -bounds.XMin * scale + padding;
        float translateY = bounds.YMax * scale + padding;

        float originX = translateX;
        float originY = bitmapHeight - (-bounds.YMin * scale + padding);

        // I thought originalAdvanceWidth shall be at glyph.OriginalAdvanceWidth
        // However, it seems we need to use some GlyphBitmapDataFormatBase class and call their FillGlyphInfo to fill info into the glyph object
        float advanceWidth = typeface.GetHAdvanceWidthFromGlyphIndex(glyphIndex) * scale;

        Shape shape = new(glyph);
        float[] sdfData = SdfGenerator.Generate(shape,
            bitmapWidth,
            bitmapHeight,
            scale,
            translateX,
            translateY,
            range
        );

        return new SimpleSdfResult() { Bitmap = sdfData, BitmapWidth = bitmapWidth, BitmapHeight = bitmapHeight, FontSize = fontSize, Origin = new Vector2(originX, originY), Range = range, AdvanceWidth = advanceWidth };
    }

}