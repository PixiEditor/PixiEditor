using Avalonia;
using PixiEditor.AvaloniaUI.ViewModels.Document;

namespace PixiEditor.AvaloniaUI.ViewModels.Dock;

internal class SwatchesDockViewModel : ViewModelBase
{
    private DocumentManagerViewModel documentManagerSubViewModel;

    public DocumentManagerViewModel DocumentManagerSubViewModel
    {
        get => documentManagerSubViewModel;
        set => SetProperty(ref documentManagerSubViewModel, value);
    }

    public SwatchesDockViewModel(DocumentManagerViewModel documentManagerViewModel)
    {
        DocumentManagerSubViewModel = documentManagerViewModel;
    }
}
