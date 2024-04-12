using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace PixiEditor.AvaloniaUI.Views.Input;

internal partial class NumberInput : UserControl
{
    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<NumberInput, double>(
            nameof(Value), 0);

    public static readonly StyledProperty<double> MinProperty =
        AvaloniaProperty.Register<NumberInput, double>(
            nameof(Min), float.NegativeInfinity);

    public static readonly StyledProperty<double> MaxProperty =
        AvaloniaProperty.Register<NumberInput, double>(
            nameof(Max), double.PositiveInfinity);

    public static readonly StyledProperty<string> FormattedValueProperty = AvaloniaProperty.Register<NumberInput, string>(
        nameof(FormattedValue), "0");

    public string FormattedValue
    {
        get => GetValue(FormattedValueProperty);
        set => SetValue(FormattedValueProperty, value);
    }

    private static Regex regex;

    public int Decimals
    {
        get { return (int)GetValue(DecimalsProperty); }
        set { SetValue(DecimalsProperty, value); }
    }

    public static readonly StyledProperty<int> DecimalsProperty =
        AvaloniaProperty.Register<NumberInput, int>(nameof(Decimals), 2);

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

    public static readonly StyledProperty<bool> FocusNextProperty =
        AvaloniaProperty.Register<NumberInput, bool>(
            nameof(FocusNext));

    public bool FocusNext
    {
        get { return (bool)GetValue(FocusNextProperty); }
        set { SetValue(FocusNextProperty, value); }
    }

    static NumberInput()
    {
        ValueProperty.Changed.Subscribe(OnValueChanged);
        FormattedValueProperty.Changed.Subscribe(FormattedValueChanged);
    }

    public NumberInput()
    {
        InitializeComponent();
    }

    private static void OnValueChanged(AvaloniaPropertyChangedEventArgs<double> e)
    {
        NumberInput input = (NumberInput)e.Sender;
        input.Value = (float)Math.Round(Math.Clamp(e.NewValue.Value, input.Min, input.Max), input.Decimals);

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
        s = s.Replace(",", ".");
        return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    private static void FormattedValueChanged(AvaloniaPropertyChangedEventArgs<string> e)
    {
        NumberInput input = (NumberInput)e.Sender;
        if(ContainsInvalidCharacter(e.NewValue.Value))
        {
            input.FormattedValue = e.OldValue.Value;
        }
    }

    private static bool ContainsInvalidCharacter(string text)
    {
        return text.Any(c => !char.IsDigit(c) && c != '.' && c != ',');
    }

    private void TextBox_MouseWheel(object sender, PointerWheelEventArgs e)
    {
        int step = (int)e.Delta.Y;

        double newValue = Value;
        if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            double multiplier = (Max - Min) * 0.1f;
            newValue += step * multiplier;
        }
        else if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            newValue += step / 2f;
        }
        else
        {
            newValue += step;
        }

        Value = (float)Math.Round(Math.Clamp(newValue, Min, Max), Decimals);

        OnScrollAction?.Invoke();
    }

    private void TextBox_OnLostFocus(object? sender, RoutedEventArgs e)
    {
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
