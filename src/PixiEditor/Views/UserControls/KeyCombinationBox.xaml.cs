using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Behaviours;
using PixiEditor.Localization;
using PixiEditor.Models.DataHolders;

namespace PixiEditor.Views.UserControls;

/// <summary>
/// Interaction logic for KeyCombinationBox.xaml
/// </summary>
internal partial class KeyCombinationBox : UserControl
{
    private KeyCombination currentCombination;
    private bool ignoreButtonPress;

    public static readonly DependencyProperty KeyCombinationProperty =
        DependencyProperty.Register(nameof(KeyCombination), typeof(KeyCombination), typeof(KeyCombinationBox), new PropertyMetadata(CombinationUpdate));

    public event EventHandler<KeyCombination> KeyCombinationChanged;

    public KeyCombination KeyCombination
    {
        get => (KeyCombination)GetValue(KeyCombinationProperty);
        set => SetValue(KeyCombinationProperty, value);
    }

    public static readonly DependencyProperty DefaultCombinationProperty =
        DependencyProperty.Register(nameof(DefaultCombination), typeof(KeyCombination), typeof(KeyCombinationBox), new PropertyMetadata(DefaultCombinationUpdate));

    public KeyCombination DefaultCombination
    {
        get => (KeyCombination)GetValue(DefaultCombinationProperty);
        set => SetValue(DefaultCombinationProperty, value);
    }

    public KeyCombinationBox()
    {
        InitializeComponent();

        UpdateText();
        UpdateButton();

        ViewModelMain.Current.LocalizationProvider.OnLanguageChanged += _ => UpdateText();
        InputLanguageManager.Current.InputLanguageChanged += (_, _) => UpdateText();
    }

    private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = true;

        if (GetModifier(e.Key == Key.System ? e.SystemKey : e.Key) is ModifierKeys modifier)
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

        if (GetModifier((e.Key == Key.System ? e.SystemKey : e.Key)) is ModifierKeys modifier)
        {
            currentCombination = new(currentCombination.Key, currentCombination.Modifiers ^ modifier);
            UpdateText();
        }
        else
        {
            KeyCombination = new(e.Key, currentCombination.Modifiers);
            Keyboard.ClearFocus();
        }

        UpdateButton();
    }

    private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        currentCombination = new();
        textBox.Text = new LocalizedString("PRESS_ANY_KEY");
        UpdateButton();
    }

    private void TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        ignoreButtonPress = e.NewFocus == button;
        currentCombination = KeyCombination;

        UpdateText();
        UpdateButton();
        FocusHelper.MoveFocusToParent((FrameworkElement)sender);
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

    private void UpdateText() => textBox.Text = currentCombination != default ? currentCombination.ToString() : new LocalizedString("NONE_SHORTCUT");

    private void UpdateButton()
    {
        if (textBox.IsKeyboardFocused)
        {
            button.IsEnabled = true;
            button.Content = "\uE711";
        }
        else if (KeyCombination != DefaultCombination)
        {
            button.IsEnabled = true;
            button.Content = "\uE72B";
        }
        else
        {
            button.IsEnabled = KeyCombination != default;
            button.Content = "\uE738";
        }
    }

    private static void CombinationUpdate(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        var box = (KeyCombinationBox)obj;

        box.currentCombination = box.KeyCombination;
        box.textBox.Text = box.KeyCombination.ToString();
        box.KeyCombinationChanged.Invoke(box, box.currentCombination);

        box.UpdateText();
        box.UpdateButton();
    }

    private static void DefaultCombinationUpdate(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        var box = (KeyCombinationBox)obj;
        box.UpdateButton();
    }

    private static ModifierKeys? GetModifier(Key key) => key switch
    {
        Key.LeftCtrl or Key.RightCtrl => ModifierKeys.Control,
        Key.LeftAlt or Key.RightAlt => ModifierKeys.Alt,
        Key.LeftShift or Key.RightShift => ModifierKeys.Shift,
        _ => null
    };
}
