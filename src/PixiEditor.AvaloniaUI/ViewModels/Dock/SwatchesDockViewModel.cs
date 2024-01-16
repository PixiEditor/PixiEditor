using System.ComponentModel;
using Avalonia;
using Dock.Model.Avalonia.Controls;
using PixiEditor.AvaloniaUI.ViewModels.Document;

namespace PixiEditor.AvaloniaUI.ViewModels.Dock;

internal class SwatchesDockViewModel : Tool
{
    public static readonly StyledProperty<DocumentManagerViewModel> DocumentManagerSubViewModelProperty = AvaloniaProperty.Register<PaletteViewerDockViewModel, DocumentManagerViewModel>(
        "DocumentManagerSubViewModel");

    public DocumentManagerViewModel DocumentManagerSubViewModel
    {
        get => GetValue(DocumentManagerSubViewModelProperty);
        set => SetValue(DocumentManagerSubViewModelProperty, value);
    }

    public SwatchesDockViewModel(DocumentManagerViewModel documentManagerViewModel)
    {
        DocumentManagerSubViewModel = documentManagerViewModel;
    }
}
