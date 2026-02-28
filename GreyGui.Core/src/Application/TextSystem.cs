using System.Buffers;
using System.Collections.Concurrent;
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
    public float DefaultFontSize { get; set; } = 24;
    public int MaxReservingCharCount { get; set; } = 2048;

    private Dictionary<string, FontInfo> _fontInfoMap = [];
    public List<GlyphInfo> GlyphInfoList {get; private init; }= [];

    private int _x = 2; // this is for default gui texture that has 2*2 white pixels
    private int _y = 0;
    private int _currentHeight = 2;

    // Multi-thread SDF bitmap generation
    private ConcurrentQueue<(Rectangle destRect, Typeface typeface, char c)> _reservedChars = [];
    private ConcurrentQueue<(Rectangle destRect, float[] bitmap)> _sdfResultProductBuffer = [];
    private volatile bool _isGeneratingSdf = false;
    private readonly object syncRoot = new();
    private ArrayPool<Color> _colorDataArrayPool = ArrayPool<Color>.Shared;


    public void SetTextParameters(float glyphPixelSize, int glyphPadding, float glyphRange)
    {
        GlyphPixelSize = glyphPixelSize;
        GlyphPadding = glyphPadding;
        GlyphRange = glyphRange;
    }
    public void LoadFont(string fontName, string fontTtfPath)
    {
        if (_fontInfoMap.TryAdd(fontName, new FontInfo(fontTtfPath)))
        {
            ReserveCharsSync(fontName, [' ']);
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
        bool reservedAny = false;
        foreach (char c in chars)
        {
            if (_reservedChars.Count < MaxReservingCharCount && !fontInfo.GlyphInfoIndexMap.ContainsKey(c))
            {
                SdfRenderInfo sdfRenderInfo = SimpleSdf.SimpleSdf.GetSdfRenderInfo(typeface, c, GlyphPixelSize, GlyphPadding, GlyphRange);

                // Because the actual bitmap will be generated asynchronously later, TryPutGlyphPlaceholder reserves the demanding space on the GreyGui.Atlas. 
                if (TryPutGlyphPlaceholder(sdfRenderInfo, out Rectangle glyphSrcRect))
                {
                    GlyphInfo glyphInfo = new()
                    {
                        SrcRect = glyphSrcRect,
                        AdvanceWidth = sdfRenderInfo.AdvanceWidth,
                        Origin = sdfRenderInfo.Origin,
                        GlyphRange = sdfRenderInfo.Range
                    };

                    // record the index
                    fontInfo.GlyphInfoIndexMap[c] = (ushort)GlyphInfoList.Count;
                    GlyphInfoList.Add(glyphInfo);

                    // add c to reservation queue, then the SDF bitmap will be generated in a separated thread
                    _reservedChars.Enqueue((glyphSrcRect, typeface, c));
                    reservedAny = true;
                }
            }
        }

        if (reservedAny && !_isGeneratingSdf)
        {
            lock (syncRoot)
            {
                if (!_isGeneratingSdf)
                {
                    StartGenerateSdfResultInSideThread();
                }
            }
        }
    }


    public bool TryPutGlyphPlaceholder(SdfRenderInfo sdfRenderInfo, out Rectangle srcRect)
    {
        if (_x + sdfRenderInfo.BitmapWidth > GreyGui.Atlas.Width)
        {
            _x = 0;
            _y += _currentHeight + 1;
            _currentHeight = 0;
        }
        if (_y + sdfRenderInfo.BitmapHeight > GreyGui.Atlas.Height)
        {
            srcRect = Rectangle.Empty;
            return false;
        }
        srcRect = new(_x, _y, sdfRenderInfo.BitmapWidth, sdfRenderInfo.BitmapHeight);
        // Update _x and _currentHeight for the next insertion
        _x += sdfRenderInfo.BitmapWidth + 1;
        _currentHeight = Math.Max(sdfRenderInfo.BitmapHeight, _currentHeight);
        return true;
    }


    public void SetGeneratedSdfBitmapToAtlas()
    {
        while (_sdfResultProductBuffer.TryDequeue(out (Rectangle destRect, float[] bitmap) result))
        {
            Rectangle destRect = result.destRect;
            Color[] data = _colorDataArrayPool.Rent(destRect.Width * destRect.Height);
            for (int y = 0; y < destRect.Height; y++)
            {
                for (int x = 0; x < destRect.Width; x++)
                {
                    int index = x + (y * destRect.Width);

                    byte pixel = (byte)(result.bitmap[index] * 255);

                    data[index] = new Color(pixel, pixel, pixel, pixel);
                }
            }
            GreyGui.Atlas.SetData(0, result.destRect, data, 0, destRect.Width * destRect.Height);
            _colorDataArrayPool.Return(data);
        }
    }

    /// <summary>
    /// Using this emthod to reserve character will freeze the game until all reserving characters are generated.
    /// </summary>
    /// <param name="fontName">The font name of characters</param>
    /// <param name="chars">Reserving characters</param>
    /// <exception cref="Exception"></exception>
    public void ReserveCharsSync(string fontName, ReadOnlySpan<char> chars)
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

    private void StartGenerateSdfResultInSideThread()
    {
        _isGeneratingSdf = true;
        Task.Run(() =>
        {
            try
            {
                // Console.WriteLine("Start generate Sdf");
                while (_isGeneratingSdf)
                {
                    // Console.WriteLine($"Remaining bitmap to generate: {_reservedChars.Count}");
                    if (_reservedChars.TryDequeue(out (Rectangle destRect, Typeface typeface, char c) request))
                    {
                        SimpleSdfResult result = SimpleSdf.SimpleSdf.GenerateSdfBitmap(request.typeface, request.c, GlyphPixelSize, GlyphPadding, GlyphPadding);
                        _sdfResultProductBuffer.Enqueue((request.destRect, result.Bitmap));

                        Thread.Sleep(1);
                    }
                    else
                    {
                        lock (syncRoot)
                        {
                            if (_reservedChars.IsEmpty)
                            {
                                _isGeneratingSdf = false;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occurs when generating sdf bitmap: {e.Message}");
                Console.WriteLine("Sdf bitmap generation ends with an exception.");
                lock (syncRoot) { _isGeneratingSdf = false; }
            }

        });
    }
}