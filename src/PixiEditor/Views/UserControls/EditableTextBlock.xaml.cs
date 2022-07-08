using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using PixiEditor.Models.Controllers;

namespace PixiEditor.Views.UserControls;

/// <summary>
///     Interaction logic for EditableTextBlock.xaml.
/// </summary>
internal partial class EditableTextBlock : UserControl
{

    public static readonly DependencyProperty TextBlockVisibilityProperty =
        DependencyProperty.Register(nameof(TextBlockVisibility),
            typeof(Visibility),
            typeof(EditableTextBlock),
            new PropertyMetadata(Visibility.Visible));


    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text),
            typeof(string),
            typeof(EditableTextBlock),
            new PropertyMetadata(default(string)));


    public static readonly DependencyProperty EnableEditingProperty =
        DependencyProperty.Register(nameof(IsEditing),
            typeof(bool),
            typeof(EditableTextBlock),
            new PropertyMetadata(OnIsEditingChanged));

    public int MaxChars
    {
        get { return (int)GetValue(MaxCharsProperty); }
        set { SetValue(MaxCharsProperty, value); }
    }


    public static readonly DependencyProperty MaxCharsProperty =
        DependencyProperty.Register(nameof(MaxChars), typeof(int), typeof(EditableTextBlock), new PropertyMetadata(int.MaxValue));


    public event EventHandler<TextChangedEventArgs> OnSubmit;

    public EditableTextBlock()
    {
        InitializeComponent();
    }

    public Visibility TextBlockVisibility
    {
        get => (Visibility)GetValue(TextBlockVisibilityProperty);
        set => SetValue(TextBlockVisibilityProperty, value);
    }

    public bool IsEditing
    {
        get => (bool)GetValue(EnableEditingProperty);
        set => SetValue(EnableEditingProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public void EnableEditing()
    {
        ShortcutController.BlockShortcutExecution("EditableTextBlock");
        TextBlockVisibility = Visibility.Hidden;
        IsEditing = true;
        Dispatcher.BeginInvoke(
            DispatcherPriority.Input,
            new Action(delegate ()
            {
                textBox.Focus();         // Set Logical Focus
                Keyboard.Focus(textBox); // Set Keyboard Focus
            }));
        textBox.SelectAll();
    }

    public void DisableEditing()
    {
        TextBlockVisibility = Visibility.Visible;
        ShortcutController.UnblockShortcutExecution("EditableTextBlock");
        IsEditing = false;
        OnSubmit?.Invoke(this, new TextChangedEventArgs(textBox.Text, Text));
    }

    private static void OnIsEditingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if ((bool)e.NewValue)
        {
            EditableTextBlock tb = (EditableTextBlock)d;
            tb.EnableEditing();
        }
    }

    private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            EnableEditing();
        }
    }

    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            DisableEditing();
        }
    }

    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        DisableEditing();
    }

    private void TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        DisableEditing();
    }

    internal class TextChangedEventArgs : EventArgs
    {
        public string NewText { get; set; }

        public string OldText { get; set; }

        public TextChangedEventArgs(string newText, string oldText)
        {
            NewText = newText;
            OldText = oldText;
        }
    }
}
