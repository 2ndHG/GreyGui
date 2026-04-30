using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GreyGui;

/// <summary>
/// The entry point of element updating in GreyGui, providing APIs related to input event handling and element state updating.
/// </summary>
public static class GuiUpdate
{
    public static int FrameId { get; set; }
    public static double ElapsedTimeSecond { get; set; }
    public static IFocusable? FocusedElement
    {
        get => _focusedElement;
        set
        {
            if (_focusedElement == value)
                return;
            FocusedElement?.TriggerOnBlurred(); // blur the current focusing element first
            _focusedElement = value;
            _focusedElement?.TriggerOnFocused();
        }
    }
    /// <summary>
    /// If the mouse is being captured by a GreyGuiElement.
    /// </summary>
    public static bool IsMouseHandled { get => MouseHandler != null; }

    /// <summary>
    /// The GreyGuiElement currently captures the mouse.
    /// </summary>
    public static GreyGuiElement? MouseHandler { get; set; }

    /// <summary>
    /// Notice GuiUpdate to introduce a new frame and provide information to this frame. 
    /// </summary>
    /// <param name="gameTime">GameTime instance, typically from Game.Update</param>
    /// <param name="mouseState"></param>
    /// <param name="keyboardState"></param>
    public static void StartFrame(GameTime gameTime, MouseState mouseState, KeyboardState keyboardState)
    {
        ElapsedTimeSecond = gameTime.ElapsedGameTime.TotalSeconds;

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

    /// <summary>
    /// Update a <see cref="GreyGuiElement"/> tree from its root.
    /// </summary>
    /// <param name="root">Root GreyGuiElement</param>
    public static void Update(GreyGuiElement root)
    {
        // If the mouse has not been handled yet
        if (GreyGuiCore.GameInstance.IsActive && MouseHandler == null)
        {
            MouseHandler = root.GetMouseHandler();
            MouseHandler?.HandleMouseEvent();
        }
        root.Update();
    }

    /// <summary>
    /// Initialize GuiUpdate. This will be called on GreyGui.Initialize.
    /// </summary>
    /// <param name="game"></param>
    public static void Initialize(Game game)
    {
        // TODO check if this is called before, if so, don't add
        game.Window.TextInput += OnTextInput;
    }

    /// <summary>
    /// Mouse state of this frame.
    /// </summary>
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

    /// <summary>
    /// Keyboard state of this frame.
    /// </summary>
    public static class Keyboard
    {
        public static Keys[] GetPressedKeys() => pressedKeys;
        public static int GetPressedKeyCount() => pressedKeyCount;
        public static bool IsKeyDown(Keys key) => !prevKeyboardState.IsKeyDown(key) && currKeyboardState.IsKeyDown(key);
        public static bool IsKeyHold(Keys key) => currKeyboardState.IsKeyDown(key);
        public static bool IsKeyUp(Keys key) => prevKeyboardState.IsKeyDown(key) && !currKeyboardState.IsKeyDown(key);

        public static ReadOnlySpan<char> GetTextInputBuffer() => CollectionsMarshal.AsSpan(activeInputBuffer);
    }

    private const int maxInputBufferSize = 64;
    private static IFocusable? _focusedElement = null;
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
        // Console.WriteLine(eventArgs.Key.ToString() + eventArgs.Character.ToString());
        if (nextFrameInputBuffer.Count > maxInputBufferSize)
        {
            return;
        }

        if (eventArgs.Key == Keys.Enter)
        {
            // Unify \n, \r to \n
            nextFrameInputBuffer.Add('\n');
            return;
        }
        if (eventArgs.Key == Keys.Back)
        {
            nextFrameInputBuffer.Add('\b');
            return;
        }
        nextFrameInputBuffer.Add(eventArgs.Character);
    }
}