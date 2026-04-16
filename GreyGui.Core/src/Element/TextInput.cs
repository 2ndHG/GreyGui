using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GreyGui;

public class TextInput : GreyGuiElement, IRatioElement, IFocusable
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
            _finalSize = _size = value;
            _isSizeDirty = true;
        }
    }
    public override Vector2 FinalSize
    {
        get => _finalSize;
    }
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

                _isSizeDirty = true;
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
            _isSizeDirty = true;
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
            _isSizeDirty = true;
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

            _isSizeDirty = true;
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
            _isSizeDirty = true;
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

            _isSizeDirty = true;
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

            _isSizeDirty = true;
        }
    }
    public Color FocusedColor { get; set; } = Color.Yellow;

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

    // events
    public event Action<TextInput>? OnBlurred;
    public event Action<TextInput>? OnClicked;
    public event Action<TextInput, string>? OnTextChanged;

    // cursor
    private int _cursorIndex = 0;
    private double _cursorBlinkFactor;
    private double _directionKeyHoldingTime;
    private float fastMoving = 0;
    private Keys _lastPressedDirectionKey;
    private Action _lastCursorMoveAction;
    readonly private Keys[] _cursorMoveKeys;
    readonly private Action[] _cursorMoveMethods = new Action[4];

    // Layout
    private readonly List<TextSegment> _textSegments = [];
    private readonly List<int> _displayTextCharIndices = [];
    private readonly List<Vector2> _segmentOffsetCache = [];
    private bool _autoEndLine;
    private int _rowCount = 1;
    private float _maxWidth = 0;

    public TextInput(Color? colorMask = null, Color? borderColor = null, Color? backgroundColor = null, Vector2 size = default, int borderRadius = default, int borderWidth = default,
    TextWidthMode widthMode = TextWidthMode.Fixed, TextHeightMode heightMode = TextHeightMode.Fixed, float widthRatio = default, float heightRatio = default, float heightWidthRatio = default, int zIndex = default, TextAlignment alignMode = TextAlignment.Left, string? fontName = null, string displayText = "", float fontSize = -1f, float textYOffset = default, FontSizeScalingMode fontSizeScalingMode = FontSizeScalingMode.None, float fontSizeScalingBaseline = 0, bool autoEndLine = default, Color? focusedColor = null, Action<TextInput>? onClicked = null, Action<TextInput>? onBlurred = null, Action<TextInput, string>? onTextChanged = null)
    {
        ColorMask = colorMask ?? Color.Black;
        BackgroundColor = backgroundColor ?? Color.Transparent;
        BorderColor = borderColor ?? Color.Transparent;
        BorderWidth = borderWidth;
        BorderRadius = borderRadius;
        FocusedColor = focusedColor ?? Color.White;
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
        OnBlurred = onBlurred;
        OnClicked = onClicked;
        OnTextChanged = onTextChanged;

        _isSizeDirty = true;
        _isDisplayTextDirty = true;

        // cursor moving function initialization
        _lastCursorMoveAction = CursorMovePlaceHolder;
        _cursorMoveKeys = [Keys.Up, Keys.Down, Keys.Left, Keys.Right];
        _cursorMoveMethods = [CursorMoveUp, CursorMoveDown, CursorMoveLeft, CursorMoveRight];
    }
    public void TriggerOnBlurred()
    {
        OnBlurred?.Invoke(this);
    }
    public void TriggerOnFocused()
    {
        OnClicked?.Invoke(this);
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

        // Console.WriteLine("ResolveLayoutDirtyUseTextWidth");
        // Console.WriteLine($"finalFontSize: {fontSize}");

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

    private int GetCharIndexAtMouse(Point mousePosition)
    {
        float fontSize = GetFinalFontSize();
        float scale = fontSize / GreyGui.TextSystem.GlyphPixelSize;
        if (mousePosition.Y < OnScreenPos.Y)
        {
            return 0;
        }
        else if (mousePosition.Y > OnScreenPos.Y + _segmentOffsetCache[^1].Y + fontSize)
        {
            return _displayText.Length;
        }

        // returns -1 when behind, 1 when ahead, 0 when within
        int AheadOrBehindMouse(int segmentIndex)
        {
            Point segScreenPos = OnScreenPos + _segmentOffsetCache[segmentIndex].ToPoint();
            if (mousePosition.Y < segScreenPos.Y)
            {
                return -1;
            }
            else if (mousePosition.Y > segScreenPos.Y + fontSize)
            {
                return 1;
            }

            if (mousePosition.X < segScreenPos.X)
            {
                return -1;
            }
            else if (mousePosition.X >= segScreenPos.X + _textSegments[segmentIndex].WidthWithSpace * scale)
            {
                return 1;
            }
            return 0;
        }

        // Binary search to find the segment index
        int left = 0;
        int right = _segmentOffsetCache.Count - 1;
        int mid = 0;
        int segmentIndex = -1;
        while (left <= right)
        {
            mid = left + (right - left) / 2;
            int comparison = AheadOrBehindMouse(mid);
            if (comparison == 0)
            {
                segmentIndex = mid;
                break;
            }
            else if (comparison < 0)
            {
                right = mid - 1;
            }
            else
            {
                left = mid + 1;
            }
        }

        if (segmentIndex == -1)
        {
            // The mouse is between left and right but not overlapped with any of them
            // There are 2 situation: left and right at the same row or not
            // We always choose right if the mouse is at right's row
            (left, right) = (right, left);
            right = Math.Clamp(right, 0, _textSegments.Count - 1);
            if (mousePosition.Y >= OnScreenPos.Y + _segmentOffsetCache[right].Y)
                segmentIndex = right;
            else
                segmentIndex = left;
        }
        segmentIndex = Math.Clamp(segmentIndex, 0, _textSegments.Count - 1);

        // Use the segment information to find the exact character
        TextSegment segment = _textSegments[segmentIndex];
        float currentX = OnScreenPos.X + _segmentOffsetCache[segmentIndex].X;

        for (int i = segment.startIndex; i < segment.startIndex + segment.CharCount; i++)
        {
            int charIndex = _displayTextCharIndices[i];
            float charWidth = GreyGui.TextSystem.GlyphInfoList[charIndex].AdvanceWidth * scale;

            if (mousePosition.X < currentX + charWidth / 2)
            {
                return i;
            }

            currentX += charWidth;
        }

        // If no exact match, set cursor index to the end of the segment
        return segment.startIndex + segment.nonSpaceCount;
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

        // Console.WriteLine($"{Size.X}");
        _isSizeDirty = false;
    }
    private void ParseText()
    {
        FontInfo fontInfo = GreyGui.TextSystem.GetFontInfo(_fontName);
        // float spaceAdvanceWidth = fontInfo.GlyphInfoMap.TryGetValue(' ', out var spaceGlyphInfo) ?
        //     spaceGlyphInfo.AdvanceWidth :
        //     GreyGui.TextSystem.GlyphPixelSize / 4; 
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

        _fontInfoVersion = GreyGui.TextSystem.FontInfoVersion;
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
    private Vector2 GetCursorCharOffset(float fontSize)
    {
        int cursorSegmentIndex = GetCursorSegmentIndex();
        Vector2 result = _segmentOffsetCache[cursorSegmentIndex];
        TextSegment cursorSegment = _textSegments[cursorSegmentIndex];
        float originalWidth = 0;
        for (int i = cursorSegment.startIndex; i < _cursorIndex; ++i)
        {
            originalWidth += GreyGui.TextSystem.GlyphInfoList[_displayTextCharIndices[i]].AdvanceWidth;
        }
        result.X += originalWidth * fontSize / GreyGui.TextSystem.GlyphPixelSize;
        return result;
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

    public override GreyGuiElement? GetMouseHandler()
    {
        Rectangle selfRect = new(OnScreenPos, _finalSize.ToPoint());
        Rectangle lastAppliedScissor = LastScissor;
        Rectangle.Intersect(ref selfRect, ref lastAppliedScissor, out Rectangle detectingRect);
        return detectingRect.Contains(GuiUpdate.Mouse.Position) ? this : null;
    }
    public override void HandleMouseEvent()
    {
        if (GuiUpdate.Mouse.IsLeftButtonDown)
        {
            if (GuiUpdate.FocusedElement != this)
            {
                _cursorIndex = _displayText.Length;
            }
            else
            {
                _cursorIndex = GetCharIndexAtMouse(GuiUpdate.Mouse.Position);
            }
            _cursorBlinkFactor = .5;
            GuiUpdate.FocusedElement = this;

        }
    }

    public override void Draw(Point pos, RenderContext renderContext, Rectangle screenScissor)
    {
        OnScreenPos = pos;
        LastScissor = screenScissor;
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
        bool isFocused = GuiUpdate.FocusedElement == this;

        for (int i = 0; i < _textSegments.Count; ++i)
        {
            TextSegment currentSegment = _textSegments[i];
            renderContext.RenderTextUsingCharIndices(_displayTextCharIndices, currentSegment.startIndex, currentSegment.nonSpaceCount, position + _segmentOffsetCache[i], fontSize, isFocused ? FocusedColor : ColorMask, screenScissor);
        }


        // Render the cursor
        if (!isFocused)
            return;
        _cursorBlinkFactor += renderContext.ElapsedTimeSecond;
        if (_cursorBlinkFactor > 1)
        {
            _cursorBlinkFactor = 0;
        }
        else if (_cursorBlinkFactor >= .5)
        {
            int cursorWidth = Math.Max(1, (int)(fontSize / 10));
            Vector2 cursorPosition = GetCursorCharOffset(fontSize);
            cursorPosition.Y += _textYOffset;
            renderContext.FillRect(new Rectangle(pos + cursorPosition.ToPoint() - new Point(cursorWidth, 0), new(cursorWidth, (int)fontSize)), FocusedColor, Color.White, 0, 0, screenScissor);
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
        if (GuiUpdate.FocusedElement != this || _isDisplayTextDirty)
        {
            return;
        }
        if (GuiUpdate.Mouse.IsLeftButtonDown && GetMouseHandler() == null)
        {
            GuiUpdate.FocusedElement = null;
        }

        ReadOnlySpan<char> inputBuffer = GuiUpdate.Keyboard.GetTextInputBuffer();
        bool displayTextChanged = false;

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
                    displayTextChanged = true;
                }
            }
            else
            {
                DisplayText = _displayText.Insert(cursorIndex++, c.ToString());
                displayTextChanged = true;
            }
            _cursorBlinkFactor = .5;
        }
        _cursorIndex = cursorIndex;

        // handle cursor moving via keyboard arrow keys
        for (int i = 0; i < 4; ++i)
        {
            if (GuiUpdate.Keyboard.IsKeyDown(_cursorMoveKeys[i]))
            {
                _lastPressedDirectionKey = _cursorMoveKeys[i];
                _lastCursorMoveAction = _cursorMoveMethods[i];
                _lastCursorMoveAction.Invoke();
                _cursorBlinkFactor = .5;
                _directionKeyHoldingTime = 0;
            }
        }

        if (GuiUpdate.Keyboard.IsKeyHold(_lastPressedDirectionKey))
        {
            _cursorBlinkFactor = .5;
            _directionKeyHoldingTime += GuiUpdate.ElapsedTimeSecond;
            if (_directionKeyHoldingTime > .5f)
            {
                fastMoving += (float)GuiUpdate.ElapsedTimeSecond * 16;
                while (fastMoving > 1)
                {
                    _lastCursorMoveAction.Invoke();
                    fastMoving -= 1;
                }
            }
        }
        else
        {
            _directionKeyHoldingTime = 0;
            fastMoving = 0;
        }
        _cursorIndex = Math.Clamp(_cursorIndex, 0, _displayText.Length);

        if (displayTextChanged)
        {
            OnTextChanged?.Invoke(this, _displayText);
        }
    }

    private void CursorMoveUp()
    {
        CursorMoveVertical(-1);
    }
    private void CursorMoveDown()
    {
        CursorMoveVertical(1);
    }
    private void CursorMoveLeft()
    {
        _cursorIndex = Math.Max(_cursorIndex - 1, 0);
    }
    private void CursorMoveRight()
    {
        _cursorIndex = Math.Min(_cursorIndex + 1, _displayText.Length);
    }
    private void CursorMovePlaceHolder()
    {
    }
    private void CursorMoveVertical(int direction)
    {
        float fontSize = GetFinalFontSize();
        Vector2 cursorOffset = GetCursorCharOffset(fontSize);
        cursorOffset.Y += fontSize * (0.5f + direction);

        // Need to plus onScreenPos because GetCharIndexAtMouse uses last onScreen pos to calculate char index
        _cursorIndex = GetCharIndexAtMouse(cursorOffset.ToPoint() + OnScreenPos);
    }
}