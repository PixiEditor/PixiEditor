using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

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

    private readonly Regex regex = new Regex("^[.][0-9]+$|^[0-9]*[.]{0,1}[0-9]*$", RegexOptions.Compiled);

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

    static NumberInput()
    {
        ValueProperty.Changed.Subscribe(OnValueChanged);
    }

    public NumberInput()
    {
        InitializeComponent();
    }

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

    private static void OnValueChanged(AvaloniaPropertyChangedEventArgs<double> e)
    {
        NumberInput input = (NumberInput)e.Sender;
        input.Value = (float)Math.Round(Math.Clamp(e.NewValue.Value, input.Min, input.Max), input.Decimals);
    }

    private void TextBox_PreviewTextInput(object sender, TextInputEventArgs e)
    {
        e.Handled = !regex.IsMatch((sender as TextBox).Text.Insert((sender as TextBox).SelectionStart, e.Text));
    }

    private void TextBox_MouseWheel(object sender, PointerWheelEventArgs e)
    {
        int step = (int)e.Delta.Y / 100;

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
}
