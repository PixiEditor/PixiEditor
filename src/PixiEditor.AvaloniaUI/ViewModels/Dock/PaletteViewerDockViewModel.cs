using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.AvaloniaUI.ViewModels.SubViewModels;

namespace PixiEditor.AvaloniaUI.ViewModels.Dock;

internal class PaletteViewerDockViewModel : ViewModelBase
{
    private ColorsViewModel colorsSubViewModel;
    private DocumentManagerViewModel documentManagerSubViewModel;

    public ColorsViewModel ColorsSubViewModel
    {
        get => colorsSubViewModel;
        set => SetProperty(ref colorsSubViewModel, value);
    }

    public DocumentManagerViewModel DocumentManagerSubViewModel
    {
        get => documentManagerSubViewModel;
        set => SetProperty(ref documentManagerSubViewModel, value);
    }

    public PaletteViewerDockViewModel(ColorsViewModel colorsSubViewModel, DocumentManagerViewModel documentManagerViewModel)
    {
        ColorsSubViewModel = colorsSubViewModel;
        DocumentManagerSubViewModel = documentManagerViewModel;
    }
}
