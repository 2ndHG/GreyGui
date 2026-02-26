using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GreyGui;

public static class GreyGuiUpdate
{
    public static int FrameId { get; set; }
    public static GreyGuiElement? FocusedElement { get; set; }
    public static void StartFrame()
    {
        ++FrameId;
    }
    public static bool HandleMouseEvent(MouseState mouseState, GreyGuiElement root)
    {
        return root.HandleMouseEvent(ref mouseState);
    }
    public static void FocusedElementHandleKeyboard(ref KeyboardState keyboardState, GreyGuiElement root)
    {
    }
    private static MouseState mouseState;
}