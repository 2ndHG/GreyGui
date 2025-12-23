using Microsoft.Xna.Framework;

namespace GreyGui;

public class Panel : GreyGuiElement, IContainer, IPercentElement
{
    public override Vector2 Size { get => _size; set => _size = value; }
    public Vector2 ContainerSize { get => _containerSize; }

    public bool UsePercentWidth => _usePercentWidth;

    public bool UseHeightWidthRatio => _useHeightWidthRatio;

    public float WidthPercent => _widthPercent;
    public float HeightWidthRatio => _heightWidthRatio;
    public bool IsLayoutDirty { get => _isLayoutDirty; set => _isLayoutDirty = value; }
    public bool IsChildrenIndexDirty { get => _isChildrenIndexDirty; set => _isChildrenIndexDirty = value; }
    public override int ZIndex
    {
        get => _zIndex; set
        {
            if (_zIndex != value)
            {
                _zIndex = value;
                if (Parent is IContainer container)
                {
                    container.IsChildrenIndexDirty = true;
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
    private List<Point> _childrenPosition = [];


    public void AppendChildren(GreyGuiElement[] elements)
    {
        _children.AddRange(elements);
    }

    public void RemoveAllChildren()
    {
        _children.Clear();
    }

    public void RemoveChildren(GreyGuiElement[] elements)
    {
        foreach (GreyGuiElement element in elements)
        {
            _children.Remove(element);
        }
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
        // As an IPercentElement
        bool sizeChanged = false;
        if (UsePercentWidth && Parent != null)
        {
            _size.X = ((IContainer)Parent).ContainerSize.X * _widthPercent;
            sizeChanged = true;
        }
        if (UseHeightWidthRatio)
        {
            _size.Y = _size.X * _heightWidthRatio;
            sizeChanged = true;
        }
        if (sizeChanged && Parent is IContainer container)
        {
            container.IsLayoutDirty = true;
        }

        // As an IContainer
        _containerSize = _size - new Vector2(BorderRadius * 2);
        foreach (GreyGuiElement child in _children)
        {
            child.IsSizeDirty = true;
        }
        _isSizeDirty = false;
    }

    public override void Update()
    {

    }

    // render context not implemented yet
    public override void Draw(Point position, RenderContext context, Rectangle screenScissor)
    {
        context.FillRect(
            new Rectangle(position.X, position.Y, (int)Size.X, (int)Size.Y),
            colorMask,
            BorderRadius,
            0f,
            GreyGui.Pixel,
            screenScissor
        );
        
        if (_isLayoutDirty)
        {
            // update layout cache
            _childrenPosition.Clear();
            int x = BorderRadius;
            float y = BorderRadius;
            foreach (GreyGuiElement child in _children)
            {
                _childrenPosition.Add(new Point(x, (int)y));
                y += child.Size.Y;
            }
            _isLayoutDirty = false;
        }
        if (_isChildrenIndexDirty)
        {
            _children.Sort((a, b) => a.ZIndex > b.ZIndex ? 1 : -1);
            _isChildrenIndexDirty = false;
        }

        for (int i = 0; i < _children.Count; i++)
        {
            GreyGuiElement child = _children[i];
            Point relativePos = _childrenPosition[i];

            child.OnScreenPos = position + relativePos;

            child.Draw(child.OnScreenPos, context, screenScissor);
        }
    }
}