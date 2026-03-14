using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GreyGui.Core;

public class TextInput : GreyGuiElement, IRatioElement
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
                Parent.IsChildrenZIndexDirty = true;
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
            _cursorIndex = value.Length;

            _isDisplayTextDirty = true;
            _isSizeDirty = _autoEndLine || _useTextWidth || _useTextHeight;
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
    public Color FocusedColor { get; set; } = new Color(145, 171, 186, 255);

    private Vector2 _size;
    private int _zIndex;
    private bool _useWidthRatio;
    private bool _useHeightRatio;
    private bool _useHeightWidthRatio;
    private bool _useTextWidth;
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

    // update logic
    private int _cursorIndex = 0;

    // Layout
    private readonly List<TextSegment> _textSegments = [];
    private readonly List<int> _displayTextCharIndices = [];
    private readonly List<Vector2> _segmentOffsetCache = [];
    private bool _autoEndLine;
    private bool _isLayoutDirty = false;
    private int _rowCount = 1;
    private float _maxWidth = 0;

    public TextInput(Color? colorMask = null, Color borderColor = default, Vector2 size = default, bool useWidthRatio = default, bool useHeightRatio = default, bool useHeightWidthRatio = default, bool useTextWidth = default, bool useTextHeight = default, float widthRatio = default, float heightRatio = default, float heightWidthRatio = default, int zIndex = default, RowLayoutMode alignMode = RowLayoutMode.Left, string? fontName = null, string displayText = "", float fontSize = -1f, float textYOffset = default, FontSizeScalingMode fontSizeScalingMode = FontSizeScalingMode.None, float fontSizeScalingBaseline = 0, bool autoEndLine = default)
    {
        ColorMask = colorMask ?? Color.Black;
        BorderColor = borderColor;
        _size = size;
        _useTextWidth = useTextWidth;
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
    private void ResolveLayoutDirty3()
    {
        _segmentOffsetCache.Clear();
        _maxWidth = 0;
        if (_textSegments.Count == 0) { return; }
        float fontSize = _useTextWidth ?
            _fontSize : _fontSizeScalingMode switch
            {
                FontSizeScalingMode.UseWidthRatio => _size.X / _fontSizeScalingBaseline * _fontSize,
                FontSizeScalingMode.UseHeightRatio => _size.Y / _fontSizeScalingBaseline * _fontSize,
                _ => _fontSize
            };
        fontSize = Math.Abs(fontSize);
        RowLayoutMode alignMode = _useTextWidth ? RowLayoutMode.Left : _alignMode;
        float scale = fontSize / GreyGui.TextSystem.GlyphPixelSize;
        float endLineThreshold = (_autoEndLine && !_useTextWidth) ? _size.X : float.MaxValue;
        float widthSum = 0;
        float prevSegmentSpaceWidth = 0;
        int undrewLastIndex = 0;
        int rowCount = 0;
        Vector2 offset = new(0, 0);
        float endlineOrigin = (alignMode, _autoEndLine) switch
        {
            (RowLayoutMode.Center, true) => endLineThreshold / 2,
            (RowLayoutMode.Right, true) => endLineThreshold,
            _ => 0
        };

        for (int i = 0; i < _textSegments.Count; ++i)
        {
            TextSegment currentSegment = _textSegments[i];
            if (currentSegment.pendingSpaceWidth == float.PositiveInfinity)
            {
                if (i > undrewLastIndex)
                {
                    ComputeNotFullRow(i);
                    _segmentOffsetCache.Add(offset);
                }
                else
                {
                    _segmentOffsetCache.Add(new(endlineOrigin, offset.Y));
                    ++rowCount;
                }
                offset.X = 0;
                offset.Y += fontSize;
                widthSum = 0;
                prevSegmentSpaceWidth = 0;
                undrewLastIndex = i + 1;
                continue;
            }
            if (currentSegment.width * scale + widthSum > endLineThreshold)
            {
                if (i > undrewLastIndex)
                {
                    ComputeFullRow(i);
                }
            }
            widthSum += currentSegment.WidthWithSpace * scale;
            prevSegmentSpaceWidth = currentSegment.pendingSpaceWidth * scale;
        }
        if (undrewLastIndex < _textSegments.Count)
        {
            ComputeNotFullRow(_textSegments.Count);
        }

        _rowCount = rowCount;
        _isLayoutDirty = false;
        return;

        void ComputeFullRow(int endIndex)
        {
            _maxWidth = Math.Max(_maxWidth, widthSum);

            float rowWidth = _autoEndLine ? widthSum : (_useTextWidth ? _maxWidth : _size.X);
            offset.X = alignMode switch
            {
                RowLayoutMode.Center => (rowWidth - widthSum + prevSegmentSpaceWidth) / 2,
                RowLayoutMode.Right => rowWidth - widthSum + prevSegmentSpaceWidth,
                _ => 0
            };
            float justifyGap = alignMode == RowLayoutMode.Justify ? (rowWidth - widthSum + prevSegmentSpaceWidth) / (endIndex - undrewLastIndex - 1) : 0;

            for (; undrewLastIndex < endIndex; ++undrewLastIndex)
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

        void ComputeNotFullRow(int endIndex)
        {
            // _maxWidth is used for calculating size.X when _useTextWidth is enabled
            _maxWidth = Math.Max(_maxWidth, widthSum);

            float rowWidth = _autoEndLine ? widthSum : (_useTextWidth ? _maxWidth : _size.X);

            // spread is same as left in the last row
            offset.X = alignMode switch
            {
                RowLayoutMode.Center => (rowWidth - widthSum + prevSegmentSpaceWidth) / 2,
                RowLayoutMode.Right => rowWidth - widthSum + prevSegmentSpaceWidth,
                _ => 0
            };
            // if (_useTextWidth) offset.X = 0;
            for (; undrewLastIndex < endIndex; ++undrewLastIndex)
            {
                _segmentOffsetCache.Add(offset);

                TextSegment drawingSegment = _textSegments[undrewLastIndex];
                offset.X += (drawingSegment.width + drawingSegment.pendingSpaceWidth) * scale;
            }
            ++rowCount;
        }
    }

    private void RecalculateSize()
    {
        // As an IRatioElement
        bool sizeChanged = false;
        if (_useWidthRatio && _parent != null)
        {
            _size.X = _parent.ContainerSize.X * _widthRatio;
            sizeChanged = true;
        }
        else if (_useTextWidth)
        {
            if (_isDisplayTextDirty)
            {
                ResolveDisplayTextDirty();
            }
            if (_isLayoutDirty)
            {
                ResolveLayoutDirty3();
            }
            _size.X = _maxWidth;
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

            if (sizeChanged || _isLayoutDirty)
            {
                ResolveLayoutDirty3();
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
        FontInfo fontInfo = GreyGui.TextSystem.GetFontInfo(_fontName);
        float spaceAdvanceWidth = fontInfo.GlyphInfoMap.TryGetValue(' ', out var spaceGlyphInfo) ?
            spaceGlyphInfo.AdvanceWidth :
            GreyGui.TextSystem.GlyphPixelSize / 4; // Normally any font should contains ' ' when initialized

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

        _isLayoutDirty = true;
    }

    private void ResolveDisplayTextDirty()
    {
        GreyGui.TextSystem.ReserveChars(_fontName, DisplayText);
        ParseText();

        _isDisplayTextDirty = false;
    }

    private int GetCursorSegmentIndex()
    {
        if (_textSegments.Count == 0)
        {
            return 0;
        }

        int cursorSegmentIndex;
        // Binary search to find which segment contains the cursor position
        int min = 0;
        int max = _textSegments.Count - 1;

        // Fallback: cursor is at the end, use last segment
        while (min <= max)
        {
            int mid = (min + max) / 2;
            TextSegment segment = _textSegments[mid];

            // Check if cursor is within this segment (skip newline segments with length == 0)
            // if (segment.nonSpaceCount + segment.pendingSpaceCount > 0)
            // {
            int segmentEnd = segment.startIndex + segment.CharCount;
            if (_cursorIndex >= segment.startIndex && _cursorIndex < segmentEnd)
            {
                cursorSegmentIndex = mid;
                return cursorSegmentIndex;
            }
            // }

            if (_cursorIndex < segment.startIndex)
            {
                max = mid - 1;
            }
            else
            {
                min = mid + 1;
            }
        }
        return Math.Max(0, _textSegments.Count - 1);
    }

    private float GetFinalFontSize()
    {
        float fontSize;
        if (_useTextWidth | _useTextHeight)
        {
            fontSize = _fontSize;
        }
        else
        {
            fontSize = _fontSizeScalingMode switch
            {
                FontSizeScalingMode.UseWidthRatio => _size.X / _fontSizeScalingBaseline * _fontSize,
                FontSizeScalingMode.UseHeightRatio => _size.Y / _fontSizeScalingBaseline * _fontSize,
                _ => _fontSize
            };
        }
        return Math.Abs(fontSize);
    }

    public override GreyGuiElement? GetMouseHandler()
    {
        return new Rectangle(OnScreenPos, _size.ToPoint()).Contains(GuiUpdate.Mouse.Position) ? this : null;
    }
    public override void HandleMouseEvent()
    {
        if (GuiUpdate.Mouse.IsLeftButtonDown)
        {
            if (GuiUpdate.FocusedElement != this)
                _cursorIndex = _displayText.Length;
            GuiUpdate.FocusedElement = this;
        }
    }

    public override void Draw(Point pos, RenderContext renderContext, Rectangle screenScissor)
    {
        OnScreenPos = pos;
        if (_isDisplayTextDirty)
        {
            ResolveDisplayTextDirty();
        }
        if (_isLayoutDirty)
        {
            ResolveLayoutDirty3();
        }

        renderContext.FillRect(new Rectangle(pos, _size.ToPoint()), Color.SkyBlue, default, 10, 0, screenScissor);

        float fontSize;
        if (_useTextWidth)
        {
            fontSize = _fontSize;
        }
        else
        {
            fontSize = _fontSizeScalingMode switch
            {
                FontSizeScalingMode.UseWidthRatio => _size.X / _fontSizeScalingBaseline * _fontSize,
                FontSizeScalingMode.UseHeightRatio => _size.Y / _fontSizeScalingBaseline * _fontSize,
                _ => _fontSize
            };
        }
        fontSize = Math.Abs(fontSize);
        Vector2 position = pos.ToVector2();
        position.Y += fontSize + _textYOffset;
        bool isFocused = GuiUpdate.FocusedElement == this;

        for (int i = 0; i < _textSegments.Count; ++i)
        {
            TextSegment currentSegment = _textSegments[i];
            renderContext.RenderTextUsingCharIndices(_displayTextCharIndices, currentSegment.startIndex, currentSegment.nonSpaceCount, position + _segmentOffsetCache[i], fontSize, isFocused ? FocusedColor : ColorMask, screenScissor);
        }


        // Render the cursor
        if (isFocused)
        {
            int cursorSegmentIndex = GetCursorSegmentIndex();
            Vector2 cursorPosition = _segmentOffsetCache[cursorSegmentIndex];
            TextSegment cursorSegment = _textSegments[cursorSegmentIndex];
            float originalWidth = 0;
            for (int i = cursorSegment.startIndex; i < _cursorIndex; ++i)
            {
                originalWidth += GreyGui.TextSystem.GlyphInfoList[_displayTextCharIndices[i]].AdvanceWidth;
            }
            cursorPosition.X += originalWidth * fontSize / GreyGui.TextSystem.GlyphPixelSize;
            renderContext.FillRect(new Rectangle(pos + cursorPosition.ToPoint() - new Point(3, 0), new(3, 50)), FocusedColor, Color.White, 0, 0, screenScissor);
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
        if (GuiUpdate.FocusedElement != this)
        {
            return;
        }

        ReadOnlySpan<char> inputBuffer = GuiUpdate.Keyboard.GetTextInputBuffer();

        // when using 'DisplayText = ' assignment, the _cursorIndex field will be set to _displayText.Length, hence we need to record the original index first
        int cursorIndex = _cursorIndex;
        for (int i = 0; i < inputBuffer.Length; ++i)
        {
            char c = inputBuffer[i];
            if (c == '\b') // backspace
            {
                if (cursorIndex > 0)
                {
                    DisplayText = _displayText.Remove((cursorIndex--) - 1, 1);
                }
            }
            else
            {
                DisplayText = _displayText.Insert(cursorIndex++, c.ToString());
            }
        }
        _cursorIndex = cursorIndex;

        // handle cursor positioning
        if (GuiUpdate.Keyboard.IsKeyDown(Keys.Left))
        {
            --_cursorIndex;
        }
        if (GuiUpdate.Keyboard.IsKeyDown(Keys.Right))
        {
            ++_cursorIndex;
        }
        _cursorIndex = Math.Clamp(_cursorIndex, 0, _displayText.Length);

    }

    private struct TextSegment
    {
        public float WidthWithSpace => width + pendingSpaceWidth;
        public int CharCount => nonSpaceCount + pendingSpaceCount;
        public int startIndex;
        public ushort nonSpaceCount;
        public ushort pendingSpaceCount;
        public float width;
        public float pendingSpaceWidth;
    }
}