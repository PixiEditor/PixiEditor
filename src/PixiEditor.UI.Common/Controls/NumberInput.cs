using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Xaml.Interactivity;
using PixiEditor.UI.Common.Behaviors;
using Action = System.Action;

namespace PixiEditor.UI.Common.Controls;

public partial class NumberInput : TextBox
{
    public static event Action<BehaviorCollection> AttachGlobalBehaviors;

    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<NumberInput, double>(
            nameof(Value), 0, coerce: CoerceValue);

    public static readonly StyledProperty<double> MinProperty =
        AvaloniaProperty.Register<NumberInput, double>(
            nameof(Min), float.NegativeInfinity, coerce: CoerceValue);

    public static readonly StyledProperty<double> MaxProperty =
        AvaloniaProperty.Register<NumberInput, double>(
            nameof(Max), double.PositiveInfinity, coerce: CoerceValue);

    public static readonly StyledProperty<string> FormattedValueProperty =
        AvaloniaProperty.Register<NumberInput, string>(
            nameof(FormattedValue), "0");

    public static readonly StyledProperty<bool> EnableScrollChangeProperty =
        AvaloniaProperty.Register<NumberInput, bool>(
            "EnableScrollChange", true);

    public static readonly StyledProperty<bool> EnableGrabberProperty = AvaloniaProperty.Register<NumberInput, bool>(
        nameof(EnableGrabber), true);

    public bool EnableGrabber
    {
        get => GetValue(EnableGrabberProperty);
        set => SetValue(EnableGrabberProperty, value);
    }

    public string FormattedValue
    {
        get => GetValue(FormattedValueProperty);
        set => SetValue(FormattedValueProperty, value);
    }

    public static readonly StyledProperty<bool> SelectOnMouseClickProperty =
        AvaloniaProperty.Register<NumberInput, bool>(
            nameof(SelectOnMouseClick), true);

    public static readonly StyledProperty<bool> ConfirmOnEnterProperty = AvaloniaProperty.Register<NumberInput, bool>(
        nameof(ConfirmOnEnter), true);

    public bool ConfirmOnEnter
    {
        get => GetValue(ConfirmOnEnterProperty);
        set => SetValue(ConfirmOnEnterProperty, value);
    }

    public bool SelectOnMouseClick
    {
        get => GetValue(SelectOnMouseClickProperty);
        set => SetValue(SelectOnMouseClickProperty, value);
    }

    private static Regex regex;

    public static readonly StyledProperty<int> DecimalsProperty =
        AvaloniaProperty.Register<NumberInput, int>(nameof(Decimals), 2, coerce: CoerceDecimals);

    public Action OnScrollAction
    {
        get { return (Action)GetValue(OnScrollActionProperty); }
        set { SetValue(OnScrollActionProperty, value); }
    }

    public static readonly StyledProperty<Action> OnScrollActionProperty =
        AvaloniaProperty.Register<NumberInput, Action>(nameof(OnScrollAction));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Min
    {
        get => (double)GetValue(MinProperty);
        set => SetValue(MinProperty, value);
    }

    public double Max
    {
        get => (double)GetValue(MaxProperty);
        set => SetValue(MaxProperty, value);
    }


    public int Decimals
    {
        get { return (int)GetValue(DecimalsProperty); }
        set { SetValue(DecimalsProperty, value); }
    }

    public static readonly StyledProperty<bool> FocusNextProperty =
        AvaloniaProperty.Register<NumberInput, bool>(
            nameof(FocusNext));

    public bool FocusNext
    {
        get { return (bool)GetValue(FocusNextProperty); }
        set { SetValue(FocusNextProperty, value); }
    }

    private static readonly DataTable DataTable = new DataTable();

    private static char[] allowedChars = new char[]
    {
        '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '+', '-', '*', '/', '(', ')', '.', ',', ' ', 'i', 'n',
        'f', 't', 'y', 'e', 'I', 'N', 'F', 'T', 'Y', 'E'
    };


