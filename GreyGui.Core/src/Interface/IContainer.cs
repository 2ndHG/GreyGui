using Microsoft.Xna.Framework;

namespace GreyGui;

public interface IContainer
{
    public void AppendChild(GreyGuiElement child);
    public void AppendChildren(ICollection<GreyGuiElement> children);
    public void RemoveChild(GreyGuiElement child);
    public void RemoveChildren(ICollection<GreyGuiElement> children);
    public void RemoveAllChildren();
    public Vector2 ContainerSize { get; }

    public bool IsLayoutDirty { get; set; }
    public bool IsChildrenIndexDirty { get; set; }
}