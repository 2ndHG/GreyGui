using Microsoft.Xna.Framework;

namespace GreyGui;
public abstract class GreyGuiDrawModule<T> where T : GreyGuiElement
{
    public abstract void OnStateEnter(T element);
    public abstract void Draw(T element, Point position, RenderContext renderContext, Rectangle screenScissor);
    public abstract void OnStateExit(T element);
}