    protected override Type StyleKeyOverride => typeof(TextBox);

    public bool EnableScrollChange
    {
        get { return (bool)GetValue(EnableScrollChangeProperty); }
        set { SetValue(EnableScrollChangeProperty, value); }
    }

    private Control? leftGrabber;
    private Control? rightGrabber;

    private double _pressedValue;
    private double _pressedRelativeX;

    private double scrollBuildup;

    static NumberInput()
    {
        ValueProperty.Changed.Subscribe(OnValueChanged);
    }

    public NumberInput()
    {
        BehaviorCollection behaviors = Interaction.GetBehaviors(this);
        AttachGlobalBehaviors?.Invoke(behaviors);
        TextBoxFocusBehavior behavior = new() { DeselectOnFocusLoss = true, SelectOnMouseClick = true };
        BindTextBoxBehavior(behavior);
        behaviors.Add(behavior);
        Interaction.SetBehaviors(this, behaviors);

        Binding binding = new Binding(nameof(FormattedValue)) { Source = this, Mode = BindingMode.TwoWay };

        this.Bind(TextProperty, binding);

        Focusable = true;
        TextAlignment = TextAlignment.Center;
        VerticalAlignment = VerticalAlignment.Center;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (EnableGrabber)
        {
            InnerLeftContent = leftGrabber = CreateMouseGrabber();
            leftGrabber.HorizontalAlignment = HorizontalAlignment.Left;
            InnerRightContent = rightGrabber = CreateMouseGrabber();
            rightGrabber.HorizontalAlignment = HorizontalAlignment.Right;
        }
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        if (!EnableGrabber)
        {
            return;
        }

        if (e.NewSize.Width < 100)
        {
            rightGrabber.IsVisible = false;
        }

        leftGrabber.Height = e.NewSize.Height - 10;
        leftGrabber.Width = e.NewSize.Width / 4f;

        rightGrabber.Height = e.NewSize.Height - 10;
        rightGrabber.Width = e.NewSize.Width / 4f;
    }

    private Control CreateMouseGrabber()
    {
        var grabber = new Grid()
        {
            Cursor = new Cursor(StandardCursorType.SizeWestEast), Background = Brushes.Transparent,
        };

        grabber.PointerPressed += GrabberPressed;
        grabber.PointerMoved += GrabberMoved;

        return grabber;
    }

    private void GrabberPressed(object sender, PointerPressedEventArgs e)
    {
        e.Pointer.Capture(leftGrabber);
        _pressedValue = Value;
        _pressedRelativeX = e.GetPosition(this).X;
        e.Handled = true;
    }

    private void GrabberMoved(object sender, PointerEventArgs e)
    {
        if (e.Pointer.Captured != null &&
            (e.Pointer.Captured.Equals(leftGrabber) || e.Pointer.Captured.Equals(rightGrabber)))
        {
            double relativeX = e.GetPosition(this).X;
            double diff = relativeX - _pressedRelativeX;

            double pixelsPerUnit = 5;

            double newValue = _pressedValue + diff / pixelsPerUnit;
            newValue = Math.Round(newValue, Math.Min(Decimals, 2));
            Value = newValue;
            e.Handled = true;
        }
    }

    private void BindTextBoxBehavior(TextBoxFocusBehavior behavior)
    {
        Binding focusNextBinding = new Binding(nameof(FocusNext)) { Source = this, Mode = BindingMode.OneWay };

        behavior.Bind(TextBoxFocusBehavior.FocusNextProperty, focusNextBinding);

        Binding selectOnMouseClickBinding = new Binding(nameof(SelectOnMouseClick))
        {
            Source = this, Mode = BindingMode.OneWay
        };

        behavior.Bind(TextBoxFocusBehavior.SelectOnMouseClickProperty, selectOnMouseClickBinding);

        Binding confirmOnEnterBinding = new Binding(nameof(ConfirmOnEnter))
        {
            Source = this, Mode = BindingMode.OneWay
        };

        behavior.Bind(TextBoxFocusBehavior.ConfirmOnEnterProperty, confirmOnEnterBinding);
    }

