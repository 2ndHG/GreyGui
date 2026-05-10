using System.Buffers;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    public override Vector2 FinalSize => _finalSize;
    public Vector2 ContainerSize { get => _containerSize; }

    public WidthMode WidthMode
    {
        get => _widthMode;
        set
        {
            if (_widthMode == value) return;
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

    public Span<GreyGuiElement> Children { get => CollectionsMarshal.AsSpan(_children); }

    private WidthMode _widthMode;
    private HeightMode _heightMode;
    private float _widthRatio;
    private float _heightRatio;
    private float _heightWidthRatio;
    private Vector2 _size;
    private Vector2 _finalSize;
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


    // Render
    protected Texture2D _imageTexture = GreyGuiCore.Atlas;
    protected Rectangle _imageSrcRect;

    public ListPanel() { }
    public ListPanel(
        Color? colorMask = null, Color borderColor = default, int borderRadius = default, int borderWidth = default,
        Vector2 size = default, WidthMode widthMode = WidthMode.Fixed, HeightMode heightMode = HeightMode.Fixed, float widthRatio = default, float heightRatio = default, float heightWidthRatio = default, int paddingTop = default, int paddingBottom = default, int paddingSide = default, int zIndex = default, RowLayoutMode layoutMode = default, float childGap = default, float rowGap = default, Texture2D? imageTexture = null, Rectangle imageSrcRect = default, ICollection<GreyGuiElement>? children = null)
    {
        ColorMask = (colorMask, imageTexture) switch
        {
            (not null, _) => (Color)colorMask,
            (null, not null) => Color.White,
            _ => Color.Gray
        };

        BorderColor = borderColor;
        BorderRadius = borderRadius;
        BorderWidth = borderWidth;
        _size = size;
        _heightMode = heightMode;
        _widthMode = widthMode;
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
        _imageTexture = imageTexture ?? GreyGuiCore.Atlas;
        _imageSrcRect = (imageTexture, imageSrcRect.IsEmpty) switch
        {
            (null, _) => new Rectangle(0, 0, 1, 1),
            (not null, true) => imageTexture.Bounds,
            (not null, false) => imageSrcRect
        };

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
        int i = 0;
        foreach (GreyGuiElement child in children)
        {
            if (child == null)
                throw new Exception($"Child at index {i} is null");
            AppendChild(child);
            i++;
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
        foreach (GreyGuiElement child in _children.ToArray())
        {
            child.ChangeParentButParentWillNotKnow(null);
        }
        _children.Clear();
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
        bool sizeChanged = false;
        _finalSize = _size;
        // As an IRatioElement
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
        // UseHeightRatio has a higher priority then UseHeightWidthRatio
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
                x += _children[i].FinalSize.X + childGapWidth;
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
            Vector2 childSize = child.FinalSize;
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
        int count = _children.Count;
        GreyGuiElement[] childrenCopy = ArrayPool<GreyGuiElement>.Shared.Rent(count);
        try
        {
            _children.CopyTo(childrenCopy, 0);
            for (int i = 0; i < count; ++i)
            {
                childrenCopy[i].Update();
            }
        }
        finally
        {
            Array.Clear(childrenCopy, 0, count);
            ArrayPool<GreyGuiElement>.Shared.Return(childrenCopy);
        }
    }

    // render context not implemented yet
    public override void Draw(Point position, RenderContext context, Rectangle screenScissor)
    {
        OnScreenPos = position;
        LastScissor = screenScissor;
        context.RenderTexture(
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
        ResolveChildrenZIndexDirty();

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

    private void ResolveChildrenZIndexDirty()
    {
        if (!_isChildrenZIndexDirty)
            return;
        // Sort the index using children's ZIndex, so the low-ZIndex-elements' indices in _children will be put in the front of _drawOrder, therefore be drew first later on
        _drawOrder.Clear();
        for (int i = 0; i < _children.Count; ++i)
        {
            _drawOrder.Add(i);
        }
        _drawOrder.Sort((a, b) => _children[a].ZIndex.CompareTo(_children[b].ZIndex));
        _isChildrenZIndexDirty = false;
    }

    public override GreyGuiElement? GetMouseHandler()
    {
        if (_isLayoutDirty)
            return null;
        ResolveChildrenZIndexDirty();

        // Later drew element earlier be tested
        for (int i = _drawOrder.Count - 1; i >= 0; --i)
        {
            GreyGuiElement? result = _children[_drawOrder[i]].GetMouseHandler();
            if (result != null)
                return result;
        }

        Rectangle selfRect = new(OnScreenPos, _finalSize.ToPoint());
        Rectangle lastAppliedScissor = LastScissor;
        Rectangle.Intersect(ref selfRect, ref lastAppliedScissor, out Rectangle detectingRect);
        return detectingRect.Contains(GuiUpdate.Mouse.Position) ? this : null;
    }
}