using Microsoft.Xna.Framework;
using GreyGui.Core;
using Microsoft.Xna.Framework.Graphics;
using Msdfgen;
using Typography;
using Typography.OpenFont;

namespace GreyGui;

public class TextSystem
{
    public float GlyphPixelSize { get; private set; } = 32f;
    public int GlyphPadding { get; private set; } = 4;
    public float GlyphRange { get; private set; } = 2f;
    public FontAtlas FontAtlas => _fontAtlas;

    private GraphicsDevice _graphicDevice;
    private Dictionary<string, FontInfo> _fontInfoMap = [];
    private FontAtlas _fontAtlas;

    public TextSystem(GraphicsDevice graphicsDevice, int textAtlasWidth, int textAtlasHeight)
    {
        _graphicDevice = graphicsDevice;
        _fontAtlas = new FontAtlas(_graphicDevice, textAtlasWidth, textAtlasHeight)
        {
            GlyphPadding = GlyphPadding
        };
    }

    public void SetTextParameters(float glyphPixelSize, int glyphPadding, float glyphRange)
    {
        GlyphPixelSize = glyphPixelSize;
        GlyphPadding = glyphPadding;
        GlyphRange = glyphRange;
        _fontAtlas.GlyphPadding = GlyphPadding;
    }
    public void LoadFont(string fontName, string fontTtfPath)
    {
        _fontInfoMap.TryAdd(fontName, new FontInfo(fontTtfPath));
    }
    public void ReserveChars(string font, ReadOnlySpan<char> chars)
    {
        FontInfo fontInfo = _fontInfoMap[font];
        TypefaceWrapper typefaceWrapper = fontInfo.TypefaceWrapper;
        for (int i = 0; i < chars.Length; ++i)
        {
            ushort c = chars[i];
            GlyphWrapper glyph = typefaceWrapper.GetGlyph(c);

            MsdfgenResult msdfgenResult = glyph.RenderMSDF(GlyphPixelSize, GlyphRange, GlyphPadding);
            FloatRGBBmp bmp = msdfgenResult.Bmp;
            if (bmp.Width == 0 || bmp.Height == 0)
            {
                Console.WriteLine($"Warning: Glyph for character '{chars[i]}' has zero width or height, skipping insertion into font atlas.");
            }
            else
            {
                if (_fontAtlas.TryInsertGlyph(bmp, out Rectangle glyphSrcRect))
                {
                    fontInfo.GlyphInfoMap.TryAdd(c, new GlyphInfo() { srcRect = glyphSrcRect });
                }
                else
                {
                    throw new Exception("Font atlas is full, cannot insert more glyphs.");
                }
            }
        }
    }
}