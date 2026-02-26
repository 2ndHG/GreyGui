using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

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

    public int HoveredFrame { get; set; }
    public int ActiveFrame { get; set; }
    public ReadOnlySpan<GreyGuiElement> Children => CollectionsMarshal.AsSpan(_children);
    public Action<Button, Point, RenderContext, Rectangle> DrawMethod { get; set; }

    private bool _useWidthRatio;
    private bool _useHeightRatio;
    private bool _useHeightWidthRatio;
    private float _widthRatio;
    private float _heightRatio;
    private float _heightWidthRatio;
    private Vector2 _size;
    private int _zIndex;
    private Vector2 _containerSize;
    private bool _isLayoutDirty;
    private bool _isChildrenZIndexDirty;
    private List<GreyGuiElement> _children = [];
    private List<Point> _childrenPosition = [];
    private List<int> _drawOrder = [];
    // Button special
    public GreyGuiButtonState State
    {
        get => _state;
    }



    private GreyGuiButtonState _state = GreyGuiButtonState.Normal;



    public Button()
    {
        DrawMethod = BasicDraw;
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
        if (GreyGuiUpdate.FrameId == HoveredFrame)
        {
            _state |= GreyGuiButtonState.Hovered;
        }
        if (GreyGuiUpdate.FrameId == ActiveFrame)
        {
            _state |= GreyGuiButtonState.Active;
        }
    }

    public override void Draw(Point position, RenderContext renderContext, Rectangle screenScissor)
    {
        DrawMethod.Invoke(this, position, renderContext, screenScissor);
    }

    public override bool HandleMouseEvent(ref MouseState mouseState)
    {
        bool result = new Rectangle(OnScreenPos, _size.ToPoint()).Contains(mouseState.Position);
        if (result)
        {
            HoveredFrame = GreyGuiUpdate.FrameId;
            if (mouseState.LeftButton == ButtonState.Pressed && ActiveFrame != HoveredFrame)
            {
                ActiveFrame = GreyGuiUpdate.FrameId;
            }
        }

        return result;
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
            GreyGui.Atlas,
            screenScissor
        );
    }
}