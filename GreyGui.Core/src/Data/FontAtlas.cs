using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Msdfgen;

namespace GreyGui.Core;

public class FontAtlas
{
    public int GlyphPadding
    {
        get => _glyphPadding;
        set => _glyphPadding = value;
    }
    public Texture2D Texture => _texture;
    private Texture2D _texture;
    private int _x = 0;
    private int _y = 0;
    private int _currentHeight;
    private int _glyphPadding;
    public FontAtlas(GraphicsDevice graphicsDevice, int textureWidth, int textureHeight)
    {
        _texture = new Texture2D(graphicsDevice, textureWidth, textureHeight);
        Color[] defaultColor = new Color[textureWidth * textureHeight];
        Array.Fill(defaultColor, Color.Black);
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
        _texture.SetData(0, srcRect, data, 0, data.Length);

        // Update _x and _currentHeight for the next insertion
        _x += bitmap.Width;
        _currentHeight = Math.Max(bitmap.Height, _currentHeight);
        return true;
    }
}