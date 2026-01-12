using Microsoft.Xna.Framework;
using GreyGui.Core;
using Msdfgen;
using Typography;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace GreyGui;

public class TextSystem
{
    public string DefaultFont { get; private set; } = "";
    public float GlyphPixelSize { get; private set; } = 48f;
    public int GlyphPadding { get; private set; } = 8;
    public float GlyphRange { get; private set; } = 8f;

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
        _fontInfoMap.TryAdd(fontName, new FontInfo(fontTtfPath));
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
    public void ReserveChars(string font, ReadOnlySpan<char> chars)
    {
        FontInfo fontInfo = _fontInfoMap[font];
        TypefaceWrapper typefaceWrapper = fontInfo.TypefaceWrapper;
        for (int i = 0; i < chars.Length; ++i)
        {
            char c = chars[i];
            if (!fontInfo.GlyphInfoMap.ContainsKey(c))
            {
                GlyphWrapper glyph = typefaceWrapper.GetGlyph(c);

                MsdfgenResult msdfgenResult = glyph.RenderMSDF(GlyphPixelSize, GlyphRange, GlyphPadding);
                FloatRGBBmp bmp = msdfgenResult.Bmp;
                if (bmp.Width == 0 || bmp.Height == 0)
                {
                    Console.WriteLine($"Warning: Glyph for character '{chars[i]}' has zero width or height, skipping insertion");
                }
                else
                {
                    if (TryInsertGlyph(bmp, out Rectangle glyphSrcRect))
                    {
                        fontInfo.GlyphInfoMap.TryAdd(c, new GlyphInfo()
                        {
                            SrcRect = glyphSrcRect,
                            AdvanceWidth = glyph.Glyph.OriginalAdvanceWidth,
                            Offset = new Vector2(
                                (float)msdfgenResult.Translation.x, 
                                (float)msdfgenResult.Translation.y + GlyphPixelSize - bmp.Height),
                            WidthHeightRatio = bmp.Width / (float)glyphSrcRect.Height,
                            GlyphRange = GlyphRange
                        });
                    }
                    else
                    {
                        throw new Exception("Font atlas is full, cannot insert more glyphs.");
                    }
                }
            }
        }
    }
    public bool TryInsertGlyph(FloatRGBBmp bitmap, out Rectangle srcRect)
    {
        if (_x + bitmap.Width > GreyGui.Atlas.Width)
        {
            _x = 0;
            _y += _currentHeight + 1;
            _currentHeight = 0;
        }
        if (_y + bitmap.Height > GreyGui.Atlas.Height)
        {
            srcRect = Rectangle.Empty;
            return false;
        }
        srcRect = new(_x, _y, bitmap.Width, bitmap.Height);

        Color[] data = new Color[bitmap.Width * bitmap.Height];
        for (int y = 0; y < bitmap.Height; y++)
        {
            int sourceY = bitmap.Height - 1 - y;

            for (int x = 0; x < bitmap.Width; x++)
            {
                int sourceIdx = x + (sourceY * bitmap.Width);
                int targetIdx = x + (y * bitmap.Width);

                FloatRGB pixel = bitmap._buffer[sourceIdx];

                data[targetIdx] = new Color(
                    MathHelper.Clamp(pixel.r, 0f, 1f),
                    MathHelper.Clamp(pixel.g, 0f, 1f),
                    MathHelper.Clamp(pixel.b, 0f, 1f),
                    1f
                );
            }
        }
        GreyGui.Atlas.SetData(0, srcRect, data, 0, data.Length);

        // Update _x and _currentHeight for the next insertion
        _x += bitmap.Width + 1;
        _currentHeight = Math.Max(bitmap.Height, _currentHeight);
        return true;
    }
}