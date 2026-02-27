using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GreyGui;

public static class GuiUpdate
{
    public static int FrameId { get; set; }
    public static GreyGuiElement? FocusedElement { get; set; }
    public static bool IsMouseHandled { get; private set; }
    public static GreyGuiElement? MouseHandler{get; set;}
    public static void StartFrame(MouseState mouseState, KeyboardState keyboardState)
    {
        prevMouseState = currMouseState;
        currMouseState = mouseState;
        prevKeyboardState = currKeyboardState;
        currKeyboardState = keyboardState;
        MouseHandler = null;
        ++FrameId;
    }
    public static void Update(GreyGuiElement root)
    {
        // If the mouse has not been handled yet
        if(MouseHandler == null )
        {
            MouseHandler = root.GetMouseHandler();
            MouseHandler?.HandleMouseEvent();
        }
        root.Update();
    }
    public static class Mouse
    {
        public static Point Position => currMouseState.Position;
        public static bool IsLeftButtonDown => currMouseState.LeftButton == ButtonState.Pressed && currMouseState.LeftButton != prevMouseState.LeftButton;
        public static bool IsLeftButtonUp => currMouseState.LeftButton == ButtonState.Released && currMouseState.LeftButton != prevMouseState.LeftButton;
        public static bool IsLeftHold => currMouseState.LeftButton == ButtonState.Pressed;
        public static bool IsRightButtonDown => currMouseState.RightButton == ButtonState.Pressed && currMouseState.RightButton != prevMouseState.RightButton;
        public static bool IsRightButtonUp => currMouseState.RightButton == ButtonState.Released && currMouseState.RightButton != prevMouseState.RightButton;
        public static bool IsRightHold => currMouseState.RightButton == ButtonState.Pressed;
    }
    private static MouseState prevMouseState;
    private static MouseState currMouseState;
    private static KeyboardState prevKeyboardState;
    private static KeyboardState currKeyboardState;
}