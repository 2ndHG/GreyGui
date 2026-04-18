using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace GreyGui;

public class Text : GreyGuiElement, IRatioElement
{
    public override Vector2 Size
    {
        get => _size;
        set
        {
            if (_size == value)
            {
                return;
            }
            _size = value;
            _isSizeDirty = true;
        }
    }
    public override Vector2 FinalSize => _finalSize;
    public Color BackgroundColor { get; set; }
    public override int ZIndex
    {
        get => _zIndex; set
        {
            if (_zIndex == value)
                return;

            if (Parent is not null)
            {
                Parent.IsChildrenZIndexDirty = true;
            }
            _zIndex = value;

        }
    }

    public TextWidthMode WidthMode
    {
        get => _widthMode;
        set
        {
            if (_widthMode == value) return;
            _widthMode = value;

            _isSizeDirty = true;
        }
    }
    public TextHeightMode HeightMode
    {
        get => _heightMode;
        set
        {
            if (_heightMode == value)
                return;
            _heightMode = value;
            _isSizeDirty = true;
        }
    }

    public float WidthRatio
    {
        get => _widthRatio;
        set
        {
            if (_widthRatio == value)
                return;

            _widthRatio = value;
            _isSizeDirty = true;
        }
    }

    public float HeightWidthRatio
    {
        get => _heightWidthRatio;
        set
        {
            if (_heightWidthRatio == value)
                return;

            _heightWidthRatio = value;
            _isSizeDirty = true;
        }
    }

    public float HeightRatio
    {
        get => _heightRatio;
        set
        {
            if (_heightRatio == value)
                return;

            _heightRatio = value;
            _isSizeDirty = true;
        }
    }
    public TextAlignment AlignMode
    {
        get => _alignMode;
        set
        {
            if (_alignMode != value)
            {
                _alignMode = value;

                _isSizeDirty = _autoEndLine;
            }
        }
    }
    public string FontName
    {
        get => _fontName;
        set
        {
            if (value == null || _fontName == value)
                return;
            _fontName = value;
            _isDisplayTextDirty = true;
            _isSizeDirty = _autoEndLine;
        }
    }
    public string DisplayText
    {
        get => _displayText;
        set
        {
            if (_displayText.Equals(value))
            {
                return;
            }
            _displayText = value;
            _isDisplayTextDirty = true;
            _isSizeDirty = _autoEndLine;
        }
    }
    public float FontSize
    {
        get => _fontSize;
        set
        {
            if (_fontSize == value)
            {
                return;
            }
            _fontSize = value;

            _isSizeDirty = _autoEndLine;
        }
    }
    public float TextYOffset
    {
        get => _textYOffset;
        set
        {
            _textYOffset = value;
        }
    }
    public FontSizeScalingMode FontSizeScalingMode
    {
        get => _fontSizeScalingMode;
        set
        {
            if (_fontSizeScalingMode == value)
            {
                return;
            }

            _fontSizeScalingMode = value;
        }
    }
    public float FontSizeScalingBaseline
    {
        get => _fontSizeScalingBaseline;
        set
        {
            if (_fontSizeScalingBaseline == value)
            {
                return;
            }
            _fontSizeScalingBaseline = value;

            _isSizeDirty = _autoEndLine;
        }
    }
    public bool AutoEndLine
    {
        get => _autoEndLine;
        set
        {
            if (_autoEndLine == value)
            {
                return;
            }
            _autoEndLine = value;

            _isSizeDirty = _autoEndLine;
        }
    }

    private Vector2 _size;
    private Vector2 _finalSize;
    private int _zIndex;
    private TextWidthMode _widthMode;
    private TextHeightMode _heightMode;
    private float _widthRatio;
    private float _heightRatio;
    private float _heightWidthRatio;
    private TextAlignment _alignMode;
    private string _fontName = GreyGui.TextSystem.DefaultFont;
    private string _displayText;
    private float _fontSize;
    private bool _isDisplayTextDirty;
    private int _fontInfoVersion;
    private float _textYOffset;
    private FontSizeScalingMode _fontSizeScalingMode;
    private float _fontSizeScalingBaseline;

