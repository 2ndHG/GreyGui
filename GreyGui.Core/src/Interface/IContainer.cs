using Microsoft.Xna.Framework;

namespace GreyGui;

public interface IContainer
{
    public void AppendChildren(GreyGuiElement[] elements);
    public void RemoveChildren(GreyGuiElement[] elements);
    public void RemoveAllChildren();
    public Vector2 ContainerSize { get; }

    public bool IsLayoutDirty { get; set; }
    public bool IsChildrenIndexDirty { get; set; }
}