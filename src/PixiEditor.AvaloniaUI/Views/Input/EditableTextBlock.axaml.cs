using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using PixiEditor.AvaloniaUI.Models.Controllers;

namespace PixiEditor.AvaloniaUI.Views.Input;

internal partial class EditableTextBlock : UserControl
{
    public static readonly StyledProperty<bool> TextBlockVisibilityProperty =
        AvaloniaProperty.Register<EditableTextBlock, bool>(
            nameof(TextBlockVisibility), true);


    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<EditableTextBlock, string>(
            nameof(Text));


    public static readonly StyledProperty<bool> EnableEditingProperty =
        AvaloniaProperty.Register<EditableTextBlock, bool>(
            nameof(IsEditing));

    public int MaxChars
    {
        get { return (int)GetValue(MaxCharsProperty); }
        set { SetValue(MaxCharsProperty, value); }
    }


    public static readonly StyledProperty<int> MaxCharsProperty =
        AvaloniaProperty.Register<EditableTextBlock, int>(nameof(MaxChars), int.MaxValue);

    public static readonly StyledProperty<SolidColorBrush> ForegroundProperty =
        AvaloniaProperty.Register<EditableTextBlock, SolidColorBrush>(
        nameof(Foreground), new SolidColorBrush(Brushes.White.Color));

    public SolidColorBrush Foreground
    {
        get { return (SolidColorBrush)GetValue(ForegroundProperty); }
        set { SetValue(ForegroundProperty, value); }
    }

    public event EventHandler<TextChangedEventArgs> OnSubmit;

    static EditableTextBlock()
    {
        EnableEditingProperty.Changed.Subscribe(OnIsEditingChanged);
    }

    public EditableTextBlock()
    {
        InitializeComponent();
    }

    public bool TextBlockVisibility
    {
        get => (bool)GetValue(TextBlockVisibilityProperty);
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
        TextBlockVisibility = false;
        IsEditing = true;
        //TODO: Note Previously there was a dispatcher and keyboard focus.

        textBox.Focus();
        textBox.SelectAll();
    }

    public void DisableEditing()
    {
        TextBlockVisibility = true;
        ShortcutController.UnblockShortcutExecution("EditableTextBlock");
        IsEditing = false;
        OnSubmit?.Invoke(this, new TextChangedEventArgs(textBox.Text, Text));
    }

    private static void OnIsEditingChanged(AvaloniaPropertyChangedEventArgs<bool> e)
    {
        if (e.NewValue.Value)
        {
            EditableTextBlock tb = (EditableTextBlock)e.Sender;
            tb.EnableEditing();
        }
    }

    private void TextBlock_MouseDown(object? sender, PointerPressedEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            EnableEditing();
            e.Handled = true;
        }
    }

    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            DisableEditing();
        }
    }

    private void TextBox_LostFocus(object? sender, RoutedEventArgs routedEventArgs)
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
