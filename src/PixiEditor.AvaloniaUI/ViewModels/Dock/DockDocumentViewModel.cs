using Avalonia;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.AvaloniaUI.ViewModels.SubViewModels;

namespace PixiEditor.AvaloniaUI.ViewModels.Dock;

internal class DockDocumentViewModel : global::Dock.Model.Avalonia.Controls.Document
{
    public static readonly StyledProperty<ViewportWindowViewModel> ViewModelProperty =
        AvaloniaProperty.Register<DockDocumentViewModel, ViewportWindowViewModel>(nameof(ViewModel));

    public ViewportWindowViewModel ViewModel
    {
        get { return (ViewportWindowViewModel)GetValue(ViewModelProperty); }
        set { SetValue(ViewModelProperty, value); }
    }
    public DockDocumentViewModel(ViewportWindowViewModel viewportViewModel)
    {
        ViewModel = viewportViewModel;
    }
}
