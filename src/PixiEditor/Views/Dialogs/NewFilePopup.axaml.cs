using Avalonia;
using Avalonia.Interactivity;

namespace PixiEditor.Views.Dialogs;

/// <summary>
///     Interaction logic for NewFilePopup.xaml.
/// </summary>
internal partial class NewFilePopup : PixiEditorPopup
{
    public static readonly StyledProperty<int> FileHeightProperty =
        AvaloniaProperty.Register<NewFilePopup, int>(nameof(FileHeight));

    public static readonly StyledProperty<int> FileWidthProperty =
        AvaloniaProperty.Register<NewFilePopup, int>(nameof(FileWidth));

    public NewFilePopup()
    {
        InitializeComponent();
        DataContext = this;
        Loaded += OnDialogShown;
    }

    private void OnDialogShown(object sender, RoutedEventArgs e)
    {
        MinWidth = Width;
        sizePicker.FocusWidthPicker();
    }

    public int FileHeight
    {
        get => (int)GetValue(FileHeightProperty);
        set => SetValue(FileHeightProperty, value);
    }

    public int FileWidth
    {
        get => (int)GetValue(FileWidthProperty);
        set => SetValue(FileWidthProperty, value);
    }
}