    // Layout
    private readonly List<TextSegment> _textSegments = [];
    private readonly List<int> _displayTextCharIndices = [];
    private readonly List<Vector2> _segmentOffsetCache = [];
    private bool _autoEndLine;
    private int _rowCount = 1;
    private float _maxWidth = 0;


    public Text(
    Color? colorMask = null, Color? borderColor = null, Color? backgroundColor = null, Vector2 size = default, int borderRadius = default, int borderWidth = default,
    TextWidthMode widthMode = TextWidthMode.Fixed, TextHeightMode heightMode = TextHeightMode.Fixed, float widthRatio = default, float heightRatio = default, float heightWidthRatio = default, int zIndex = default, TextAlignment alignMode = TextAlignment.Left, string? fontName = null, string displayText = "", float fontSize = -1f, float textYOffset = default, FontSizeScalingMode fontSizeScalingMode = FontSizeScalingMode.None, float fontSizeScalingBaseline = 0, bool autoEndLine = default)
    {
        ColorMask = colorMask ?? Color.Black;
        BorderColor = borderColor ?? Color.Transparent;
        BackgroundColor = backgroundColor ?? Color.Transparent;
        BorderRadius = borderRadius;
        BorderWidth = borderWidth;
        _size = size;

        _widthMode = widthMode;
        _heightMode = heightMode;
        _widthRatio = widthRatio;
        _heightRatio = heightRatio;
        _heightWidthRatio = heightWidthRatio;
        _zIndex = zIndex;
        _alignMode = alignMode;
        _fontName = fontName ?? GreyGui.TextSystem.DefaultFont;
        _displayText = displayText;
        fontSize = fontSize < 0 ? GreyGui.TextSystem.DefaultFontSize : fontSize;
        _fontSize = fontSize;
        _textYOffset = textYOffset;
        _fontSizeScalingMode = fontSizeScalingMode;

        // adoption priority: user input > FontSizeScalingMode > default
        _fontSizeScalingBaseline = (fontSizeScalingBaseline, fontSizeScalingMode, size.X, size.Y) switch
        {
            ( > 0, _, _, _) => fontSizeScalingBaseline,
            ( <= 0.1f, FontSizeScalingMode.UseWidthRatio, > 0, _) => size.X,
            ( <= 0.1f, FontSizeScalingMode.UseHeightRatio, _, > 0) => size.Y,
            _ => fontSize,
        };
        _autoEndLine = autoEndLine;

        _isSizeDirty = true;
        _isDisplayTextDirty = true;
    }

    private void ResolveLayoutDirty()
    {
        if (_widthMode != TextWidthMode.TextWidth)
            ResolveLayoutDirtyNotUseTextWidth();
        else
            ResolveLayoutDirtyUseTextWidth();
    }

