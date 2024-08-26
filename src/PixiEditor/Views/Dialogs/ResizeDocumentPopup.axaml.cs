using Avalonia.Interactivity;

namespace PixiEditor.Views.Dialogs;

/// <summary>
///     Interaction logic for ResizeDocumentPopup.xaml
/// </summary>
internal partial class ResizeDocumentPopup : ResizeablePopup
{
    public ResizeDocumentPopup()
    {
        InitializeComponent();
        DataContext = this;
        Loaded += (_, _) => sizePicker.FocusWidthPicker();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Close(true);
    }
}
