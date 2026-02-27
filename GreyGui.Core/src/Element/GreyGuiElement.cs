using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GreyGui;

public abstract class GreyGuiElement
{
    public bool Enabled { get; set; } = true;
    public Color ColorMask { get; set; } = Color.Gray;
    public Color BorderColor { get; set; } = Color.Gray;
    public Point OnScreenPos { get; set; }
    public int BorderRadius { get; set; }
    public int BorderWidth { get; set; }
    public IContainer? Parent { get => _parent; }
    public abstract Vector2 Size { get; set; }
    /// <summary>
    /// Smaller draw first
    /// </summary>
    public abstract int ZIndex { get; set; }
    public virtual bool IsSizeDirty { get => _isSizeDirty; set => _isSizeDirty = value; }

    protected bool _isSizeDirty = true;
    protected IContainer? _parent;

    public abstract void Update();
    public abstract void ResolveSizeDirty();
    public abstract void Draw(Point position, RenderContext renderContext, Rectangle screenScissor);
    public virtual GreyGuiElement? GetMouseHandler()
    {
        return null;
    }

    public virtual void HandleMouseEvent()
    {
    }

    public void ChangeParentButParentWillNotKnow(IContainer? newParentValue)
    {
        _isSizeDirty = _parent != newParentValue;
        _parent = newParentValue;
    }
    /// <summary>
    /// Identified to <c>Parent?.RemoveChild(this);</c>
    /// </summary>
    public void LeaveParent()
    {
        _parent?.RemoveChild(this);
    }
}