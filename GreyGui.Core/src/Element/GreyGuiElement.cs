using Microsoft.Xna.Framework;

namespace GreyGui;

public abstract class GreyGuiElement
{
    public bool Enabled { get; set; } = true;
    public Color colorMask = Color.Gray;
    public Color borderColor = Color.Gray;
    public Point OnScreenPos { get; set; }
    public int BorderRadius { get; set; }
    public int BorderWidth { get; set; }
    public GreyGuiElement? Parent { get; set; }
    public abstract Vector2 Size { get; set; }
    public abstract int ZIndex { get; set; }
    public virtual bool IsSizeDirty { get => _isSizeDirty; internal set => _isSizeDirty = value; }

    internal bool _isSizeDirty;



    public abstract void Update();
    public abstract void ResolveSizeDirty();
    public abstract void Draw(Point position, RenderContext renderContext, Rectangle screenScissor);


    protected virtual bool IsMouseOver(Point mousePosition)
    {
        return false;
    }
}