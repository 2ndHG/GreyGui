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


    public Vector2 ContainerSize { get => _containerSize - new Vector2(PaddingSide * 2, PaddingVertical * 2); }

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

    public int PaddingVertical { get; set; }
    public int PaddingSide { get; set; }
    public bool IsLayoutDirty { get => _isLayoutDirty; set => _isLayoutDirty = value; }
    public bool IsChildrenZIndexDirty { get => _isChildrenZIndexDirty; set => _isChildrenZIndexDirty = value; }
    public ReadOnlySpan<GreyGuiElement> Children => CollectionsMarshal.AsSpan(_children);
    public Action<Button, Point, RenderContext, Rectangle> DrawMethod { get; set; }
    public GreyGuiButtonState State => _state;
    public Texture2D ImageTexture { get => _imageTexture; set => _imageTexture = value; }
    public Rectangle ImageSrcRect { get => _imageSrcRect; set => _imageSrcRect = value; }
    public event Action? OnLeftClicked;
    public event Action? OnRightClicked;


    protected WidthMode _widthMode;
    protected HeightMode _heightMode;
    protected float _widthRatio;
    protected float _heightRatio;
    protected float _heightWidthRatio;
    protected Vector2 _size;
    protected Vector2 _finalSize;
    protected int _zIndex;
    protected Vector2 _containerSize;
    protected bool _isLayoutDirty;
    protected bool _isChildrenZIndexDirty;
    protected List<GreyGuiElement> _children = [];
    protected List<Point> _childrenPosition = [];
    protected List<int> _drawOrder = [];
    protected int _hoveredFrame;
    protected int _lHoldingFrames;
    protected int _lPressedFrame;
    protected int _rHoldingFrames;
    protected int _rPressedFrame;


    protected GreyGuiButtonState _state = GreyGuiButtonState.Normal;

    // Render
    protected Texture2D _imageTexture;
    protected Rectangle _imageSrcRect;

    public Button(Color? colorMask = null, Color borderColor = default, Vector2 size = default, WidthMode widthMode = WidthMode.Fixed, HeightMode heightMode = HeightMode.Fixed, float widthRatio = default, float heightRatio = default, float heightWidthRatio = default, int zIndex = default, int paddingVertical = 0, int paddingSide = 0, int borderRadius = 0, int borderWidth = 0, Texture2D? imageTexture = null, Rectangle imageSrcRect = default, Action? onLeftClicked = null)
    {
        ColorMask = (colorMask, imageTexture) switch
        {
            (not null, _) => (Color)colorMask,
            (null, not null) => Color.White,
            _ => Color.Gray
        };
        BorderColor = borderColor;
        _size = size;
        _widthMode = widthMode;
        _heightMode = heightMode;
        _widthRatio = widthRatio;
        _heightRatio = heightRatio;
        _heightWidthRatio = heightWidthRatio;
        _zIndex = zIndex;
        _imageTexture = imageTexture == null ? GreyGuiCore.Atlas : imageTexture;
        _imageSrcRect = (imageTexture, imageSrcRect.IsEmpty) switch
        {
            (null, _) => new Rectangle(0, 0, 1, 1),
            (not null, true) => imageTexture.Bounds,
            (not null, false) => imageSrcRect
        };

        PaddingVertical = paddingVertical;
        PaddingSide = paddingSide;
        BorderRadius = borderRadius;
        BorderWidth = borderWidth;

        OnLeftClicked = onLeftClicked;

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
        _finalSize = _size;

        // As an IRatioElement
        bool sizeChanged = false;
        if (_widthMode == WidthMode.ParentRatio)
        {
            if (_parent == null)
            {
                _finalSize.X = GreyGuiCore.NullParentWidth * _widthRatio;
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
                _finalSize.Y = GreyGuiCore.NullParentHeight * _heightRatio;
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

        // As an IContainer
        _containerSize = _finalSize - new Vector2(BorderRadius * (Constant.SQRT2 - 1) * 2);
        foreach (GreyGuiElement child in _children)
        {
            child.IsSizeDirty = true;
        }

        _isSizeDirty = false;
        _isLayoutDirty = _isLayoutDirty || sizeChanged;
    }
    public Button SetChild(GreyGuiElement child)
    {
        AppendChild(child);
        return this;
    }

    public override void Update()
    {
        _state = GreyGuiButtonState.Normal;

        if (GuiUpdate.FrameId == _lPressedFrame + _lHoldingFrames || GuiUpdate.FrameId == _rHoldingFrames + _rPressedFrame)
        {
            _state = GreyGuiButtonState.Active;
        }
        else if (GuiUpdate.FrameId == _hoveredFrame)
        {
            _state = GreyGuiButtonState.Hovered;
        }
        else
        {
            _lHoldingFrames = 0;
            _rHoldingFrames = 0;
        }
    }

    public override void Draw(Point position, RenderContext renderContext, Rectangle screenScissor)
    {
        DrawMethod.Invoke(this, position, renderContext, screenScissor);
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
        _hoveredFrame = GuiUpdate.FrameId;
        if (GuiUpdate.Mouse.IsLeftButtonDown)
        {
            _lPressedFrame = GuiUpdate.FrameId;
            _lHoldingFrames = 0;
        }
        else if (GuiUpdate.Mouse.IsLeftHold)
        {
            _lHoldingFrames++;
        }
        if (GuiUpdate.Mouse.IsLeftButtonUp && _state.HasFlag(GreyGuiButtonState.Active))
        {
            // Console.WriteLine("Left Button clicked");
            OnLeftClicked?.Invoke();
        }
        if (GuiUpdate.Mouse.IsRightButtonDown)
        {
            _rPressedFrame = GuiUpdate.FrameId;
            _rHoldingFrames = 0;
        }
        else if (GuiUpdate.Mouse.IsRightHold)
        {
            _rHoldingFrames++;
        }
        if (GuiUpdate.Mouse.IsRightButtonUp && _state.HasFlag(GreyGuiButtonState.Active))
        {
            // Console.WriteLine("Right Button clicked");
            OnRightClicked?.Invoke();
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
        Point childPosition = selfPosition + new Point(PaddingSide, PaddingVertical) + ((ContainerSize - child.FinalSize) / 2 + new Vector2(BorderRadius * (Constant.SQRT2 - 1))).ToPoint();

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
        button.LastScissor = screenScissor;
        float scale = button.State switch
        {
            GreyGuiButtonState.Active => 0.8f,
            GreyGuiButtonState.Hovered => 1.2f,
            _ => 1f
        };
        Color colorMask = button.ColorMask * scale;
        colorMask.A = button.ColorMask.A;

        scale = button.State switch
        {
            GreyGuiButtonState.Active => 0.8f,
            GreyGuiButtonState.Hovered => 1.2f,
            _ => 1f
        };
        Color borderColor = button.BorderColor * scale;
        borderColor.A = button.ColorMask.A;

        renderContext.RenderTexture(
            button.ImageTexture,
            new Rectangle(position, button.FinalSize.ToPoint()),
            button.ImageSrcRect,
            colorMask,
            borderColor,
            button.BorderRadius,
            button.BorderWidth,
            screenScissor
        );
    }
}