    private static double CoerceValue(AvaloniaObject o, double value)
    {
        double min = (double)o.GetValue(MinProperty);
        double max = (double)o.GetValue(MaxProperty);
        int decimals = (int)o.GetValue(DecimalsProperty);

        return Math.Round(Math.Clamp(value, min, max), decimals);
    }

    private static int CoerceDecimals(AvaloniaObject o, int value)
    {
        if (value < 0)
        {
            value = 0;
        }

        return value;
    }

    private static void OnValueChanged(AvaloniaPropertyChangedEventArgs<double> e)
    {
        NumberInput input = (NumberInput)e.Sender;
        //input.Value = (float)Math.Round(Math.Clamp(e.NewValue.Value, input.Min, input.Max), input.Decimals);

        var preFormatted = FormatValue(input.Value, input.Decimals);
        input.FormattedValue = preFormatted;
    }

    private static string FormatValue(double value, int decimals)
    {
        string separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        string decimalString = ((float)value).ToString(CultureInfo.CurrentCulture);

        string preFormatted = decimalString;
        if (preFormatted.Contains(separator))
        {
            if (preFormatted.Split(separator)[1].Length > decimals)
            {
                preFormatted =
                    preFormatted[
                        ..(preFormatted.LastIndexOf(separator, StringComparison.InvariantCulture) + decimals +
                           1)];
            }

            preFormatted = preFormatted.TrimEnd('0');
            preFormatted = preFormatted.TrimEnd(separator.ToCharArray());
        }

        return preFormatted;
    }

    private static bool TryParse(string s, out double value)
    {
        if (s == null)
        {
            value = 0;
            return false;
        }

        s = s.Replace(",", ".");

        if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
        {
            return true;
        }

        return TryEvaluateExpression(s, out value);
    }

    private static bool TryEvaluateExpression(string s, out double value)
    {
        try
        {
            var computed = DataTable.Compute(s, "");
            if (IsNumber(computed))
            {
                value = Convert.ChangeType(computed, typeof(double)) as double? ?? 0;
                return true;
            }

            value = 0;
            return false;
        }
        catch
        {
            value = 0;
            return false;
        }
    }

    private static bool IsNumber(object value)
    {
        return value is sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal;
    }

    private static bool ContainsInvalidCharacter(string text)
    {
        if (text == null)
        {
            return false;
        }

        return text.Any(c => !allowedChars.Contains(c));
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        if (!EnableScrollChange)
        {
            return;
        }

        e.Handled = true;
        double requiredBuildup = 1f;

        if (Decimals == 0 && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            requiredBuildup = 2;
        }

        scrollBuildup += e.Delta.Y;

        if (Math.Abs(scrollBuildup) < requiredBuildup)
        {
            return;
        }

        double step = Math.Sign(e.Delta.Y);

        double newValue = Value;
        if (e.KeyModifiers.HasFlag(KeyModifiers.Shift) && Min - double.NegativeInfinity > 0.1f &&
            Max - double.PositiveInfinity > 0.1f)
        {
            double multiplier = (Max - Min) * 0.1f;
            newValue += step * multiplier;
        }
        else if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && Decimals > 0)
        {
            newValue += step / 2f;
        }
        else
        {
            newValue += step;
        }

        Value = (float)Math.Round(Math.Clamp(newValue, Min, Max), Decimals);

        scrollBuildup = 0;
        OnScrollAction?.Invoke();
    }

    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);
        if (TryParse(FormattedValue, out double value))
        {
            Value = (float)Math.Round(Math.Clamp(value, Min, Max), Decimals);
            FormattedValue = FormatValue(Value, Decimals);
        }
        else
        {
            FormattedValue = FormatValue(Value, Decimals);
        }
    }
}
