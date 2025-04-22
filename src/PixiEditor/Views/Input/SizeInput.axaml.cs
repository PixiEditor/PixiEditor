using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using PixiEditor.Models.Dialogs;

namespace PixiEditor.Views.Input;

/// <summary>
///     Interaction logic for SizeInput.xaml.
/// </summary>
internal partial class SizeInput : UserControl
{
    public static readonly StyledProperty<double> SizeProperty =
        AvaloniaProperty.Register<SizeInput, double>(nameof(Size), defaultValue: 1);

    public static readonly StyledProperty<double> MinSizeProperty = AvaloniaProperty.Register<SizeInput, double>(
        nameof(MinSize), defaultValue: 1);

    public static readonly StyledProperty<double> MaxSizeProperty =
        AvaloniaProperty.Register<SizeInput, double>(nameof(MaxSize), defaultValue: double.MaxValue);

    public static readonly StyledProperty<bool> BehaveLikeSmallEmbeddedFieldProperty =
        AvaloniaProperty.Register<SizeInput, bool>(nameof(BehaveLikeSmallEmbeddedField), defaultValue: true);

    public static readonly StyledProperty<string> UnitProperty =
        AvaloniaProperty.Register<SizeInput, string>(nameof(Unit), defaultValue: "PIXEL_UNIT");

    public static readonly StyledProperty<bool> FocusNextProperty = AvaloniaProperty.Register<SizeInput, bool>(
        nameof(FocusNext), defaultValue: true);

    public static readonly StyledProperty<int> DecimalsProperty = AvaloniaProperty.Register<SizeInput, int>(
        nameof(Decimals), defaultValue: 0);

    public int Decimals
    {
        get => GetValue(DecimalsProperty);
        set => SetValue(DecimalsProperty, value);
    }

    public bool FocusNext
    {
        get => GetValue(FocusNextProperty);
        set => SetValue(FocusNextProperty, value);
    }

    public Action OnScrollAction
    {
        get { return GetValue(OnScrollActionProperty); }
        set { SetValue(OnScrollActionProperty, value); }
    }

    public static readonly StyledProperty<Action> OnScrollActionProperty =
        AvaloniaProperty.Register<SizeInput, Action>(nameof(OnScrollAction));

    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public double MinSize
    {
        get => GetValue(MinSizeProperty);
        set => SetValue(MinSizeProperty, value);
    }

    public double MaxSize
    {
        get => (int)GetValue(MaxSizeProperty);
        set => SetValue(MaxSizeProperty, value);
    }

    public bool BehaveLikeSmallEmbeddedField
    {
        get => (bool)GetValue(BehaveLikeSmallEmbeddedFieldProperty);
        set => SetValue(BehaveLikeSmallEmbeddedFieldProperty, value);
    }

    public SizeInput()
    {
        InitializeComponent();
    }

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        if (input.IsFocused) return;
        FocusAndSelect();
    }

    public void FocusAndSelect()
    {
        input.Focus();
        input.SelectAll();
    }

    private void Border_MouseLeftButtonDown(object? sender, PointerPressedEventArgs e)
    {
        /*Point pos = Mouse.GetPosition(textBox);
        int charIndex = textBox.GetCharacterIndexFromPoint(pos, true);
        var charRect = textBox.GetRectFromCharacterIndex(charIndex);
        double middleX = (charRect.Left + charRect.Right) / 2;
        if (pos.X > middleX)
            textBox.CaretIndex = charIndex + 1;
        else
            textBox.CaretIndex = charIndex;*/
        //TODO: Above functions not found in Avalonia
        input.SelectAll();
        e.Handled = true;
        if (!input.IsFocused)
            input.Focus();
    }

    public string Unit
    {
        get => (string)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    private void Border_MouseWheel(object? sender, PointerWheelEventArgs e)
    {
        int step = (int)e.Delta.Y / 100;

        if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            Size += step * 2;
        }
        else if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            if (step < 0)
            {
                Size /= 2;
            }
            else
            {
                Size *= 2;
            }
        }
        else
        {
            Size += step;
        }

        OnScrollAction?.Invoke();
    }
}
