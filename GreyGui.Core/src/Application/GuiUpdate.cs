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

        _prevMouseState = _currMouseState;
        _currMouseState = mouseState;
        _prevKeyboardState = _currKeyboardState;
        _currKeyboardState = keyboardState;
        keyboardState.GetPressedKeys(_pressedKeys);
        _pressedKeyCount = keyboardState.GetPressedKeyCount();
        MouseHandler = null;

        (_nextFrameInputBuffer, _activeInputBuffer) = (_activeInputBuffer, _nextFrameInputBuffer);
        _nextFrameInputBuffer.Clear();
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
    public static void StopHandlingMouseThisFrame()
    {
        MouseHandler ??= _virtualMouseHandler;
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
        public static Point Position => _currMouseState.Position;
        public static bool IsLeftButtonDown => _currMouseState.LeftButton == ButtonState.Pressed && _currMouseState.LeftButton != _prevMouseState.LeftButton;
        public static bool IsLeftButtonUp => _currMouseState.LeftButton == ButtonState.Released && _currMouseState.LeftButton != _prevMouseState.LeftButton;
        public static bool IsLeftHold => _currMouseState.LeftButton == ButtonState.Pressed;
        public static bool IsRightButtonDown => _currMouseState.RightButton == ButtonState.Pressed && _currMouseState.RightButton != _prevMouseState.RightButton;
        public static bool IsRightButtonUp => _currMouseState.RightButton == ButtonState.Released && _currMouseState.RightButton != _prevMouseState.RightButton;
        public static bool IsRightHold => _currMouseState.RightButton == ButtonState.Pressed;
    }

    /// <summary>
    /// Keyboard state of this frame.
    /// </summary>
    public static class Keyboard
    {
        public static Keys[] GetPressedKeys() => _pressedKeys;
        public static int GetPressedKeyCount() => _pressedKeyCount;
        public static bool IsKeyDown(Keys key) => !_prevKeyboardState.IsKeyDown(key) && _currKeyboardState.IsKeyDown(key);
        public static bool IsKeyHold(Keys key) => _currKeyboardState.IsKeyDown(key);
        public static bool IsKeyUp(Keys key) => _prevKeyboardState.IsKeyDown(key) && !_currKeyboardState.IsKeyDown(key);

        public static ReadOnlySpan<char> GetTextInputBuffer() => CollectionsMarshal.AsSpan(_activeInputBuffer);
    }

    private const int maxInputBufferSize = 64;
    private static IFocusable? _focusedElement = null;
    private static MouseState _prevMouseState;
    private static MouseState _currMouseState;
    private static KeyboardState _prevKeyboardState;
    private static KeyboardState _currKeyboardState;
    private readonly static Keys[] _pressedKeys = new Keys[64];
    private static int _pressedKeyCount = 0;
    private static List<char> _nextFrameInputBuffer = [];
    private static List<char> _activeInputBuffer = [];
    private readonly static GreyGuiElement _virtualMouseHandler = new Text(displayText: "Virtual Mouse Handler");

    private static void OnTextInput(object? _, TextInputEventArgs eventArgs)
    {
        // Console.WriteLine(eventArgs.Key.ToString() + eventArgs.Character.ToString());
        if (_nextFrameInputBuffer.Count > maxInputBufferSize)
        {
            return;
        }

        if (eventArgs.Key == Keys.Enter)
        {
            // Unify \n, \r to \n
            _nextFrameInputBuffer.Add('\n');
            return;
        }
        if (eventArgs.Key == Keys.Back)
        {
            _nextFrameInputBuffer.Add('\b');
            return;
        }
        _nextFrameInputBuffer.Add(eventArgs.Character);
    }
}