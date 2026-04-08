using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GreyGui;

public class Image : GreyGuiElement, IRatioElement
{
    public override Vector2 Size
    {
        get => _size;
        set
        {
            if (_size == value) return;

            _size = value;
            _isSizeDirty = true;
        }
    }
    public override Vector2 FinalSize => _finalSize;
    public override int ZIndex
    {
        get => _zIndex; set
        {
            if (_zIndex != value)
            {
                _zIndex = value;
                if (_parent is not null)
                {
                    _parent.IsChildrenZIndexDirty = true;
                }
            }
        }
    }

    public WidthMode WidthMode
    {
        get => _widthMode;
        set
        {
            if (_widthMode == value)
                return;
            _widthMode = value;
            _isSizeDirty = true;
        }
    }
    public HeightMode HeightMode
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

    private WidthMode _widthMode;
    private HeightMode _heightMode;
    private float _widthRatio;
    private float _heightRatio;
    private float _heightWidthRatio;
    protected Vector2 _size;
    protected Vector2 _finalSize;
    protected int _zIndex;


    // Render
    protected Texture2D _imageTexture;
    protected Rectangle _imageSrcRect;

    public Image(Color? colorMask = null, Color borderColor = default, Vector2 size = default, WidthMode widthMode = WidthMode.Fixed, HeightMode heightMode = HeightMode.Fixed, float widthRatio = default, float heightRatio = default, float heightWidthRatio = default, int zIndex = default, int borderRadius = 0, int borderWidth = 0, Texture2D? imageTexture = null, Rectangle imageSrcRect = default)
    {
        ColorMask = colorMask ?? Color.White;
        BorderColor = borderColor;
        _size = size;
        _widthMode = widthMode;
        _heightMode = heightMode;
        _widthRatio = widthRatio;
        _heightRatio = heightRatio;
        _heightWidthRatio = heightWidthRatio;
        _zIndex = zIndex;
        _imageTexture = imageTexture == null ? GreyGui.Atlas : imageTexture;
        _imageSrcRect = (imageTexture, imageSrcRect.IsEmpty) switch
        {
            (null, _) => new Rectangle(0, 0, 1, 1),
            (not null, true) => imageTexture.Bounds,
            (not null, false) => imageSrcRect
        };

        BorderRadius = borderRadius;
        BorderWidth = borderWidth;

        _isSizeDirty = true;
    }

    public override void ResolveSizeDirty()
    {
        if (!_isSizeDirty)
        {
            return;
        }

        _finalSize = _size;

        // As an IRatioElement
        bool sizeChanged = false;
        if (_widthMode == WidthMode.ParentRatio)
        {
            if (_parent == null)
            {
                _finalSize.X = GreyGui.NullParentWidth * _widthRatio;
            }
            else
            {
                _finalSize.X = _parent.ContainerSize.X * _widthRatio;
            }
            sizeChanged = true;
        }
        if (_heightMode == HeightMode.ParentRatio)
        {
            if (_parent == null)
            {
                _finalSize.Y = GreyGui.NullParentHeight * _heightRatio;
            }
            else
            {
                _finalSize.Y = _parent.ContainerSize.Y * _heightRatio;
            }
            sizeChanged = true;
        }
        else if (_heightMode == HeightMode.HeightWidthRatio)
        {
            _finalSize.Y = _finalSize.X * _heightWidthRatio;
            sizeChanged = true;
        }

        if (sizeChanged && _parent is not null)
        {
            _parent.IsLayoutDirty = true;
        }

        _isSizeDirty = false;
    }

    public override void Update()
    {
    }

    public override void Draw(Point position, RenderContext renderContext, Rectangle screenScissor)
    {
        renderContext.RenderTexture(
            _imageTexture,
            new Rectangle(position, _finalSize.ToPoint()),
            _imageSrcRect,
            ColorMask,
            BorderColor,
            BorderRadius,
            BorderWidth,
            screenScissor
        );
    }

    public override GreyGuiElement? GetMouseHandler()
    {
        return new Rectangle(OnScreenPos, _finalSize.ToPoint()).Contains(GuiUpdate.Mouse.Position) ? this : null;
    }
    public override void HandleMouseEvent()
    {
    }
}