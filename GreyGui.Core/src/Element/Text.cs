using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace GreyGui;

public class Text : GreyGuiElement, IRatioElement
{
    public override Vector2 Size
    {
        get => _size; set
        {
            if (_size == value)
            {
                return;
            }
            _size = value;
            _isSizeDirty = true;
        }
    }
    public override int ZIndex
    {
        get => _zIndex; set
        {
            if (_zIndex == value)
                return;

            if (Parent is not null)
            {
                Parent.IsChildrenIndexDirty = true;
            }
            _zIndex = value;

        }
    }

    public bool UseWidthRatio
    {
        get => _useWidthRatio;
        set
        {
            if (_useWidthRatio == value)
                return;

            _useWidthRatio = value;
            _isSizeDirty = true;
        }
    }

    public bool UseHeightRatio
    {
        get => _useHeightRatio;
        set
        {
            if (_useHeightRatio == value)
                return;

            _useHeightRatio = value;
            _isSizeDirty = true;
        }
    }

    public bool UseHeightWidthRatio
    {
        get => _useHeightWidthRatio;
        set
        {
            if (_useHeightWidthRatio == value)
                return;

            _useHeightWidthRatio = value;
            _isSizeDirty = true;
        }
    }
    public bool UseTextHeight
    {
        get => _useTextHeight;
        set
        {
            if (_useTextHeight == value)
            {
                return;
            }
            _useTextHeight = value;
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
    public RowLayoutMode AlignMode
    {
        get => _alignMode;
        set
        {
            if (_alignMode != value)
            {
                _alignMode = value;

                _isLayoutDirty = true;
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

            _isLayoutDirty = true;
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
            _isLayoutDirty = true;
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

            _isLayoutDirty = true;
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

            _isLayoutDirty = true;
            _isSizeDirty = _autoEndLine;
        }
    }

    private Vector2 _size;
    private int _zIndex;
    private bool _useWidthRatio;
    private bool _useHeightRatio;
    private bool _useHeightWidthRatio;
    private bool _useTextHeight;
    private float _widthRatio;
    private float _heightRatio;
    private float _heightWidthRatio;
    private RowLayoutMode _alignMode;
    private string _fontName = GreyGui.TextSystem.DefaultFont;
    private string _displayText;
    private float _fontSize;
    private bool _isDisplayTextDirty;
    private float _textYOffset;
    private FontSizeScalingMode _fontSizeScalingMode;
    private float _fontSizeScalingBaseline;

    // Layout
    private readonly List<TextSegment> _textSegments = [];
    private readonly List<int> _displayTextCharIndices = [];
    private readonly List<Vector2> _segmentOffsetCache = [];
    private int _textSegmentCount = 0;
    private bool _autoEndLine;
    private bool _isLayoutDirty = false;
    private int _rowCount = 1;

    public Text(Color colorMask, Color borderColor = default, Vector2 size = default, bool useWidthRatio = default, bool useHeightRatio = default, bool useHeightWidthRatio = default, bool useTextHeight = default, float widthRatio = default, float heightRatio = default, float heightWidthRatio = default, int zIndex = default, RowLayoutMode alignMode = RowLayoutMode.Left, string? fontName = null, string displayText = "", float fontSize = -1f, float textYOffset = default, FontSizeScalingMode fontSizeScalingMode = FontSizeScalingMode.None, float fontSizeScalingBaseline = 0, bool autoEndLine = default)
    {
        ColorMask = colorMask;
        BorderColor = borderColor;
        _size = size;
        _useWidthRatio = useWidthRatio;
        _useHeightRatio = useHeightRatio;
        _useHeightWidthRatio = useHeightWidthRatio;
        _useTextHeight = useTextHeight;
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
        _segmentOffsetCache.Clear();
        if (_textSegmentCount == 0)
        {
            return;
        }
        float fontSize = _fontSizeScalingMode switch
        {
            FontSizeScalingMode.UseWidthRatio => _size.X / _fontSizeScalingBaseline * _fontSize,
            FontSizeScalingMode.UseHeightRatio => _size.Y / _fontSizeScalingBaseline * _fontSize,
            _ => _fontSize
        };
        fontSize = Math.Abs(fontSize);
        float scale = fontSize / GreyGui.TextSystem.GlyphPixelSize;
        float maxRowSpace = _autoEndLine ? _size.X : float.MaxValue;
        float widthSum = _textSegments[0].WidthWithSpace * scale;
        float widthSumWithoutSpace = 0;
        float prevSegmentSpaceWidth = _textSegments[0].pendingSpaceWidth * scale;
        int undrewLastIndex = 0;
        int rowCount = 0;
        Vector2 offset = new(0, 0);
        for (int i = 1; i < _textSegmentCount; ++i)
        {
            TextSegment currentSegment = _textSegments[i];
            // the row is full, we must draw the row before the current segment comes in
            if (widthSum + currentSegment.width * scale > maxRowSpace)
            {
                offset.X = _alignMode switch
                {
                    RowLayoutMode.Center => (maxRowSpace - widthSum + prevSegmentSpaceWidth) / 2,
                    RowLayoutMode.Right => maxRowSpace - widthSum + prevSegmentSpaceWidth,
                    _ => 0
                };
                float justifyGap = _alignMode == RowLayoutMode.Spread ? (maxRowSpace - widthSum + prevSegmentSpaceWidth) / (i - undrewLastIndex - 1) : 0;

                for (; undrewLastIndex < i; ++undrewLastIndex)
                {
                    _segmentOffsetCache.Add(offset);

                    TextSegment drawingSegment = _textSegments[undrewLastIndex];
                    offset.X += (drawingSegment.width + drawingSegment.pendingSpaceWidth) * scale + justifyGap;
                }
                offset.X = 0;
                offset.Y += fontSize;
                ++rowCount;
                widthSum = 0;
            }

            // Add the segment in the row
            widthSum += currentSegment.WidthWithSpace * scale;
            widthSumWithoutSpace += currentSegment.width * scale;
            prevSegmentSpaceWidth = currentSegment.pendingSpaceWidth * scale;
        }
        {
            // the last row
            // spread is same as left in the last row
            maxRowSpace = _size.X;
            offset.X = _alignMode switch
            {
                RowLayoutMode.Center => (maxRowSpace - widthSum + prevSegmentSpaceWidth) / 2,
                RowLayoutMode.Right => maxRowSpace - widthSum + prevSegmentSpaceWidth,
                _ => 0
            };
            for (; undrewLastIndex < _textSegmentCount; ++undrewLastIndex)
            {
                _segmentOffsetCache.Add(offset);

                TextSegment drawingSegment = _textSegments[undrewLastIndex];
                offset.X += (drawingSegment.width + drawingSegment.pendingSpaceWidth) * scale;
            }
            ++rowCount;
        }

        _rowCount = rowCount;
        _isLayoutDirty = false;
    }

    private void RecalculateSize()
    {
        // As an IRatioElement
        bool sizeChanged = false;
        if (UseWidthRatio && _parent != null)
        {
            _size.X = _parent.ContainerSize.X * _widthRatio;
            sizeChanged = true;
        }
        // UseHeightRatio has a higher priority then UseHeightWidthRatio
        if (UseHeightRatio)
        {
            if (_parent != null)
            {
                _size.Y = _parent.ContainerSize.Y * _heightRatio;
                sizeChanged = true;
            }
        }
        else if (UseHeightWidthRatio)
        {
            _size.Y = _size.X * _heightWidthRatio;
            sizeChanged = true;
        }
        else if (UseTextHeight)
        {

            // Normally if the size is changed, we set _isLayoutDirty = true; later in this function.
            // But we need to calculate text layout here in order to calculate height, so check _isDisplayTextDirty and do recalculation
            if (_isDisplayTextDirty)
            {
                ResolveDisplayTextDirty();
            }

            if (sizeChanged)
            {
                ResolveLayoutDirty();
            }
            float fontSize = _fontSizeScalingMode switch
            {
                FontSizeScalingMode.UseWidthRatio => _size.X / _fontSizeScalingBaseline * _fontSize,
                _ => _fontSize
            };
            fontSize = Math.Abs(fontSize);
            _size.Y = _rowCount * fontSize;

            sizeChanged = true;
        }

        // we have calculated the layout before if UseTextHeight is true
        if (sizeChanged && !UseTextHeight)
        {
            _isLayoutDirty = true;
        }

        if (sizeChanged && _parent is not null)
        {
            _parent.IsLayoutDirty = true;
        }

        _isSizeDirty = false;
    }
    private void ParseText()
    {
        TextSegment GetOrCreateSegment(int index)
        {
            if (index < _textSegments.Count)
            {
                return _textSegments[index];
            }
            TextSegment segment = new();
            _textSegments.Add(segment);
            return segment;
        }

        void ResetTextSegment(TextSegment segment, int startIndex)
        {
            segment.pendingSpaceWidth = segment.width = 0;
            segment.startIndex = startIndex;
            segment.length = 0;
        }

        FontInfo fontInfo = GreyGui.TextSystem.GetFontInfo(_fontName);
        float spaceAdvanceWidth = fontInfo.GlyphInfoMap.TryGetValue(' ', out var spaceGlyphInfo) ?
            spaceGlyphInfo.AdvanceWidth :
            GreyGui.TextSystem.GlyphPixelSize / 4; // Normally this default setting should not be used

        _displayTextCharIndices.Clear();
        _textSegmentCount = 0;

        if (string.IsNullOrEmpty(_displayText)) return;

        TextSegment currentSegment = GetOrCreateSegment(0);
        ResetTextSegment(currentSegment, 0);

        int pendingSpaces = 0;

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

            // c is not a space
            bool shouldSplit = false;

            if (currentSegment.length > 0)
            {
                char prevC = _displayText[i - 1];

                if (pendingSpaces > 0)
                {
                    shouldSplit = true;
                }
                else if (TextHelper.IsCjk(c) || TextHelper.IsCjk(prevC))
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
                _textSegmentCount++;

                currentSegment = GetOrCreateSegment(_textSegmentCount);
                ResetTextSegment(currentSegment, i);
                pendingSpaces = 0;
            }

            currentSegment.width += charWidth;
            currentSegment.length++;
        }

        // the remaining segment
        currentSegment.pendingSpaceWidth = spaceAdvanceWidth * pendingSpaces;
        _textSegmentCount++;

        _isLayoutDirty = true;
    }

    private void ResolveDisplayTextDirty()
    {
        GreyGui.TextSystem.ReserveChars(_fontName, DisplayText);
        ParseText();

        _isDisplayTextDirty = false;
    }
    public override void Draw(Point pos, RenderContext renderContext, Rectangle screenScissor)
    {
        if (_isDisplayTextDirty)
        {
            ResolveDisplayTextDirty();
        }
        if (_isLayoutDirty)
        {
            ResolveLayoutDirty();
        }

        float fontSize = _fontSizeScalingMode switch
        {
            FontSizeScalingMode.UseWidthRatio => _size.X / _fontSizeScalingBaseline * _fontSize,
            FontSizeScalingMode.UseHeightRatio => _size.Y / _fontSizeScalingBaseline * _fontSize,
            _ => _fontSize
        };
        fontSize = Math.Abs(fontSize);
        Vector2 position = pos.ToVector2();
        position.Y += fontSize + _textYOffset;

        for (int i = 0; i < _textSegmentCount; ++i)
        {
            TextSegment currentSegment = _textSegments[i];
            renderContext.RenderTextUsingCharIndices(_displayTextCharIndices, currentSegment.startIndex, currentSegment.length, position + _segmentOffsetCache[i], fontSize, ColorMask, screenScissor);
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

    private class TextSegment
    {
        public float WidthWithSpace => width + pendingSpaceWidth;
        public int startIndex;
        public int length;
        public float width;
        public float pendingSpaceWidth;
    }

    // for reference purpose, delete when done refactoring
    private void ParseText_Legacy()
    {
        TextSegment GetOrCreateSegment(int index)
        {
            if (index < _textSegments.Count)
            {
                return _textSegments[index];
            }
            TextSegment segment = new();
            _textSegments.Add(segment);
            return segment;
        }

        void ResetTextSegment(TextSegment segment, int startIndex)
        {
            segment.pendingSpaceWidth = segment.width = 0;
            segment.startIndex = startIndex;
            segment.length = 0;
        }

        FontInfo fontInfo = GreyGui.TextSystem.GetFontInfo(_fontName);
        float spaceAdvanceWidth = fontInfo.GlyphInfoMap.TryGetValue(' ', out var spaceGlyphInfo) ?
            spaceGlyphInfo.AdvanceWidth :
            GreyGui.TextSystem.GlyphPixelSize / 4; // Normally this default setting should not be used

        _displayTextCharIndices.Clear();
        _textSegmentCount = 0;

        if (string.IsNullOrEmpty(_displayText)) return;

        TextSegment currentSegment = GetOrCreateSegment(0);
        ResetTextSegment(currentSegment, 0);

        int pendingSpaces = 0;

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

            // c is not a space
            bool shouldSplit = false;

            if (currentSegment.length > 0)
            {
                char prevC = _displayText[i - 1];

                if (pendingSpaces > 0)
                {
                    shouldSplit = true;
                }
                else if (TextHelper.IsCjk(c) || TextHelper.IsCjk(prevC))
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
                _textSegmentCount++;

                currentSegment = GetOrCreateSegment(_textSegmentCount);
                ResetTextSegment(currentSegment, i);
                pendingSpaces = 0;
            }

            currentSegment.width += charWidth;
            currentSegment.length++;
        }

        // the remaining segment
        currentSegment.pendingSpaceWidth = spaceAdvanceWidth * pendingSpaces;
        _textSegmentCount++;

        _isLayoutDirty = true;
    }

}