    private void ResolveLayoutDirtyUseTextWidth()
    {
        _segmentOffsetCache.Clear();
        _maxWidth = 0;
        _rowCount = 0;
        if (_textSegments.Count == 0) { return; }

        float fontSize = GetFinalFontSize();
        float scale = fontSize / GreyGui.TextSystem.GlyphPixelSize;
        float widthSum = 0;
        int rowCount = 0;
        Vector2 offset = new(0, 0);


        // The first traversal defines the width of this ui element, i.e., size.X.
        ReadOnlySpan<TextSegment> textSegmentSpan = CollectionsMarshal.AsSpan(_textSegments);
        for (int i = 0; i < textSegmentSpan.Length; ++i)
        {
            TextSegment currentSegment = _textSegments[i];
            if (currentSegment.pendingSpaceWidth == float.PositiveInfinity)
            {
                _maxWidth = Math.Max(widthSum, _maxWidth);
                widthSum = 0;
                continue;
            }
            widthSum += currentSegment.WidthWithSpace * scale;
        }
        _maxWidth = Math.Max(widthSum, _maxWidth);

        int undrewLastIndex = 0;
        widthSum = 0;
        for (int i = 0; i < _textSegments.Count; ++i)
        {
            TextSegment currentSegment = _textSegments[i];
            if (currentSegment.pendingSpaceWidth == float.PositiveInfinity)
            {
                ComputeNotFullRow(i);
                _segmentOffsetCache.Add(offset);
                offset.X = 0;
                offset.Y += fontSize;
                widthSum = 0;
                undrewLastIndex = i + 1;
                continue;
            }
            widthSum += currentSegment.WidthWithSpace * scale;
        }
        if (undrewLastIndex < _textSegments.Count)
        {
            ComputeNotFullRow(_textSegments.Count);
        }

        _rowCount = rowCount;
        return;
        void ComputeNotFullRow(int endIndex)
        {
            // spread is same as left in the last row
            offset.X = _alignMode switch
            {
                TextAlignment.Center => (_maxWidth - widthSum) / 2,
                TextAlignment.Right => _maxWidth - widthSum,
                _ => 0
            };
            int gapCount = endIndex - undrewLastIndex - 1;
            float justifyGap = _alignMode == TextAlignment.Justify && gapCount > 0 ? (_maxWidth - widthSum) / gapCount : 0;

            for (; undrewLastIndex < endIndex; ++undrewLastIndex)
            {
                _segmentOffsetCache.Add(offset);

                TextSegment drawingSegment = _textSegments[undrewLastIndex];
                offset.X += (drawingSegment.width + drawingSegment.pendingSpaceWidth) * scale + justifyGap;
            }
            if (_alignMode == TextAlignment.Justify)
                offset.X -= justifyGap;
            ++rowCount;
        }
    }

    private void ResolveLayoutDirtyNotUseTextWidth()
    {
        _segmentOffsetCache.Clear();
        _maxWidth = 0;
        _rowCount = 0;
        if (_textSegments.Count == 0) { return; }

        float fontSize = GetFinalFontSize();
        float scale = fontSize / GreyGui.TextSystem.GlyphPixelSize;
        float endLineThreshold = _autoEndLine ? _finalSize.X : float.MaxValue;
        float widthSum = 0;
        float prevSegmentSpaceWidth = 0;
        int undrewLastIndex = 0;
        int rowCount = 0;
        Vector2 offset = new(0, 0);
        float singleNewlineX = _alignMode switch
        {
            TextAlignment.Center => _finalSize.X / 2,
            TextAlignment.Right => _finalSize.X,
            _ => 0
        };

        for (int i = 0; i < _textSegments.Count; ++i)
        {
            TextSegment currentSegment = _textSegments[i];
            if (currentSegment.pendingSpaceWidth == float.PositiveInfinity)
            {
                if (i > undrewLastIndex)
                {
                    ComputeRow(i, false);
                    _segmentOffsetCache.Add(offset);
                }
                else
                {
                    _segmentOffsetCache.Add(new(singleNewlineX, offset.Y));
                    ++rowCount;
                }
                offset.X = 0;
                offset.Y += fontSize;
                widthSum = 0;
                prevSegmentSpaceWidth = 0;
                undrewLastIndex = i + 1;
                continue;
            }
            if (_autoEndLine && currentSegment.width * scale + widthSum > endLineThreshold)
            {
                if (i > undrewLastIndex)
                {
                    ComputeRow(i, true);
                    offset.X = 0;
                    offset.Y += fontSize;
                    widthSum = 0;
                }
            }
            widthSum += currentSegment.WidthWithSpace * scale;
            prevSegmentSpaceWidth = currentSegment.pendingSpaceWidth * scale;
        }
        if (undrewLastIndex < _textSegments.Count)
        {
            ComputeRow(_textSegments.Count, false);
        }

        _rowCount = rowCount;
        return;

        void ComputeRow(int endIndex, bool isFullRow)
        {
            float rowWidth = _finalSize.X;
            offset.X = _alignMode switch
            {
                TextAlignment.Center => (rowWidth - widthSum + prevSegmentSpaceWidth) / 2,
                TextAlignment.Right => rowWidth - widthSum + prevSegmentSpaceWidth,
                _ => 0
            };
            float justifyGap = 0;
            if (isFullRow && _alignMode == TextAlignment.Justify)
            {
                int gapCount = endIndex - undrewLastIndex - 1;
                if (gapCount > 0)
                {
                    justifyGap = (rowWidth - widthSum + prevSegmentSpaceWidth) / gapCount;
                }
            }

            for (; undrewLastIndex < endIndex; ++undrewLastIndex)
            {
                _segmentOffsetCache.Add(offset);

                TextSegment drawingSegment = _textSegments[undrewLastIndex];
                offset.X += (drawingSegment.width + drawingSegment.pendingSpaceWidth) * scale + justifyGap;
            }
            ++rowCount;
        }
    }



