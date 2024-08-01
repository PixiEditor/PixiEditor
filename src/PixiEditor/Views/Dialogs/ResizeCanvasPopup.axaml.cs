using Avalonia;
using Avalonia.Interactivity;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.Views.Dialogs;

/// <summary>
///     Interaction logic for ResizeCanvasPopup.xaml
/// </summary>
internal partial class ResizeCanvasPopup : ResizeablePopup
{
    public static readonly StyledProperty<ResizeAnchor> SelectedAnchorPointProperty =
        AvaloniaProperty.Register<ResizeCanvasPopup, ResizeAnchor>(nameof(SelectedAnchorPoint), ResizeAnchor.TopLeft);

    public ResizeAnchor SelectedAnchorPoint
    {
        get => GetValue(SelectedAnchorPointProperty);
        set => SetValue(SelectedAnchorPointProperty, value);
    }

    public ResizeCanvasPopup()
    {
        InitializeComponent();
        Loaded += (_, _) => sizePicker.FocusWidthPicker();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Close(true);
    }
}
