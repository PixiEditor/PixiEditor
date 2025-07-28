using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using PixiEditor.Models.Input;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels;

namespace PixiEditor.Views.Shortcuts;

/// <summary>
/// Interaction logic for KeyCombinationBox.xaml
/// </summary>
internal partial class KeyCombinationBox : UserControl
{
    private KeyCombination currentCombination;
    private bool ignoreButtonPress;

    public static readonly StyledProperty<KeyCombination> KeyCombinationProperty =
        AvaloniaProperty.Register<KeyCombinationBox, KeyCombination>(
            nameof(KeyCombination));

    public KeyCombination KeyCombination
    {
        get => GetValue(KeyCombinationProperty);
        set => SetValue(KeyCombinationProperty, value);
    }

    public event EventHandler<KeyCombination> KeyCombinationChanged;

    public static readonly StyledProperty<KeyCombination> DefaultCombinationProperty =
        AvaloniaProperty.Register<KeyCombinationBox, KeyCombination>(
            nameof(DefaultCombination));

    public KeyCombination DefaultCombination
    {
        get => GetValue(DefaultCombinationProperty);
        set => SetValue(DefaultCombinationProperty, value);
    }

    static KeyCombinationBox()
    {
        KeyCombinationProperty.Changed.Subscribe(CombinationUpdate);
        DefaultCombinationProperty.Changed.Subscribe(DefaultCombinationUpdate);
    }

    public KeyCombinationBox()
    {
        InitializeComponent();

        UpdateText();
        UpdateButton();

        ViewModelMain.Current.LocalizationProvider.OnLanguageChanged += _ => UpdateText();

        //TODO: Fix
        //InputLanguageManager.Current.InputLanguageChanged += (_, _) => UpdateText();
    }

    private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = true;

        if (GetModifier(e.Key) is { } modifier)
        {
            currentCombination = new(currentCombination.Key, currentCombination.Modifiers | modifier);
        }
        else
        {
            currentCombination = new(e.Key, currentCombination.Modifiers);
        }

        UpdateText();
        UpdateButton();
    }

    private void TextBox_PreviewKeyUp(object sender, KeyEventArgs e)
    {
        e.Handled = true;

        if (GetModifier(e.Key) is { } modifier)
        {
            currentCombination = currentCombination with { Modifiers = currentCombination.Modifiers ^ modifier };
        }
        else
        {
            KeyCombination = currentCombination with { Key = e.Key };
            focusGrid.Focus();
        }

        UpdateButton();
        UpdateText();
    }

    private void TextBox_GotKeyboardFocus(object sender, GotFocusEventArgs e)
    {
        currentCombination = new();
        textBox.Text = new LocalizedString("PRESS_ANY_KEY");
        UpdateButton();
    }

    private void TextBox_LostKeyboardFocus(object sender, RoutedEventArgs e)
    {
        ignoreButtonPress = TopLevel.GetTopLevel(this).FocusManager.GetFocusedElement() == button;
        currentCombination = KeyCombination;

        UpdateText();
        UpdateButton();
        focusGrid.Focus();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (ignoreButtonPress)
        {
            ignoreButtonPress = false;
            return;
        }

        if (KeyCombination != DefaultCombination)
        {
            KeyCombination = DefaultCombination;
        }
        else
        {
            KeyCombination = default;
        }
    }

    private void UpdateText() => textBox.Text = currentCombination != default
        ? currentCombination.ToString()
        : new LocalizedString("NONE_SHORTCUT");

    private void UpdateButton()
    {
        //TODO: Maybe make better icons
        if (textBox.IsFocused)
        {
            button.IsEnabled = true;
            button.Content = "\u2715";
        }
        else if (KeyCombination != DefaultCombination)
        {
            button.IsEnabled = true;
            button.Content = "\u2190";
        }
        else
        {
            button.IsEnabled = KeyCombination != default;
            button.Content = "―";
        }
    }

    private static void CombinationUpdate(AvaloniaPropertyChangedEventArgs<KeyCombination> e)
    {
        var box = (KeyCombinationBox)e.Sender;

        box.currentCombination = box.KeyCombination;
        box.textBox.Text = box.KeyCombination.ToString();
        box.KeyCombinationChanged.Invoke(box, box.currentCombination);

        box.UpdateText();
        box.UpdateButton();
    }

    private static void DefaultCombinationUpdate(AvaloniaPropertyChangedEventArgs<KeyCombination> e)
    {
        var box = (KeyCombinationBox)e.Sender;
        box.UpdateButton();
    }

    private static KeyModifiers? GetModifier(Key key) => key switch
    {
        Key.LeftCtrl or Key.RightCtrl => KeyModifiers.Control,
        Key.LeftAlt or Key.RightAlt => KeyModifiers.Alt,
        Key.LeftShift or Key.RightShift => KeyModifiers.Shift,
        Key.LWin or Key.RWin => KeyModifiers.Meta,
        _ => null
    };
}
