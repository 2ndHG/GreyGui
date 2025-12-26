using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;

namespace GreyGui;

public class Panel : GreyGuiElement, IContainer, IPercentElement
{
    public override Vector2 Size
    {
        get => _size;
        set
        {
            if (_size == value)
                return;
            _size = value;
            _isSizeDirty = true;
            _isLayoutDirty = true;
        }
    }
    public Vector2 ContainerSize { get => _containerSize; }

    public bool UsePercentWidth
    {
        get => _usePercentWidth;
        set
        {
            if (_usePercentWidth == value)
                return;
            _usePercentWidth = value;
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
            _isSizeDirty = true;
            _useHeightWidthRatio = value;
        }
    }

    public float WidthPercent
    {
        get => _widthPercent;
        set
        {
            if (_widthPercent == value)
                return;
            _widthPercent = value;
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
    public int PaddingTop { get; set; }
    public int PaddingSide { get; set; }
    public bool IsLayoutDirty { get => _isLayoutDirty; set => _isLayoutDirty = value; }
    public bool IsChildrenIndexDirty { get => _isChildrenIndexDirty; set => _isChildrenIndexDirty = value; }
    public override int ZIndex
    {
        get => _zIndex; set
        {
            if (_zIndex != value)
            {
                _zIndex = value;
                if (_parent is not null)
                {
                    _parent.IsChildrenIndexDirty = true;
                }
            }
        }
    }


    private bool _usePercentWidth;
    private bool _useHeightWidthRatio;
    private float _widthPercent;
    private float _heightWidthRatio;
    private Vector2 _size;
    private int _zIndex;
    private Vector2 _containerSize;
    private bool _isLayoutDirty;
    private bool _isChildrenIndexDirty;
    private List<GreyGuiElement> _children = [];
    private Dictionary<GreyGuiElement, int> _childrenIndexMap = [];
    private List<Point> _childrenPosition = [];
    private List<int> _drawOrder = [];

    public Panel() { }
    public Panel(
        Color colorMask, Color borderColor = default, int borderRadius = default, int borderWidth = default,
        Vector2 size = default, bool usePercentWidth = default, bool useHeightWidthRatio = default, float widthPercent = default, float heightWidthRatio = default, int paddingTop = default, int paddingSide = default, int zIndex = default, ICollection<GreyGuiElement>? children = null)
    {
        ColorMask = colorMask;
        BorderColor = borderColor;
        BorderRadius = borderRadius;
        BorderWidth = borderWidth;
        _size = size;
        _usePercentWidth = usePercentWidth;
        _useHeightWidthRatio = useHeightWidthRatio;
        _widthPercent = widthPercent;
        _heightWidthRatio = heightWidthRatio;
        PaddingTop = paddingTop;
        PaddingSide = paddingSide;
        _zIndex = zIndex;
        if (children != null)
        {
            AppendChildren(children);
        }
    }

    public void AppendChild(GreyGuiElement child)
    {
        child.Parent?.RemoveChild(child);
        if (_childrenIndexMap.TryAdd(child, _children.Count))
        {
            _children.Add(child);
            child.ChangeParentButParentWillNotKnow(this);

            _isChildrenIndexDirty = true;
            _isLayoutDirty = true;
        }
    }

    public void AppendChildren(ICollection<GreyGuiElement> children)
    {
        foreach (GreyGuiElement child in children)
        {
            AppendChild(child);
        }
    }
    public void RemoveChild(GreyGuiElement child)
    {
        if (_childrenIndexMap.Remove(child))
        {
            _children.Remove(child);
            child.ChangeParentButParentWillNotKnow(null);

            _isChildrenIndexDirty = true;
            _isLayoutDirty = true;
        }
    }

    public void RemoveChildren(ICollection<GreyGuiElement> children)
    {
        foreach (GreyGuiElement child in children)
        {
            RemoveChild(child);
        }
    }


    public void RemoveAllChildren()
    {
        foreach (GreyGuiElement child in _children)
        {
            RemoveChild(child);
        }
        _isLayoutDirty = true;
        _isChildrenIndexDirty = true;
    }

    public Panel SetChildren(ICollection<GreyGuiElement> children)
    {
        RemoveAllChildren();
        AppendChildren(children);
        return this;
    }

    public override void ResolveSizeDirty()
    {
        if (_isSizeDirty)
        {
            RecalculateSize();
        }
        foreach (GreyGuiElement child in _children)
        {
            child.ResolveSizeDirty();
        }
    }

    private void RecalculateSize()
    {
        Console.WriteLine("Resolve Size");
        // As an IPercentElement
        bool sizeChanged = false;
        if (UsePercentWidth && _parent != null)
        {
            _size.X = _parent.ContainerSize.X * _widthPercent;
            sizeChanged = true;
        }
        if (UseHeightWidthRatio)
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
        _containerSize.X -= PaddingSide * 2;
        _containerSize.Y -= PaddingTop;
        foreach (GreyGuiElement child in _children)
        {
            child.IsSizeDirty = true;
        }
        _isSizeDirty = false;
        _isLayoutDirty = _isLayoutDirty || sizeChanged;
    }

    public override void Update()
    {

    }

    // render context not implemented yet
    public override void Draw(Point position, RenderContext context, Rectangle screenScissor)
    {
        context.FillRect(
            new Rectangle(position.X, position.Y, (int)Size.X, (int)Size.Y),
            ColorMask,
            BorderColor,
            BorderRadius,
            BorderWidth,
            GreyGui.Pixel,
            screenScissor
        );

        if (_isLayoutDirty)
        {
            Console.WriteLine("Recalculate Children Layout");
            // update layout cache
            _childrenPosition.Clear();
            float xPadding = (BorderRadius * (Constant.SQRT2 - 1)) + PaddingSide;
            float x = 0;
            float rowMaxY = 0;
            float y = BorderRadius * (Constant.SQRT2 - 1) + PaddingTop;
            foreach (GreyGuiElement child in _children)
            {
                Vector2 childSize = child.Size;
                if (x + childSize.X > _containerSize.X)
                {
                    x = 0;
                    y += rowMaxY;
                    rowMaxY = 0;
                }
                _childrenPosition.Add(new Point((int)(xPadding + x), (int)y));
                x += childSize.X;
                rowMaxY = Math.Max(childSize.Y, rowMaxY);
            }
            _isLayoutDirty = false;
        }
        if (_isChildrenIndexDirty)
        {
            Console.WriteLine("Recalculate Children draw order");
            List<GreyGuiElement> sorted = [.. _children];
            sorted.Sort((a, b) => a.ZIndex > b.ZIndex ? 1 : -1);
            _drawOrder.Clear();
            foreach (GreyGuiElement element in sorted)
            {
                _drawOrder.Add(_childrenIndexMap[element]);
            }
            _isChildrenIndexDirty = false;
        }

        for (int i = 0; i < _drawOrder.Count; i++)
        {
            GreyGuiElement child = _children[_drawOrder[i]];
            Point relativePos = _childrenPosition[_drawOrder[i]];

            child.OnScreenPos = position + relativePos;

            child.Draw(child.OnScreenPos, context, screenScissor);
        }
    }
}