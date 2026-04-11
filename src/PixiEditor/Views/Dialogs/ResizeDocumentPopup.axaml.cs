using Avalonia;
using Avalonia.Interactivity;
using Drawie.Backend.Core.Surfaces;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.Views.Dialogs;

/// <summary>
///     Interaction logic for ResizeDocumentPopup.xaml
/// </summary>
internal partial class ResizeDocumentPopup : ResizeablePopup
{
    public static readonly StyledProperty<ResamplingMethod> SamplingProperty = AvaloniaProperty.Register<ResizeDocumentPopup, ResamplingMethod>(
        nameof(Sampling));

    public ResamplingMethod Sampling
    {
        get => GetValue(SamplingProperty);
        set => SetValue(SamplingProperty, value);
    }

    public ResamplingMethod[] AllSamplingOptions => [ResamplingMethod.NearestNeighbor, ResamplingMethod.Bilinear, ResamplingMethod.Bicubic];

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
