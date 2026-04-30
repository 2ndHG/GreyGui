using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
    public VerticalAlignment VerticalAlignment
    {
        get => _verticalAlignment;
        set
        {
            if (_verticalAlignment == value)
                return;
            _verticalAlignment = value;
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

    private WidthMode _widthMode;
    private HeightMode _heightMode;
    private float _widthRatio;
    private float _heightRatio;
    private float _heightWidthRatio;
    private Vector2 _size;
    private Vector2 _finalSize;
    private int _zIndex;
    private RowLayoutMode _layoutMode;
    private VerticalAlignment _verticalAlignment;
    private float _childGap;
    private Vector2 _containerSize;
    private bool _isLayoutDirty;
    private bool _isChildrenZIndexDirty;
    private List<GreyGuiElement> _children = [];
    private List<Point> _childrenPosition = [];
    private List<int> _drawOrder = [];

    // Render
    protected Texture2D _imageTexture = GreyGuiCore.Atlas;
    protected Rectangle _imageSrcRect;

    public RowPanel() { }
    public RowPanel(
        Color? colorMask = null, Color borderColor = default, int borderRadius = default, int borderWidth = default,
        Vector2 size = default, WidthMode widthMode = WidthMode.Fixed, HeightMode heightMode = HeightMode.Fixed, float widthRatio = default, float heightRatio = default, float heightWidthRatio = default, int paddingTop = default, int paddingBottom = default, int paddingSide = default, int zIndex = default, RowLayoutMode layoutMode = default, VerticalAlignment verticalAlignment = VerticalAlignment.Top, float childGap = default, Texture2D? imageTexture = null, Rectangle imageSrcRect = default, ICollection<GreyGuiElement>? children = null)
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
        _widthMode = widthMode;
        _heightMode = heightMode;
        _widthRatio = widthRatio;
        _heightRatio = heightRatio;
        _heightWidthRatio = heightWidthRatio;
        PaddingTop = paddingTop;
        PaddingBottom = paddingBottom;
        PaddingSide = paddingSide;
        LayoutMode = layoutMode;
        _verticalAlignment = verticalAlignment;
        _childGap = childGap;
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
        foreach (GreyGuiElement child in _children.ToArray())
        {
            child.ChangeParentButParentWillNotKnow(null);
        }
        _children.Clear();
        _isLayoutDirty = true;
        _isChildrenZIndexDirty = true;
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
        _containerSize.X -= PaddingSide * 2 + _childGap * MathF.Max(0, _children.Count - 1);
        _containerSize.Y -= PaddingTop + PaddingBottom;
        foreach (GreyGuiElement child in _children)
        {
            child.IsSizeDirty = true;
        }
        _isSizeDirty = false;
        _isLayoutDirty = _isLayoutDirty || sizeChanged;
    }

    private void ResolveLayoutDirty()
    {
        float elementTotalWidth = 0f;
        for (int i = 0; i < _children.Count; ++i)
        {
            elementTotalWidth += _children[i].FinalSize.X;
        }
        float xLayoutMulti = _layoutMode switch
        {
            RowLayoutMode.Center => .5f,
            RowLayoutMode.Right => 1f,
            _ => 0
        };
        float yLayoutMulti = _verticalAlignment switch
        {
            VerticalAlignment.Center => .5f,
            VerticalAlignment.Bottom => 1f,
            _ => 0
        };
        float emptySpace = _containerSize.X - elementTotalWidth;
        float borderRadiusPad = BorderRadius * (Constant.SQRT2 - 1);

        float x = borderRadiusPad + PaddingSide + emptySpace * xLayoutMulti;
        float y = borderRadiusPad + PaddingTop;
        float childGap = (_layoutMode == RowLayoutMode.Justify && _children.Count > 1) ?
            emptySpace / (_children.Count - 1) + _childGap
            : _childGap;

        // update layout cache  
        _childrenPosition.Clear();
        for (int i = 0; i < _children.Count; ++i)
        {
            Vector2 childSize = _children[i].FinalSize;
            _childrenPosition.Add(new Point((int)x, (int)(y + (_containerSize.Y - childSize.Y) * yLayoutMulti)));
            x += childSize.X + childGap;
        }
        _isLayoutDirty = false;
    }
    public override void Update()
    {
        foreach (GreyGuiElement child in _children)
        {
            child.Update();
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

        // DFS Draw children
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