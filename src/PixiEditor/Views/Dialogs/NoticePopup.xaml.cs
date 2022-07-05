using System.Windows;

namespace PixiEditor.Views.Dialogs;

/// <summary>
/// Interaction logic for NoticePopup.xaml.
/// </summary>
internal partial class NoticePopup : Window
{
    public static readonly DependencyProperty BodyProperty =
        DependencyProperty.Register(nameof(Body), typeof(string), typeof(NoticePopup));

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

    private void OkButton_Close(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
