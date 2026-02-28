using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GreyGui;

public class Button : GreyGuiElement, IContainer, IRatioElement
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


    public Vector2 ContainerSize { get => _containerSize - new Vector2(PaddingSide * 2, PaddingVertical * 2); }

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

    public int PaddingVertical { get; set; }
    public int PaddingSide { get; set; }
    public bool IsLayoutDirty { get => _isLayoutDirty; set => _isLayoutDirty = value; }
    public bool IsChildrenZIndexDirty { get => _isChildrenZIndexDirty; set => _isChildrenZIndexDirty = value; }
    public ReadOnlySpan<GreyGuiElement> Children => CollectionsMarshal.AsSpan(_children);
    public Action<Button, Point, RenderContext, Rectangle> DrawMethod { get; set; }
    public GreyGuiButtonState State => _state;
    public Texture2D ImageTexture { get => _imageTexture; set => _imageTexture = value; }
    public Rectangle ImageSrcRect { get => _srcRect; set => _srcRect = value; }
    public event Action? OnLeftClicked;
    public event Action? OnRightClicked;


    protected bool _useWidthRatio;
    protected bool _useHeightRatio;
    protected bool _useHeightWidthRatio;
    protected float _widthRatio;
    protected float _heightRatio;
    protected float _heightWidthRatio;
    protected Vector2 _size;
    protected int _zIndex;
    protected Vector2 _containerSize;
    protected bool _isLayoutDirty;
    protected bool _isChildrenZIndexDirty;
    protected List<GreyGuiElement> _children = [];
    protected List<Point> _childrenPosition = [];
    protected List<int> _drawOrder = [];
    protected int _hoveredFrame;
    protected int _holdingFrames;
    protected int _pressedFrame;

    protected GreyGuiButtonState _state = GreyGuiButtonState.Normal;

    // Render
    protected Texture2D _imageTexture = GreyGui.Atlas;
    protected Rectangle _srcRect;

    public Button(Color colorMask, Color borderColor = default, Vector2 size = default, bool useWidthRatio = default, bool useHeightRatio = default, bool useHeightWidthRatio = default, float widthRatio = default, float heightRatio = default, float heightWidthRatio = default, int zIndex = default, int paddingVertical = 0, int paddingSide = 0, int borderRadius = 0, int borderWidth = 0, Texture2D? imageTexture = null, Rectangle imageSrcRect = default)
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
        _imageTexture = imageTexture == null ? GreyGui.Atlas : imageTexture;
        _srcRect = (imageTexture, imageSrcRect.IsEmpty) switch
        {
            (null, _) => new Rectangle(0, 0, 1, 1),
            (not null, true) => imageTexture.Bounds,
            (not null, false) => imageSrcRect
        };

        PaddingVertical = paddingVertical;
        PaddingSide = paddingSide;
        BorderRadius = borderRadius;
        BorderWidth = borderWidth;

        DrawMethod = BasicDraw;

        _isSizeDirty = true;
    }


    public void AppendChild(GreyGuiElement child)
    {
        RemoveAllChildren();
        child.Parent?.RemoveChild(child);
        _children.Add(child);
        child.ChangeParentButParentWillNotKnow(this);

        _isChildrenZIndexDirty = true;
        _isLayoutDirty = true;
    }
    public void RemoveChild(GreyGuiElement child)
    {
        if (_children.Count > 0 && _children[0] == child)
        {
            child.ChangeParentButParentWillNotKnow(null);
        }
        _children.Clear();
    }

    public void RemoveAllChildren()
    {
        if (_children.Count > 0)
        {
            _children[0].ChangeParentButParentWillNotKnow(null);
        }
        _children.Clear();
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

        // As an IContainer
        _containerSize = _size - new Vector2(BorderRadius * (Constant.SQRT2 - 1) * 2);
        foreach (GreyGuiElement child in _children)
        {
            child.IsSizeDirty = true;
        }

        _isSizeDirty = false;
        _isLayoutDirty = _isLayoutDirty || sizeChanged;
    }

    public override void Update()
    {
        _state = GreyGuiButtonState.Normal;
        if (GuiUpdate.FrameId == _hoveredFrame)
        {
            _state |= GreyGuiButtonState.Hovered;
        }
        if (GuiUpdate.FrameId <= _pressedFrame + _holdingFrames)
        {
            _state |= GreyGuiButtonState.Active;
        }
        else
        {
            _holdingFrames = 0;
        }
    }

    public override void Draw(Point position, RenderContext renderContext, Rectangle screenScissor)
    {
        DrawMethod.Invoke(this, position, renderContext, screenScissor);
    }


    public override GreyGuiElement? GetMouseHandler()
    {
        return new Rectangle(OnScreenPos, _size.ToPoint()).Contains(GuiUpdate.Mouse.Position) ? this : null;
    }
    public override void HandleMouseEvent()
    {
        _hoveredFrame = GuiUpdate.FrameId;
        if (GuiUpdate.Mouse.IsLeftButtonDown)
        {
            GuiUpdate.FocusedElement = this;
            _pressedFrame = GuiUpdate.FrameId;
        }
        if (GuiUpdate.Mouse.IsLeftHold)
        {
            _holdingFrames++;
        }
        if (GuiUpdate.Mouse.IsLeftButtonUp && _state.HasFlag(GreyGuiButtonState.Active))
        {
            OnLeftClicked?.Invoke();
        }
    }

    public void DrawChildren(Point selfPosition, RenderContext context, Rectangle screenScissor)
    {
        if (_children.Count == 0)
        {
            return;
        }
        GreyGuiElement child = _children[0];
        child.ResolveSizeDirty();
        Point childPosition = selfPosition + new Point(PaddingSide, PaddingVertical) + ((ContainerSize - child.Size) / 2).ToPoint();

        // context.FillRect(new(childPosition, new(50, 50)), Color.Blue, Color.Blue, 0, 0, GreyGui.Atlas, screenScissor);
        child.Draw(childPosition, context, screenScissor);
        if (child is IContainer container)
        {
            container.DrawChildren(childPosition, context, screenScissor);
        }
    }
    public static void BasicDraw(Button button, Point position, RenderContext renderContext, Rectangle screenScissor)
    {
        button.OnScreenPos = position;
        Color colorMask = button.ColorMask;
        float scale = button.State switch
        {
            GreyGuiButtonState.Hovered => 1.2f,
            GreyGuiButtonState.Active => 0.8f,
            _ => 1f
        };
        colorMask = new Color(colorMask * scale, colorMask.A);
        Color borderColor = button.BorderColor;
        scale = button.State switch
        {
            GreyGuiButtonState.Hovered => 1.2f,
            GreyGuiButtonState.Active => 0.8f,
            GreyGuiButtonState.Selected => 1.2f,
            _ => 1f
        };
        borderColor = new Color(borderColor * scale, borderColor.A);
        renderContext.FillRect(
            new Rectangle(position, button.Size.ToPoint()),
            colorMask,
            borderColor,
            button.BorderRadius,
            button.BorderWidth,
            screenScissor
        );
    }
}