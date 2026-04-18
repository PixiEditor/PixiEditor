using Avalonia;
using Avalonia.Interactivity;

namespace PixiEditor.Views.Dialogs;

/// <summary>
/// Interaction logic for NoticePopup.xaml.
/// </summary>
internal partial class InputPopup : PixiEditorPopup
{
    public string InputText { get; set; }
    public string Label { get; set; }

    public InputPopup()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        InputBox.Text = InputText;
        InputBox.Watermark = Label;
        InputBox.Focus();
        InputBox.CaretIndex = InputBox.Text?.Length ?? 0;
    }

    private void OkButton_Close(object? sender, RoutedEventArgs e)
    {
        InputText = InputBox.Text ?? string.Empty;
        SetResultAndCloseCommand();
    }
}
