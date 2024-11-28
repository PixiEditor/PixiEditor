using Avalonia;
using Avalonia.Interactivity;

namespace PixiEditor.Views.Dialogs;

/// <summary>
/// Interaction logic for NoticePopup.xaml.
/// </summary>
internal partial class NoticePopup : PixiEditorPopup
{
    public static readonly StyledProperty<string> BodyProperty =
        AvaloniaProperty.Register<NoticePopup, string>(nameof(Body));

    public string Body
    {
        get => (string)GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    public NoticePopup()
    {
        InitializeComponent();
    }


    private void OkButton_Close(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