    private void RecalculateSize()
    {
        Vector2 sizeBefore = _finalSize;
        _finalSize = _size;
        // As an IRatioElement
        if (_widthMode == TextWidthMode.ParentRatio)
        {
            if (_parent == null)
            {
                _finalSize.X = GreyGui.NullParentWidth * _widthRatio;
            }
            else
            {
                _finalSize.X = _parent.ContainerSize.X * _widthRatio;
            }
        }

        // UseHeightRatio has a higher priority then UseHeightWidthRatio
        if (_heightMode == TextHeightMode.ParentRatio)
        {
            if (_parent == null)
            {
                _finalSize.Y = GreyGui.NullParentHeight * _heightRatio;
            }
            else
            {
                _finalSize.Y = _parent.ContainerSize.Y * _heightRatio;
            }
        }
        else if (_heightMode == TextHeightMode.HeightWidthRatio)
        {
            _finalSize.Y = _finalSize.X * _heightWidthRatio;
        }

        // we have calculated the layout before if UseTextHeight is true
        if (_isDisplayTextDirty)
            ResolveDisplayTextDirty();
        ResolveLayoutDirty();
        if (_widthMode == TextWidthMode.TextWidth)
            _finalSize.X = _maxWidth;
        if (_heightMode == TextHeightMode.TextHeight)
            _finalSize.Y = _rowCount * GetFinalFontSize();


        if (sizeBefore != _finalSize && _parent is not null)
        {
            _parent.IsLayoutDirty = true;
        }

        _isSizeDirty = false;
    }

