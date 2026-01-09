using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;

namespace GreyGui;

public class RowPanel : GreyGuiElement, IContainer, IRatioElement
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
    public int PaddingTop { get; set; }
    public int PaddingBottom { get; set; }
    public int PaddingSide { get; set; }
    public float ChildGap
    {
        get => _childGap;
        set
        {
            if (value == _childGap)
                return;
            _childGap = value;
            _isLayoutDirty = true;
        }
    }
    public PanelLayoutMode LayoutMode
    {
        get => _layoutMode;
        set
        {
            if (_layoutMode == value)
                return;
            _layoutMode = value;
            IsLayoutDirty = true;
        }
    }
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


    private bool _useWidthRatio;
    private bool _useHeightRatio;
    private bool _useHeightWidthRatio;
    private float _widthRatio;
    private float _heightRatio;
    private float _heightWidthRatio;
    private Vector2 _size;
    private int _zIndex;
    private PanelLayoutMode _layoutMode;
    private float _childGap;
    private Vector2 _containerSize;
    private bool _isLayoutDirty;
    private bool _isChildrenIndexDirty;
    private List<GreyGuiElement> _children = [];
    private Dictionary<GreyGuiElement, int> _childrenIndexMap = [];
    private List<Point> _childrenPosition = [];
    private List<int> _drawOrder = [];

    public RowPanel() { }
    public RowPanel(
        Color colorMask, Color borderColor = default, int borderRadius = default, int borderWidth = default,
        Vector2 size = default, bool useWidthRatio = default, bool useHeightRatio = default, bool useHeightWidthRatio = default, float widthRatio = default, float heightRatio = default, float heightWidthRatio = default, int paddingTop = default, int paddingBottom = default, int paddingSide = default, int zIndex = default, PanelLayoutMode layoutMode = default, float childGap = default, ICollection<GreyGuiElement>? children = null)
    {
        ColorMask = colorMask;
        BorderColor = borderColor;
        BorderRadius = borderRadius;
        BorderWidth = borderWidth;
        _size = size;
        _useWidthRatio = useWidthRatio;
        _useHeightRatio = useHeightRatio;
        _useHeightWidthRatio = useHeightWidthRatio;
        _widthRatio = widthRatio;
        _heightRatio = heightRatio;
        _heightWidthRatio = heightWidthRatio;
        PaddingTop = paddingTop;
        PaddingBottom = paddingBottom;
        PaddingSide = paddingSide;
        LayoutMode = layoutMode;
        _childGap = childGap;
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

    public RowPanel SetChildren(ICollection<GreyGuiElement> children)
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
        _containerSize.X -= PaddingSide * 2 + _childGap * MathF.Max(0, _children.Count - 1);
        _containerSize.Y -= PaddingTop + PaddingBottom;
        foreach (GreyGuiElement child in _children)
        {
            child.IsSizeDirty = true;
        }
        _isSizeDirty = false;
        _isLayoutDirty = _isLayoutDirty || sizeChanged;
    }

    private void RecalculateLayout()
    {
        float elementTotalWidth = 0f;
        for (int i = 0; i < _children.Count; ++i)
        {
            elementTotalWidth += _children[i].Size.X;
        }
        float xLayoutMulti = _layoutMode switch
        {
            PanelLayoutMode.Center => .5f,
            PanelLayoutMode.Right => 1f,
            _ => 0
        };
        float emptySpace = _containerSize.X - elementTotalWidth;
        float borderRadiusPad = BorderRadius * (Constant.SQRT2 - 1);

        float x = borderRadiusPad + PaddingSide + emptySpace * xLayoutMulti;
        float y = borderRadiusPad + PaddingTop;
        float childGap = (_layoutMode == PanelLayoutMode.Spread && _children.Count > 1) ?
            emptySpace / (_children.Count - 1) + _childGap
            : _childGap;

        // update layout cache  
        _childrenPosition.Clear();
        for (int i = 0; i < _children.Count; ++i)
        {
            _childrenPosition.Add(new Point((int)x, (int)y));
            x += _children[i].Size.X + childGap;
        }
        _isLayoutDirty = false;
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
            GreyGui.Atlas,
            screenScissor
        );

        if (_isLayoutDirty)
        {
            RecalculateLayout();
        }
        if (_isChildrenIndexDirty)
        {
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