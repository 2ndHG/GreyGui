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
            if (value == null || _fontName.Equals(value))
                return;
            _fontName = value;
            _isDisplayTextDirty = true;
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

    private Vector2 _size;
    private int _zIndex;
    private bool _useWidthRatio;
    private bool _useHeightRatio;
    private bool _useHeightWidthRatio;
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
    private float _textTotalWidth;

    private List<GlyphInfo> _textGlyphList = [];
    private List<TextSegment> _textSegments = [];

    public Text(Color colorMask, Color borderColor = default, Vector2 size = default, bool useWidthRatio = default, bool useHeightRatio = default, bool useHeightWidthRatio = default, float widthRatio = default, float heightRatio = default, float heightWidthRatio = default, int zIndex = default, RowLayoutMode alignMode = RowLayoutMode.Left, string? fontName = null, string displayText = "", float fontSize = 24f, float textYOffset = default, FontSizeScalingMode fontSizeScalingMode = FontSizeScalingMode.None, float fontSizeScalingBaseline = 0)
    {
        ColorMask = colorMask;
        BorderColor = borderColor;
        _size = size;
        _useWidthRatio = useWidthRatio;
        _useHeightRatio = useHeightRatio;
        _useHeightWidthRatio = useHeightWidthRatio;
        _widthRatio = widthRatio;
        _heightRatio = heightRatio;
        _heightWidthRatio = heightWidthRatio;
        _zIndex = zIndex;
        _alignMode = alignMode;
        _fontName = fontName ?? GreyGui.TextSystem.DefaultFont;
        _displayText = displayText;
        _fontSize = fontSize;
        _textYOffset = textYOffset;
        _fontSizeScalingMode = fontSizeScalingMode;

        // priority: user input > FontSizeScalingMode > default
        _fontSizeScalingBaseline = (fontSizeScalingBaseline, fontSizeScalingMode, size.X, size.Y) switch
        {
            ( > 0, _, _, _) => fontSizeScalingBaseline,
            ( <= 0.1f, FontSizeScalingMode.UseWidthRatio, > 0, _) => size.X,
            ( <= 0.1f, FontSizeScalingMode.UseHeightRatio, _, > 0) => size.Y,
            _ => fontSize,
        };

        _isSizeDirty = true;
        _isDisplayTextDirty = true;
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

        if (sizeChanged && _parent is not null)
        {
            _parent.IsLayoutDirty = true;
        }
        _isSizeDirty = false;
    }

    private void ParseTextSpaceAsNotWord()
    {
        FontInfo fontInfo = GreyGui.TextSystem.GetFontInfo(_fontName);
        float spaceAdvanceWidth = fontInfo.GlyphInfoMap[' '].AdvanceWidth;
        TextSegment textSegment = new();
        _textTotalWidth = 0;
        for (int i = 0; i < _displayText.Length; ++i)
        {
            if (_displayText[i] == ' ')
            {
                _textTotalWidth += spaceAdvanceWidth;
                if (textSegment.glyphInfoList.Count > 0)
                {
                    textSegment.segmentAdvanceWidth += spaceAdvanceWidth;
                    _textSegments.Add(textSegment);
                    textSegment = new();
                }
                else
                {
                    _textSegments[^1].segmentAdvanceWidth += spaceAdvanceWidth;
                }
            }
            else
            {
                GlyphInfo glyphInfo = fontInfo.GlyphInfoMap[_displayText[i]];
                _textTotalWidth += glyphInfo.AdvanceWidth;
                textSegment.segmentWidth += glyphInfo.AdvanceWidth;
                textSegment.segmentAdvanceWidth += glyphInfo.AdvanceWidth;
                textSegment.glyphInfoList.Add(glyphInfo);
            }
        }
        if (textSegment.glyphInfoList.Count > 0)
        {
            _textSegments.Add(textSegment);
        }
    }
    private void ResolveDisplayTextDirty()
    {
        ParseTextSpaceAsNotWord();
        _isDisplayTextDirty = false;
    }
    public override void Draw(Point position, RenderContext renderContext, Rectangle screenScissor)
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
        Vector2 cursorPosition = position.ToVector2();
        cursorPosition.Y += fontSize + _textYOffset;

        float scale = fontSize / GreyGui.TextSystem.GlyphPixelSize;
        cursorPosition.X += _alignMode switch
        {
            RowLayoutMode.Center => (_size.X - _textTotalWidth * scale) / 2,
            RowLayoutMode.Right => _size.X - _textTotalWidth * scale,
            _ => 0
        };
        foreach (TextSegment segment in _textSegments)
        {
            renderContext.RenderText(segment.glyphInfoList, cursorPosition, fontSize, ColorMask, screenScissor);
            cursorPosition.X += segment.segmentAdvanceWidth * scale;
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
        public List<GlyphInfo> glyphInfoList = [];
        public float segmentWidth;
        public float segmentAdvanceWidth;
    }
}