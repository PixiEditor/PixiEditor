using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using PixiEditor.Models.Controllers;

namespace PixiEditor.Views.Input;

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

        Dispatcher.UIThread.Post(
            () =>
            {
                textBox.Focus();
                textBox.SelectAll();
            }, DispatcherPriority.Input);
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

    private void OnDoubleTapped(object sender, TappedEventArgs e)
    {
        EnableEditing();
        e.Handled = true;
    }

    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is Key.Enter or Key.Escape)
        {
            DisableEditing();
            e.Handled = true;
            return;
        }

        e.Handled = e.Key is Key.Left or Key.Right;
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
