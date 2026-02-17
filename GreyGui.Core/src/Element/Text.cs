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
            _fontSize = value;
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
    public float FontSizeScalingBaseline
    {
        get => _fontSizeScalingBaseline;
        set
        {
            _fontSizeScalingBaseline = value;
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
            IsSizeDirty = true;
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
    private int _textSegmentCount = 1;
    private bool _autoEndLine;
    private readonly List<int> _displayTextCharIndices = [];

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

    private int CalculateRowCount()
    {
        if (!_autoEndLine)
        {
            return 1;
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
        float widthSum = _textSegments[0].widthWithSpace * scale;
        float widthSumWithoutSpace = 0;
        int rowCount = 0;
        for (int i = 1; i < _textSegmentCount; ++i)
        {
            TextSegment currentSegment = _textSegments[i];
            // the row is full, we must draw the row before the current segment comes in
            if (widthSum + currentSegment.widthWithoutSpace * scale > maxRowSpace)
            {
                ++rowCount;
                widthSum = 0;
            }

            // Add the segment in the row
            widthSum += currentSegment.widthWithSpace * scale;
            widthSumWithoutSpace += currentSegment.widthWithoutSpace * scale;
        }
        ++rowCount;

        return rowCount;
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
            if (_isDisplayTextDirty)
            {
                ResolveDisplayTextDirty();
            }
            float fontSize = _fontSizeScalingMode switch
            {
                FontSizeScalingMode.UseWidthRatio => _size.X / _fontSizeScalingBaseline * _fontSize,
                _ => _fontSize
            };
            fontSize = Math.Abs(fontSize);
            _size.Y = CalculateRowCount() * fontSize;

            sizeChanged = true;
        }

        if (sizeChanged && _parent is not null)
        {
            _parent.IsLayoutDirty = true;
        }

        _isSizeDirty = false;
    }

    private void ParseText_InProgress()
    {
        TextSegment GetOrCreateSegment(int index)
        {
            if (index < _textSegments.Count)
            {
                return _textSegments[index];
            }
            else
            {
                TextSegment segment = new();
                _textSegments.Add(segment);
                return segment;
            }
        }
        void ResetTextSegment(TextSegment textSegment)
        {
            textSegment.spaceWidth = textSegment.widthWithoutSpace = textSegment.widthWithSpace = 0;
        }

        FontInfo fontInfo = GreyGui.TextSystem.GetFontInfo(_fontName);
        GlyphInfo spaceInfo = fontInfo.GlyphInfoMap[' '];
        float spaceAdvanceWidth = spaceInfo.AdvanceWidth;

        _displayTextCharIndices.Clear();
        _textSegmentCount = 0;
        TextSegment textSegment = GetOrCreateSegment(0);
        ResetTextSegment(textSegment);
        textSegment.startIndex = 0;

        bool prevIsSpace = false;
        int backspacesOfThisSegment = 0;
        for (int i = 0; i < _displayText.Length; ++i)
        {
            char c = _displayText[i];
            int charIndex = fontInfo.GetCharIndex(c);
            _displayTextCharIndices.Add(charIndex);
            if (c == ' ')
            {
                ++backspacesOfThisSegment;
                prevIsSpace = true;
            }
            else
            {
                if (prevIsSpace)
                {
                    float backspaceWidth = spaceAdvanceWidth * backspacesOfThisSegment;
                    textSegment.widthWithSpace = textSegment.widthWithoutSpace + backspaceWidth;
                    textSegment.spaceWidth = backspaceWidth;
                    _textSegmentCount++; // Add back

                    textSegment = GetOrCreateSegment(_textSegmentCount);
                    textSegment.startIndex = i;
                    ResetTextSegment(textSegment);
                    backspacesOfThisSegment = 0;
                }
                GlyphInfo glyphInfo = GreyGui.TextSystem.GlyphInfoList[charIndex];
                textSegment.widthWithoutSpace += glyphInfo.AdvanceWidth;
                textSegment.length++;
                prevIsSpace = false;
            }
        }
        // Add the last segment
        {
            float backspaceWidth = spaceAdvanceWidth * backspacesOfThisSegment;
            textSegment.widthWithSpace = textSegment.widthWithoutSpace + backspaceWidth;
            textSegment.spaceWidth = backspaceWidth;
            _textSegmentCount++;
        }
    }

    private void ResolveDisplayTextDirty()
    {
        GreyGui.TextSystem.ReserveChars(_fontName, DisplayText);
        ParseText_InProgress();

        _isDisplayTextDirty = false;
    }
    public override void Draw(Point pos, RenderContext renderContext, Rectangle screenScissor)
    {
        if (_isDisplayTextDirty)
        {
            ResolveDisplayTextDirty();
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

        float scale = fontSize / GreyGui.TextSystem.GlyphPixelSize;
        float maxRowSpace = _autoEndLine ? _size.X : float.MaxValue;
        float widthSum = _textSegments[0].widthWithSpace * scale;
        float widthSumWithoutSpace = 0;
        float prevSegmentSpaceWidth = _textSegments[0].spaceWidth * scale;
        int undrewLastIndex = 0;
        Vector2 offset = new();

        for (int i = 1; i < _textSegmentCount; ++i)
        {
            TextSegment currentSegment = _textSegments[i];
            // the row is full, we must draw the row before the current segment comes in
            if (widthSum + currentSegment.widthWithoutSpace * scale > maxRowSpace)
            {
                // render this row with specified layout mode
                offset.X = _alignMode switch
                {
                    RowLayoutMode.Center => (maxRowSpace - widthSum + prevSegmentSpaceWidth) / 2,
                    RowLayoutMode.Right => maxRowSpace - widthSum + prevSegmentSpaceWidth,
                    _ => 0
                };
                float justifyGap = _alignMode == RowLayoutMode.Spread ? (maxRowSpace - widthSum + prevSegmentSpaceWidth) / (i - undrewLastIndex - 1) : 0;

                for (; undrewLastIndex < i; ++undrewLastIndex)
                {
                    TextSegment drawingSegment = _textSegments[undrewLastIndex];
                    renderContext.RenderTextUsingCharIndices(_displayTextCharIndices, drawingSegment.startIndex, drawingSegment.length, position + offset, fontSize, ColorMask, screenScissor);
                    offset.X += (drawingSegment.widthWithoutSpace + drawingSegment.spaceWidth) * scale + justifyGap;
                }
                widthSum = 0;
                offset.X = 0;
                offset.Y += fontSize;
            }

            // Add the segment in the row
            widthSum += currentSegment.widthWithSpace * scale;
            widthSumWithoutSpace += currentSegment.widthWithoutSpace * scale;
            prevSegmentSpaceWidth = _textSegments[i].spaceWidth * scale;
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
                TextSegment drawingSegment = _textSegments[undrewLastIndex];
                renderContext.RenderTextUsingCharIndices(_displayTextCharIndices, drawingSegment.startIndex, drawingSegment.length, position + offset, fontSize, ColorMask, screenScissor);
                offset.X += (drawingSegment.widthWithoutSpace + drawingSegment.spaceWidth) * scale;
            }
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
        public int startIndex;
        public int length;
        public float widthWithoutSpace;
        public float widthWithSpace;
        public float spaceWidth;
    }
}