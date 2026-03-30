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
            if (value == _useHeightRatio)
                return;
            _isSizeDirty = true;
            _useHeightRatio = value;
        }
    }
    public bool UseHeightWidthRatio
    {
        get => _useHeightWidthRatio;
        set
        {
            if (_useHeightWidthRatio == value)
                return;
            _isSizeDirty = true;
            _useHeightWidthRatio = value;
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

    protected bool _useWidthRatio;
    protected bool _useHeightRatio;
    protected bool _useHeightWidthRatio;
    protected float _widthRatio;
    protected float _heightRatio;
    protected float _heightWidthRatio;
    protected Vector2 _size;
    protected int _zIndex;


    // Render
    protected Texture2D _imageTexture;
    protected Rectangle _imageSrcRect;

    public Image(Color? colorMask = null, Color borderColor = default, Vector2 size = default, bool useWidthRatio = default, bool useHeightRatio = default, bool useHeightWidthRatio = default, float widthRatio = default, float heightRatio = default, float heightWidthRatio = default, int zIndex = default, int borderRadius = 0, int borderWidth = 0, Texture2D? imageTexture = null, Rectangle imageSrcRect = default)
    {
        ColorMask = (colorMask, imageTexture) switch
        {
            (not null, _) => (Color)colorMask,
            (null, not null) => Color.White,
            _ => Color.Gray
        };
        BorderColor = borderColor;
        _size = size;
        _useWidthRatio = useWidthRatio;
        _useHeightRatio = useHeightRatio;
        _useHeightWidthRatio = useHeightWidthRatio;
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
        // As an IRatioElement
        bool sizeChanged = false;
        if (UseWidthRatio && _parent != null)
        {
            _size.X = _parent.ContainerSize.X * _widthRatio;
            sizeChanged = true;
        }
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

    public override void Update()
    {
    }

    public override void Draw(Point position, RenderContext renderContext, Rectangle screenScissor)
    {
        renderContext.RenderTexture(
            _imageTexture,
            new Rectangle(position, _size.ToPoint()),
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
        return new Rectangle(OnScreenPos, _size.ToPoint()).Contains(GuiUpdate.Mouse.Position) ? this : null;
    }
    public override void HandleMouseEvent()
    {
    }
}