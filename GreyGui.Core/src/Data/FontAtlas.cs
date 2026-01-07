using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Msdfgen;

namespace GreyGui.Core;

public class FontAtlas
{
    public int GlyphPadding
    {
        get => _glyphPadding;
        set
        {
            _x += _glyphPadding - value;
            _glyphPadding = value;
        }
    }
    private Texture2D _texture;
    private int _x = 0;
    private int _y = 0;
    private int _currentHeight;
    private int _glyphPadding;
    public FontAtlas(GraphicsDevice graphicsDevice, int textureWidth, int textureHeight)
    {
        _texture = new Texture2D(graphicsDevice, textureWidth, textureHeight);
        Color[] defaultColor = new Color[textureWidth * textureHeight];
        _texture.SetData(defaultColor);
    }
    public bool TryInsertGlyph(FloatRGBBmp bitmap, out Rectangle srcRect)
    {
        if (_x + bitmap.Width > _texture.Width)
        {
            _x = 0;
            _y += _currentHeight;
            _currentHeight = 0;
        }
        if (_y + bitmap.Height > _texture.Height)
        {
            srcRect = Rectangle.Empty;
            return false;
        }
        srcRect = new(_x, _y, bitmap.Width, bitmap.Height);

        Color[] data = new Color[bitmap.Width * bitmap.Height];
        for (int i = 0; i < bitmap._buffer.Length; i++)
        {
            FloatRGB pixel = bitmap._buffer[i];
            data[i] = new Color(
                MathHelper.Clamp(pixel.r, 0f, 1f),
                MathHelper.Clamp(pixel.g, 0f, 1f),
                MathHelper.Clamp(pixel.b, 0f, 1f),
                1f
            );
        }
        _texture.SetData(0, srcRect, data, 0, data.Length);

        // Update _x and _currentHeight for the next insertion
        _x += bitmap.Width + GlyphPadding;
        _currentHeight = Math.Max(bitmap.Height, _currentHeight);
        return true;
    }
}