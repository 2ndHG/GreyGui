using Microsoft.Xna.Framework;
using SimpleSdf;
using Typography.OpenFont;

namespace GreyGui;

public class TextSystem
{
    public string DefaultFont { get; private set; } = "";
    public float GlyphPixelSize { get; private set; } = 40;
    public int GlyphPadding { get; private set; } = 4;
    public float GlyphRange { get; private set; } = 4f;
    public float DefaultFontSize {get; set;} = 24;

    private Dictionary<string, FontInfo> _fontInfoMap = [];

    private int _x = 1;
    private int _y = 0;
    private int _currentHeight = 1;


    public void SetTextParameters(float glyphPixelSize, int glyphPadding, float glyphRange)
    {
        GlyphPixelSize = glyphPixelSize;
        GlyphPadding = glyphPadding;
        GlyphRange = glyphRange;
    }
    public void LoadFont(string fontName, string fontTtfPath)
    {
        if(_fontInfoMap.TryAdd(fontName, new FontInfo(fontTtfPath)))
        {
            ReserveChars(fontName, [' ']);
        }
        if (DefaultFont.Length == 0)
        {
            DefaultFont = fontName;
        }
    }
    public void SetDefaultFont(string fontName)
    {
        if (_fontInfoMap.ContainsKey(fontName))
        {
            DefaultFont = fontName;
        }
        else
        {
            throw new Exception($"No font with font name {fontName} is loaded");
        }
    }
    public FontInfo GetFontInfo(string fontName)
    {
        return _fontInfoMap[fontName];
    }
    public void ReserveChars(string fontName, ReadOnlySpan<char> chars)
    {
        FontInfo fontInfo = _fontInfoMap[fontName];
        Typeface typeface = fontInfo.Typeface;
        for (int i = 0; i < chars.Length; ++i)
        {
            char c = chars[i];
            SimpleSdfResult result = SimpleSdf.SimpleSdf.GenerateSdfBitmap(typeface, c, GlyphPixelSize, GlyphPadding, GlyphPadding);

            if (!fontInfo.GlyphInfoMap.ContainsKey(c))
            {
                if (result.BitmapWidth == 0 || result.BitmapHeight == 0)
                {
                    Console.WriteLine($"Warning: Glyph for character '{chars[i]}' has zero width or height, skipping insertion");
                }
                else
                {
                    if (TryInsertGlyph(result, out Rectangle glyphSrcRect))
                    {
                        GlyphInfo glyphInfo = new()
                        {
                            SrcRect = glyphSrcRect,
                            AdvanceWidth = result.AdvanceWidth,
                            Origin = result.Origin,
                            GlyphRange = result.Range
                        };
                        fontInfo.GlyphInfoMap.TryAdd(c, glyphInfo);
                    }
                    else
                    {
                        throw new Exception("Font atlas is full, cannot insert more glyphs.");
                    }
                }
            }
        }
    }
    public bool TryInsertGlyph(SimpleSdfResult sdfResult, out Rectangle srcRect)
    {
        if (_x + sdfResult.BitmapWidth > GreyGui.Atlas.Width)
        {
            _x = 0;
            _y += _currentHeight + 1;
            _currentHeight = 0;
        }
        if (_y + sdfResult.BitmapHeight > GreyGui.Atlas.Height)
        {
            srcRect = Rectangle.Empty;
            return false;
        }
        srcRect = new(_x, _y, sdfResult.BitmapWidth, sdfResult.BitmapHeight);

        Color[] data = new Color[sdfResult.BitmapWidth * sdfResult.BitmapHeight];
        for (int y = 0; y < sdfResult.BitmapHeight; y++)
        {
            for (int x = 0; x < sdfResult.BitmapWidth; x++)
            {
                int index = x + (y * sdfResult.BitmapWidth);

                byte pixel = (byte)(sdfResult.Bitmap[index] * 255);

                data[index] = new Color(pixel, pixel, pixel, pixel);
            }
        }
        GreyGui.Atlas.SetData(0, srcRect, data, 0, data.Length);

        // Update _x and _currentHeight for the next insertion
        _x += sdfResult.BitmapWidth + 1;
        _currentHeight = Math.Max(sdfResult.BitmapHeight, _currentHeight);
        return true;
    }
    public void SetAntiAliasingRange(float value)
    {
        GreyGui.Shader.Parameters["antiAliasingRange"].SetValue(value);
    }
}