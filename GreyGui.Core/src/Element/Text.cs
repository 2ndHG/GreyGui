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
            if (_fontSize == value)
                return;
            if (_useAutoTextOffset)
            {
                _textOffset = new Vector2(0, value);
            }
            _fontSize = value;
        }
    }
    public Vector2 TextOffset
    {
        get => _textOffset;
        set
        {
            if (_textOffset == value || _useAutoTextOffset)
                return;
            _textOffset = value;
        }
    }
    public bool UseAutoTextOffset
    {
        get => _useAutoTextOffset;
        set
        {
            if (_useAutoTextOffset == value)
                return;
            _useAutoTextOffset = value;
            if (_useAutoTextOffset)
            {
                TextOffset = new Vector2(0, _fontSize);
            }
        }
    }
    public float FontSizeScalingBaseline
    {
        get => _fontSizeScalingBaseline;
        set
        {
            if (value <= 0)
                return;
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
    private RowLayoutMode _wordLayoutMode;
    private string _fontName = GreyGui.TextSystem.DefaultFont;
    private string _displayText;
    private float _fontSize;
    private bool _isDisplayTextDirty;
    private bool _useAutoTextOffset = true;
    private Vector2 _textOffset;
    private FontSizeScalingMode _fontSizeScalingMode;
    private float _fontSizeScalingBaseline;

    private List<GlyphInfo> _textGlyphList = [];
    public Text(Color colorMask, Color borderColor = default, Vector2 size = default, bool useWidthRatio = default, bool useHeightRatio = default, bool useHeightWidthRatio = default, float widthRatio = default, float heightRatio = default, float heightWidthRatio = default, int zIndex = default, RowLayoutMode wordLayoutMode = RowLayoutMode.Left, string? fontName = null, string displayText = "", float fontSize = 24f, bool useAutoTextOffset = true, Vector2 textOffset = default, FontSizeScalingMode fontSizeScalingMode = FontSizeScalingMode.None, float fontSizeScalingBaseline = 0)
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
        _wordLayoutMode = wordLayoutMode;
        _fontName = fontName ?? GreyGui.TextSystem.DefaultFont;
        _displayText = displayText;
        _fontSize = fontSize;
        _useAutoTextOffset = useAutoTextOffset;
        _textOffset = useAutoTextOffset ? new Vector2(0, fontSize) : textOffset;
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
    private void ResolveDisplayTextDirty()
    {
        _textGlyphList.Clear();
        FontInfo fontInfo = GreyGui.TextSystem.GetFontInfo(_fontName);
        foreach (char c in _displayText)
        {
            _textGlyphList.Add(fontInfo.GlyphInfoMap[c]);
        }
        _isDisplayTextDirty = false;
    }
    public override void Draw(Point position, RenderContext renderContext, Rectangle screenScissor)
    {
        if (_isDisplayTextDirty)
        {
            ResolveDisplayTextDirty();
        }

        float sizeX = Math.Max(_size.X, 0);
        float sizeY = Math.Max(_size.Y, 0);
        float fontSize = _fontSizeScalingMode switch
        {
            FontSizeScalingMode.UseWidthRatio => sizeX / _fontSizeScalingBaseline * _fontSize,
            FontSizeScalingMode.UseHeightRatio => sizeY / _fontSizeScalingBaseline * _fontSize,
            _ => _fontSize
        };
        if (_useAutoTextOffset)
        {
            _textOffset = new Vector2(0, fontSize);
        }
        renderContext.RenderText(_textGlyphList, position.ToVector2() + _textOffset, fontSize, ColorMask, screenScissor);
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