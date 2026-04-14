using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
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
    public int FontInfoVersion
    {
        get => _fontInfoVersion;
        private set
        {
            _fontInfoVersion = value;
        }
    }

    public string CachePath { get; set; } = "Content";

    private Dictionary<string, FontInfo> _fontInfoMap = [];
    public List<GlyphInfo> GlyphInfoList { get; private init; } = [];

    private int _nextGlyphX = 2; // this is for default gui texture that has 2*2 white pixels
    private int _nextGlyphY = 0;
    private int _currentRowHeight = 2;

    // Multi-thread SDF bitmap generation
    private ConcurrentQueue<(Rectangle destRect, Typeface typeface, char c)> _reservedChars = [];
    private ConcurrentQueue<(Rectangle destRect, float[] bitmap)> _sdfResultProductBuffer = [];
    private volatile bool _isGeneratingSdf = false;
    private readonly object syncRoot = new();
    private ArrayPool<Color> _colorDataArrayPool = ArrayPool<Color>.Shared;

    // Bitmap png catch
    private int _fontInfoVersion;

    /// <summary>
    /// Set the parameters for SDF glyph generation.
    /// </summary>
    /// <param name="glyphPixelSize">Base pixel size</param>
    /// <param name="glyphPadding">Empty space around the character, in pixel. <br/><remarks>The value of glyphPadding should be greater than 2 * glyphRange, otherwise the gradient produced by glyphRange will be clipped.</remarks></param>
    /// <param name="glyphRange">The range factor for SDF glyph generation, affecting the pixelizing effect of the final glyph. </param>
    public void SetTextParameters(float glyphPixelSize, int glyphPadding, float glyphRange)
    {
        GlyphPixelSize = glyphPixelSize;
        GlyphPadding = glyphPadding;
        GlyphRange = glyphRange;
    }

    /// <summary>
    /// Load a font information from a .ttf file, set the default font to this font if its the first time calling this method.
    /// </summary>
    /// <param name="fontName">Font name</param>
    /// <param name="fontTtfPath">The related path of the .ttf file</param>
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

    /// <summary>
    /// Set the default font. 
    /// </summary>
    /// <remarks>
    /// Text-related elements will use the default font when not specifying a font name.
    /// </remarks>
    /// <param name="fontName"></param>
    /// <exception cref="Exception"></exception>
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
    /// <summary>
    /// Get fontInfo of a font name.
    /// </summary>
    /// <param name="fontName"></param>
    /// <returns></returns>
    public FontInfo GetFontInfo(string fontName)
    {
        return _fontInfoMap[fontName];
    }

    /// <summary>
    /// Start generating the demanding characters in the background. 
    /// </summary>
    /// <param name="fontName">Target font name</param>
    /// <param name="chars">Requiring characters</param>
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
        if (_nextGlyphX + sdfRenderInfo.BitmapWidth > GreyGui.Atlas.Width)
        {
            _nextGlyphX = 0;
            _nextGlyphY += _currentRowHeight + 1;
            _currentRowHeight = 0;
        }
        if (_nextGlyphY + sdfRenderInfo.BitmapHeight > GreyGui.Atlas.Height)
        {
            srcRect = Rectangle.Empty;
            return false;
        }
        srcRect = new(_nextGlyphX, _nextGlyphY, sdfRenderInfo.BitmapWidth, sdfRenderInfo.BitmapHeight);
        // Update _x and _currentHeight for the next insertion
        _nextGlyphX += sdfRenderInfo.BitmapWidth + 1;
        _currentRowHeight = Math.Max(sdfRenderInfo.BitmapHeight, _currentRowHeight);
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
    /// Generate the demanding characters. Using this method to reserve character will freeze the game until all reserving characters are generated.
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

    /// <summary>
    /// Set anti-aliasing factor in the shader. The higher the value, the sharper edge of text.
    /// </summary>
    /// <param name="value">Value of anti-aliasing factor</param>
    public void SetAntiAliasingRange(float value)
    {
        GreyGui.Shader.Parameters["antiAliasingRange"].SetValue(value);
    }

    public void ExportAtlasToStorage()
    {
        string savingPath = Path.Combine(CachePath, "GreyGui");
        Directory.CreateDirectory(savingPath);
        string pngPath = Path.Combine(savingPath, "CachedAtlas.png");
        using FileStream fs = File.OpenWrite(pngPath);
        {
            Rectangle rect = GreyGui.Atlas.Bounds;
            // Color[] color = new Color[rect.Width * rect.Height];
            // GreyGui.Atlas.GetData(0, 0, rect, color, 0, rect.Width * rect.Height);

            GreyGui.Atlas.SaveAsPng(fs, rect.Width, rect.Height);
        }

        AtlasInfo atlasInfo = new()
        {
            FontInfoMap = _fontInfoMap,
            GlyphInfoList = GlyphInfoList,
            NextGlyphX = _nextGlyphX,
            NextGlyphY = _nextGlyphY,
            CurrentRowHeight = _currentRowHeight
        };
        string jsonPath = Path.Combine(savingPath, "CachedAtlas.json");
        File.WriteAllText(jsonPath, JsonSerializer.Serialize(atlasInfo));
        Console.WriteLine("Exported");
    }
    public void LoadAtlas()
    {

    }

    private bool TryInsertGlyph(SimpleSdfResult sdfResult, out Rectangle srcRect)
    {
        if (_nextGlyphX + sdfResult.BitmapWidth > GreyGui.Atlas.Width)
        {
            _nextGlyphX = 0;
            _nextGlyphY += _currentRowHeight + 1;
            _currentRowHeight = 0;
        }
        if (_nextGlyphY + sdfResult.BitmapHeight > GreyGui.Atlas.Height)
        {
            srcRect = Rectangle.Empty;
            return false;
        }
        srcRect = new(_nextGlyphX, _nextGlyphY, sdfResult.BitmapWidth, sdfResult.BitmapHeight);

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
        _nextGlyphX += sdfResult.BitmapWidth + 1;
        _currentRowHeight = Math.Max(sdfResult.BitmapHeight, _currentRowHeight);
        return true;
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