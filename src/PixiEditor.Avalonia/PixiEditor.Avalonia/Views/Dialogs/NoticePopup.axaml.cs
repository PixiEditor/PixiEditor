using System.Windows;
using Avalonia;
using Avalonia.Interactivity;

namespace PixiEditor.Views.Dialogs;

/// <summary>
/// Interaction logic for NoticePopup.xaml.
/// </summary>
internal partial class NoticePopup : Window
{
    public static readonly StyledProperty<string> BodyProperty =
        AvaloniaProperty.Register<NoticePopup, string>(nameof(Body));

    public new string Title
    {
        get => base.Title;
        set => base.Title = value;
    }

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
