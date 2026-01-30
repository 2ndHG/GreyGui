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
            _fontSize = value;
            _isFontSizeDirty = true;
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
    private string _fontName = GreyGui.TextSystem.DefaultFont;
    private string _displayText = "";
    private float _fontSize = 24f;
    private bool _isDisplayTextDirty;
    private bool _isFontSizeDirty;
    private Vector2 _textPosition;

    private UiVertex[] _drawVertices = [];
    private int[] _drawVertexIndex = [];
    private List<GlyphInfo> _textGlyphList = [];

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

        (_drawVertices, _drawVertexIndex) = UiVertexHelper.GenerateTextVertices(_textGlyphList, _textPosition, ColorMask, _fontSize);
        _isDisplayTextDirty = false;
    }
    private void ResolveFontSizeDirty()
    {
        float scale = _fontSize / GreyGui.TextSystem.GlyphPixelSize;
        Vector2 cursor = _textPosition;
        int vertexCount = 0;
        ReadOnlySpan<GlyphInfo> textGlyphSpan = CollectionsMarshal.AsSpan(_textGlyphList);
        for (int i = 0; i < textGlyphSpan.Length; i++)
        {
            // pursuing ultimate performance, avoiding any value copying
            ref readonly GlyphInfo glyphInfo = ref textGlyphSpan[i];
            Vector2 finalSize = glyphInfo.SrcRect.Size.ToVector2() * scale;
            Vector4 rectParams = new(finalSize.X, finalSize.Y, _fontSize, -1);

            (float left, float top) = cursor - glyphInfo.Origin * scale;
            float right = left + finalSize.X;
            float bottom = top + finalSize.Y;

            _drawVertices[vertexCount].RectParams = rectParams;
            _drawVertices[vertexCount++].Position = new(left, top, 0);

            _drawVertices[vertexCount].RectParams = rectParams;
            _drawVertices[vertexCount++].Position = new(right, top, 0);

            _drawVertices[vertexCount].RectParams = rectParams;
            _drawVertices[vertexCount++].Position = new(right, bottom, 0);

            _drawVertices[vertexCount].RectParams = rectParams;
            _drawVertices[vertexCount++].Position = new(left, bottom, 0);

            cursor.X += glyphInfo.AdvanceWidth * scale;
        }
    }

    public override void Draw(Point position, RenderContext renderContext, Rectangle screenScissor)
    {
        if (_isDisplayTextDirty)
        {
            ResolveDisplayTextDirty();
        }
        renderContext.AddCommand(GreyGui.Atlas, _drawVertices, _drawVertexIndex, screenScissor);
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