    private void ParseText()
    {
        FontInfo fontInfo = GreyGui.TextSystem.GetFontInfo(_fontName);
        float spaceAdvanceWidth;
        if (fontInfo.GlyphInfoIndexMap.TryGetValue(' ', out ushort spaceGlyphInfoIndex))
        {
            spaceAdvanceWidth = GreyGui.TextSystem.GlyphInfoList[spaceGlyphInfoIndex].AdvanceWidth;
        }
        else
        {
            spaceAdvanceWidth = GreyGui.TextSystem.GlyphPixelSize / 4;
        }

        _displayTextCharIndices.Clear();
        _textSegments.Clear();

        if (string.IsNullOrEmpty(_displayText))
        {
            _textSegments.Add(new TextSegment() { });
            return;
        }

        TextSegment currentSegment = new() { startIndex = 0 };

        ushort pendingSpaces = 0;

        for (int i = 0; i < _displayText.Length; i++)
        {
            char c = _displayText[i];
            int charIndex = fontInfo.GetCharIndex(c);
            _displayTextCharIndices.Add(charIndex);
            float charWidth = GreyGui.TextSystem.GlyphInfoList[charIndex].AdvanceWidth;

            if (c == ' ')
            {
                pendingSpaces++;
                continue;
            }
            else if (c == '\n')
            {
                // Add the current segment
                if (currentSegment.nonSpaceCount + pendingSpaces > 0)
                {
                    currentSegment.pendingSpaceWidth = spaceAdvanceWidth * pendingSpaces;
                    currentSegment.pendingSpaceCount = pendingSpaces;
                    _textSegments.Add(currentSegment);
                }

                // Add a special segment that indicating '\n'
                _textSegments.Add(new TextSegment() { startIndex = i, pendingSpaceCount = 1, pendingSpaceWidth = float.PositiveInfinity, width = 0 });

                currentSegment = new() { startIndex = i + 1 };
                pendingSpaces = 0;
                continue;
            }

            // c is not a space or \n
            bool shouldSplit = false;

            if (pendingSpaces > 0)
            {
                shouldSplit = true;
            }
            if (currentSegment.nonSpaceCount > 0)
            {
                char prevC = _displayText[i - 1];

                if (TextHelper.IsCjk(c) || TextHelper.IsCjk(prevC))
                {
                    if (!TextHelper.IsLineStartForbidden(c) && !TextHelper.IsLineEndForbidden(prevC))
                    {
                        shouldSplit = true;
                    }
                }
            }

            if (shouldSplit)
            {
                currentSegment.pendingSpaceWidth = spaceAdvanceWidth * pendingSpaces;
                currentSegment.pendingSpaceCount = pendingSpaces;
                _textSegments.Add(currentSegment);

                currentSegment = new() { startIndex = i };
                pendingSpaces = 0;
            }

            currentSegment.width += charWidth;
            currentSegment.nonSpaceCount++;
        }

        // the remaining segment
        if (pendingSpaces >= 0 || currentSegment.nonSpaceCount > 0)
        {
            currentSegment.pendingSpaceWidth = spaceAdvanceWidth * pendingSpaces;
            currentSegment.pendingSpaceCount = pendingSpaces;
            _textSegments.Add(currentSegment);
        }

    }


    private void ResolveDisplayTextDirty()
    {
        GreyGui.TextSystem.ReserveChars(_fontName, DisplayText);
        ParseText();

        _isDisplayTextDirty = false;
        _fontInfoVersion = GreyGui.TextSystem.FontInfoVersion;
    }

    private float GetFinalFontSize()
    {
        float fontSize;
        // In this two situation, UseWidthRatio or UseHeightRatio is not allowed
        if ((_widthMode == TextWidthMode.TextWidth && _fontSizeScalingMode == FontSizeScalingMode.UseWidthRatio) ||
            (_heightMode == TextHeightMode.TextHeight && _fontSizeScalingMode == FontSizeScalingMode.UseHeightRatio))
        {
            fontSize = _fontSize;
        }
        else
        {
            fontSize = _fontSizeScalingMode switch
            {
                FontSizeScalingMode.UseWidthRatio => _finalSize.X / _fontSizeScalingBaseline * _fontSize,
                FontSizeScalingMode.UseHeightRatio => _finalSize.Y / _fontSizeScalingBaseline * _fontSize,
                _ => _fontSize
            };
        }
        return Math.Max(0, fontSize);
    }

    public override void Draw(Point pos, RenderContext renderContext, Rectangle screenScissor)
    {
        if (_isDisplayTextDirty || _fontInfoVersion != GreyGui.TextSystem.FontInfoVersion)
        {
            ResolveDisplayTextDirty();
        }

        if (BackgroundColor != Color.Transparent || BorderWidth > 0)
        {
            renderContext.FillRect(new Rectangle(pos, _finalSize.ToPoint()), BackgroundColor, BorderColor, BorderRadius, BorderWidth, screenScissor);
        }

        float fontSize = GetFinalFontSize();
        Vector2 position = pos.ToVector2();
        position.Y += fontSize + _textYOffset;

        for (int i = 0; i < _textSegments.Count; ++i)
        {
            TextSegment currentSegment = _textSegments[i];
            renderContext.RenderTextUsingCharIndices(_displayTextCharIndices, currentSegment.startIndex, currentSegment.nonSpaceCount, position + _segmentOffsetCache[i], fontSize, ColorMask, screenScissor);
        }
    }

    public override void ResolveSizeDirty()
    {
        if (_isSizeDirty)
        {
            RecalculateSize();
        }
    }

    public override void Update()
    {

    }


}