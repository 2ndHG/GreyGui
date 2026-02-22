using Microsoft.Xna.Framework;

namespace GreyGui;

public interface IContainer
{
    public void AppendChild(GreyGuiElement child);
    public void RemoveChild(GreyGuiElement child);
    public void RemoveAllChildren();
    public void DrawChildren(Point selfPosition, RenderContext context, Rectangle screenScissor);
    public Vector2 ContainerSize { get; }

    public bool IsLayoutDirty { get; set; }
    public bool IsChildrenZIndexDirty { get; set; }
}