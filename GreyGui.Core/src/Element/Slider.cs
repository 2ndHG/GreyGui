using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GreyGui.Core;

public class Slider : GreyGuiElement, IRatioElement, IFocusable
{
    public override Vector2 Size
    {
        get => _size;
        set
        {
            if (_size == value) return;

            _size = value;
            _isSizeDirty = true;
        }
    }
    public override Vector2 FinalSize => _finalSize;
    public override int ZIndex
    {
        get => _zIndex; set
        {
            if (_zIndex != value)
            {
                _zIndex = value;
                if (_parent is not null)
                {
                    _parent.IsChildrenZIndexDirty = true;
                }
            }
        }
    }

    public WidthMode WidthMode
    {
        get => _widthMode;
        set
        {
            if (_widthMode == value)
                return;
            _widthMode = value;
            _isSizeDirty = true;
        }
    }
    public HeightMode HeightMode
    {
        get => _heightMode;
        set
        {
            if (_heightMode == value)
                return;
            _heightMode = value;
            _isSizeDirty = true;
        }
    }

    public float WidthRatio
    {
        get => _widthRatio;
        set
        {
            if (_widthRatio == value)
                return;
            _widthRatio = value;
            _isSizeDirty = true;
        }
    }
    public float HeightRatio
    {
        get => _heightRatio;
        set
        {
            if (_heightRatio == value)
                return;
            _heightRatio = value;
            _isSizeDirty = true;
        }
    }
    public float HeightWidthRatio
    {
        get => _heightWidthRatio;
        set
        {
            if (_heightWidthRatio == value)
                return;
            _heightWidthRatio = value;
            _isSizeDirty = true;
        }
    }
    public int MaxStep { get; set; }
    public SliderValueMode ValueMode { get; set; }

    public TrackDirection TrackDirection { get; set; } = TrackDirection.Horizontal;
    public TrackLengthMode TrackLengthMode { get; set; } = TrackLengthMode.UseButtonSize;

    public event Action? OnValueChanged;
    public float Percentage => _percentage;
    public float Step => ValueMode == SliderValueMode.Step ?
        MathF.Round(_percentage * MaxStep) : MathF.Floor(_percentage * MaxStep);

    private WidthMode _widthMode;
    private HeightMode _heightMode;
    private float _widthRatio;
    private float _heightRatio;
    private float _heightWidthRatio;
    protected Vector2 _size;
    protected Vector2 _finalSize;
    protected int _zIndex;

    private Vector2 _buttonSize;
    private float _percentage = 1f;
    private Point _onPressedMousePosition;
    private float _onPressedPercentage;

    protected Texture2D _panelTexture;
    protected Rectangle _panelSrcRect;
    protected Texture2D _buttonTexture;
    protected Rectangle _buttonSrcRect;
    protected Color _buttonColor;



    public Slider(Color? colorMask = null, Color borderColor = default, Vector2 size = default, WidthMode widthMode = WidthMode.Fixed, HeightMode heightMode = HeightMode.Fixed, float widthRatio = default, float heightRatio = default, float heightWidthRatio = default, int zIndex = default, int borderRadius = 0, int borderWidth = 0, Texture2D? panelTexture = null, Rectangle panelSrcRect = default, Texture2D? buttonTexture = null, Rectangle buttonSrcRect = default, int maxStep = 10, Color? buttonColor = null,
    SliderValueMode valueMode = SliderValueMode.Percentage)
    {
        ColorMask = colorMask ?? Color.Gray;
        _buttonColor = buttonColor ?? new Color(.8f, .8f, .8f, .7f);
        BorderColor = borderColor;
        _size = size;
        _widthMode = widthMode;
        _heightMode = heightMode;
        _widthRatio = widthRatio;
        _heightRatio = heightRatio;
        _heightWidthRatio = heightWidthRatio;
        _zIndex = zIndex;
        _panelTexture = panelTexture ?? GreyGuiCore.Atlas;
        _panelSrcRect = (panelTexture, panelSrcRect.IsEmpty) switch
        {
            (null, _) => new Rectangle(0, 0, 1, 1),
            (not null, true) => panelTexture.Bounds,
            (not null, false) => panelSrcRect
        };

        _buttonTexture = buttonTexture ?? GreyGuiCore.Atlas;
        _buttonSrcRect = (buttonTexture, buttonSrcRect.IsEmpty) switch
        {
            (null, _) => new Rectangle(0, 0, 1, 1),
            (not null, true) => buttonTexture.Bounds,
            (not null, false) => buttonSrcRect
        };

        BorderRadius = borderRadius;
        BorderWidth = borderWidth;

        MaxStep = maxStep;
        ValueMode = valueMode;

        _isSizeDirty = true;
    }
    public override GreyGuiElement? GetMouseHandler()
    {
        Rectangle selfRect = new(OnScreenPos, _finalSize.ToPoint());
        Rectangle lastAppliedScissor = LastScissor;
        Rectangle.Intersect(ref selfRect, ref lastAppliedScissor, out Rectangle detectingRect);
        // Console.WriteLine($"{detectingRect} {GuiUpdate.Mouse.Position}");
        return detectingRect.Contains(GuiUpdate.Mouse.Position) ? this : null;
    }

