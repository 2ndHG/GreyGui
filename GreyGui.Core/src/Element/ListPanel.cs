using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GreyGui;

public class ListPanel : GreyGuiElement, IContainer, IRatioElement
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
    public RowLayoutMode LayoutMode
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
    public bool IsChildrenZIndexDirty { get => _isChildrenZIndexDirty; set => _isChildrenZIndexDirty = value; }
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


    private bool _useWidthRatio;
    private bool _useHeightRatio;
    private bool _useHeightWidthRatio;
    private float _widthRatio;
    private float _heightRatio;
    private float _heightWidthRatio;
    private Vector2 _size;
    private int _zIndex;
    private RowLayoutMode _layoutMode;
    private float _childGap;
    private float _rowGap;
    private Vector2 _containerSize;
    private bool _isLayoutDirty;
    private bool _isChildrenZIndexDirty;
    private List<GreyGuiElement> _children = [];
    private List<Point> _childrenPosition = [];
    private List<int> _drawOrder = [];

    public ListPanel() { }
    public ListPanel(
        Color colorMask, Color borderColor = default, int borderRadius = default, int borderWidth = default,
        Vector2 size = default, bool useWidthRatio = default, bool useHeightRatio = default, bool useHeightWidthRatio = default, float widthRatio = default, float heightRatio = default, float heightWidthRatio = default, int paddingTop = default, int paddingBottom = default, int paddingSide = default, int zIndex = default, RowLayoutMode layoutMode = default, float childGap = default, float rowGap = default, ICollection<GreyGuiElement>? children = null)
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
        _rowGap = rowGap;
        _zIndex = zIndex;
        if (children != null)
        {
            AppendChildren(children);
        }
    }

    public void AppendChild(GreyGuiElement child)
    {
        child.Parent?.RemoveChild(child);

        _children.Add(child);
        child.ChangeParentButParentWillNotKnow(this);

        _isChildrenZIndexDirty = true;
        _isLayoutDirty = true;

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
        _children.Remove(child);
        child.ChangeParentButParentWillNotKnow(null);

        _isChildrenZIndexDirty = true;
        _isLayoutDirty = true;

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
        _isChildrenZIndexDirty = true;
    }

    public ListPanel SetChildren(ICollection<GreyGuiElement> children)
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

    private void ResolveLayoutDirty()
    {
        float xPadding = (BorderRadius * (Constant.SQRT2 - 1)) + PaddingSide;
        float xLayoutMulti = _layoutMode switch
        {
            RowLayoutMode.Center => .5f,
            RowLayoutMode.Right => 1f,
            _ => 0
        };
        void InsertRow(float y, float rowElementTotalWidth, float rowHeight, int elementBegin, int elementEnd)
        {
            int gapCount = elementEnd - elementBegin - 1;
            float emptySpace;
            float childGapWidth;
            if (_layoutMode == RowLayoutMode.Justify)
            {
                emptySpace = _containerSize.X - rowElementTotalWidth;
                childGapWidth = gapCount == 0 ? 0 : emptySpace / gapCount;
            }
            else
            {
                emptySpace = _containerSize.X - rowElementTotalWidth - gapCount * _childGap;
                childGapWidth = _childGap;
            }
            float x = xLayoutMulti * emptySpace + xPadding;

            for (int i = elementBegin; i < elementEnd; ++i)
            {
                _childrenPosition.Add(new Point((int)MathF.Round(x), (int)MathF.Round(y)));
                x += _children[i].Size.X + childGapWidth;
            }
        }
        // update layout cache
        _childrenPosition.Clear();
        float totalWidth = 0;
        float rowHeight = 0;
        float y = BorderRadius * (Constant.SQRT2 - 1) + PaddingTop;

        int notInsertedIndex = 0;
        float gapWidth = 0;
        for (int i = 0; i < _children.Count; ++i)
        {
            GreyGuiElement child = _children[i];
            Vector2 childSize = child.Size;
            if (totalWidth + gapWidth + childSize.X > _containerSize.X)
            {
                int count = i - notInsertedIndex;
                InsertRow(y, totalWidth, rowHeight, notInsertedIndex, i);
                notInsertedIndex = i;
                totalWidth = 0;
                gapWidth = 0;
                y += rowHeight + (count > 0 ? _rowGap : 0);
                rowHeight = 0;
            }
            totalWidth += childSize.X;
            gapWidth += _childGap;
            rowHeight = Math.Max(childSize.Y, rowHeight);
        }
        {
            InsertRow(y, totalWidth, rowHeight, notInsertedIndex, _children.Count);
        }
        _isLayoutDirty = false;
    }
    public override void Update()
    {
        for (int i = 0; i < _drawOrder.Count; ++i)
        {
            _children[i].Update();
        }
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
            screenScissor
        );
        OnScreenPos = position;
    }

    public void DrawChildren(Point position, RenderContext context, Rectangle screenScissor)
    {
        foreach (GreyGuiElement child in _children)
        {
            child.ResolveSizeDirty();
        }
        if (_isLayoutDirty)
        {
            ResolveLayoutDirty();
        }
        if (_isChildrenZIndexDirty)
        {
            // Sort the index using children's ZIndex, so the low-ZIndex-elements' indices in _children will be put in the front of _drawOrder, therefore be drew first later on
            _drawOrder.Clear();
            for (int i = 0; i < _children.Count; ++i)
            {
                _drawOrder.Add(i);
            }
            _drawOrder.Sort((a, b) => _children[a].ZIndex.CompareTo(_children[b].ZIndex));
            _isChildrenZIndexDirty = false;
        }

        for (int i = 0; i < _drawOrder.Count; i++)
        {
            int drawOrder = _drawOrder[i];
            _children[drawOrder].Draw(position + _childrenPosition[drawOrder], context, screenScissor);
            if (_children[drawOrder] is IContainer container)
            {
                container.DrawChildren(position + _childrenPosition[drawOrder], context, screenScissor);
            }
        }
    }

    public override GreyGuiElement? GetMouseHandler()
    {
        for (int i = 0; i < _drawOrder.Count; ++i)
        {
            GreyGuiElement? result = _children[i].GetMouseHandler();
            if (result != null)
                return result;
        }
        if (new Rectangle(OnScreenPos, _size.ToPoint()).Contains(GuiUpdate.Mouse.Position))
        {
            return this;
        }
        return null;
    }
}