using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GreyGui;

public static class GuiUpdate
{
    public static int FrameId { get; set; }
    public static GreyGuiElement? FocusedElement { get; set; }
    public static bool IsMouseHandled { get; private set; }
    public static GreyGuiElement? MouseHandler { get; set; }
    public static void StartFrame(MouseState mouseState, KeyboardState keyboardState)
    {
        prevMouseState = currMouseState;
        currMouseState = mouseState;
        prevKeyboardState = currKeyboardState;
        currKeyboardState = keyboardState;
        keyboardState.GetPressedKeys(pressedKeys);
        pressedKeyCount = keyboardState.GetPressedKeyCount();
        MouseHandler = null;

        (nextFrameInputBuffer, activeInputBuffer) = (activeInputBuffer, nextFrameInputBuffer);
        nextFrameInputBuffer.Clear();
        ++FrameId;
    }
    public static void Update(GreyGuiElement root)
    {
        // If the mouse has not been handled yet
        if (MouseHandler == null)
        {
            MouseHandler = root.GetMouseHandler();
            MouseHandler?.HandleMouseEvent();
        }
        root.Update();
    }
    public static void Initialize(Game game)
    {
        game.Window.TextInput += OnTextInput;
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
    public static class Keyboard
    {
        public static Keys[] GetPressedKeys() => pressedKeys;
        public static int GetPressedKeyCount() => pressedKeyCount;
        public static bool IsKeyDown(Keys key) => !prevKeyboardState.IsKeyDown(key) && currKeyboardState.IsKeyDown(key);
        public static bool IsKeyHold(Keys key) => currKeyboardState.IsKeyDown(key);

        public static ReadOnlySpan<char> GetTextInputBuffer() => CollectionsMarshal.AsSpan(activeInputBuffer);
    }

    private const int maxInputBufferSize = 64;
    private static MouseState prevMouseState;
    private static MouseState currMouseState;
    private static KeyboardState prevKeyboardState;
    private static KeyboardState currKeyboardState;
    private readonly static Keys[] pressedKeys = new Keys[64];
    private static int pressedKeyCount = 0;
    private static List<char> nextFrameInputBuffer = [];
    private static List<char> activeInputBuffer = [];

    private static void OnTextInput(object? _, TextInputEventArgs eventArgs)
    {
        Console.WriteLine(eventArgs.Key.ToString() + eventArgs.Character.ToString());
        if(nextFrameInputBuffer.Count > maxInputBufferSize)
        {
            return;
        }
        
        if(eventArgs.Key == Keys.Enter)
        {
            // Unify \n, \r to \n
            nextFrameInputBuffer.Add('\n');
            return;
        }
        nextFrameInputBuffer.Add(eventArgs.Character);
    }
}