    public override void HandleMouseEvent()
    {
        if (GuiUpdate.Mouse.IsLeftButtonDown)
        {
            GuiUpdate.FocusedElement = this;
            _onPressedPercentage = _percentage;
            Point buttonPosition = OnScreenPos;
            buttonPosition.X += (int)(_percentage * (_finalSize.X - _buttonSize.X));

            _onPressedMousePosition = new Rectangle(buttonPosition, _buttonSize.ToPoint()).Contains(GuiUpdate.Mouse.Position) ?
                GuiUpdate.Mouse.Position : buttonPosition + new Point((int)(_buttonSize.X / 2), 0);
        }
    }



    public override void Draw(Point position, RenderContext renderContext, Rectangle screenScissor)
    {
        OnScreenPos = position;
        LastScissor = screenScissor;
        renderContext.RenderTexture(
            _panelTexture,
            new Rectangle(position, _finalSize.ToPoint()),
            _panelSrcRect,
            ColorMask,
            BorderColor,
            BorderRadius,
            BorderWidth,
            screenScissor
        );
        Point buttonPosition = position;
        buttonPosition.X += (int)(_percentage * (_finalSize.X - _buttonSize.X));
        renderContext.RenderTexture(
            _buttonTexture,
            new Rectangle(buttonPosition, _buttonSize.ToPoint()),
            _buttonSrcRect,
            GuiUpdate.FocusedElement == this ?
                _buttonColor with { A = 255 } : _buttonColor,
            BorderColor,
            BorderRadius,
            BorderWidth,
            screenScissor
        );
    }

    public override void ResolveSizeDirty()
    {
        if (!_isSizeDirty)
        {
            return;
        }

        _finalSize = _size;

        // As an IRatioElement
        bool sizeChanged = false;
        if (_widthMode == WidthMode.ParentRatio)
        {
            if (_parent == null)
            {
                _finalSize.X = GreyGuiCore.NullParentWidth * _widthRatio;
            }
            else
            {
                _finalSize.X = _parent.ContainerSize.X * _widthRatio;
            }
            sizeChanged = true;
        }
        if (_heightMode == HeightMode.ParentRatio)
        {
            if (_parent == null)
            {
                _finalSize.Y = GreyGuiCore.NullParentHeight * _heightRatio;
            }
            else
            {
                _finalSize.Y = _parent.ContainerSize.Y * _heightRatio;
            }
            sizeChanged = true;
        }
        else if (_heightMode == HeightMode.HeightWidthRatio)
        {
            _finalSize.Y = _finalSize.X * _heightWidthRatio;
            sizeChanged = true;
        }

        if (sizeChanged && _parent is not null)
        {
            _parent.IsLayoutDirty = true;
        }

        _buttonSize = _finalSize;
        if (ValueMode == SliderValueMode.Percentage)
        {
            _buttonSize.X = Math.Max(_buttonSize.X * 0.2f, _buttonSize.Y);
        }
        else
        {
            _buttonSize.X = Math.Max(_finalSize.X / (MaxStep + 1), _finalSize.Y);
        }
        _isSizeDirty = false;
    }

    public override void Update()
    {
        if (GuiUpdate.FocusedElement != this)
        {
            return;
        }

        if (GuiUpdate.Mouse.IsLeftHold)
        {
            if (ValueMode == SliderValueMode.Percentage)
                PercentageModeDragUpdate();
            else
                StepModeDragUpdate();
        }
        else
        {
            GuiUpdate.FocusedElement = null;
        }
    }

    public void TriggerOnBlurred()
    {
    }

    public void TriggerOnFocused()
    {
    }

    private void StepModeDragUpdate()
    {
        float originalValue = Step;

        if (MaxStep <= 0)
            return;

        float trackLength = (_finalSize.X - _buttonSize.X) / MaxStep;
        int step = (int)MathF.Floor((GuiUpdate.Mouse.Position.X - OnScreenPos.X - _buttonSize.X / 2f) / trackLength + 0.5f);
        step = Math.Clamp(step, 0, MaxStep);
        _percentage = (float)step / MaxStep;

        float newValue = Step;


        if (originalValue != newValue)
        {
            OnValueChanged?.Invoke();
            // Console.WriteLine($"{Percentage}, {Step}");
        }
    }
    private void PercentageModeDragUpdate()
    {
        float originalValue = _percentage;

        float trackLength = _finalSize.X - _buttonSize.X;
        float percentageOffset = (GuiUpdate.Mouse.Position.X - _onPressedMousePosition.X) / trackLength;
        _percentage = Math.Clamp(_onPressedPercentage + percentageOffset, 0f, 1f);
        float newValue = _percentage;

        if (originalValue != newValue)
        {
            OnValueChanged?.Invoke();
            // Console.WriteLine($"{Percentage}, {Step}");
        }
    }
}