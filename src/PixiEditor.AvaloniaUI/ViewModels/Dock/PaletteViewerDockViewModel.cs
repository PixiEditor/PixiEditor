using Avalonia;
using Dock.Model.Avalonia.Controls;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.AvaloniaUI.ViewModels.SubViewModels;

namespace PixiEditor.AvaloniaUI.ViewModels.Dock;

internal class PaletteViewerDockViewModel : Tool
{
    public static readonly StyledProperty<ColorsViewModel> ColorsViewModelProperty = AvaloniaProperty.Register<ColorPickerDockViewModel, ColorsViewModel>(
        nameof(ColorsSubViewModel));

    public ColorsViewModel ColorsSubViewModel
    {
        get => GetValue(ColorsViewModelProperty);
        set => SetValue(ColorsViewModelProperty, value);
    }

    public static readonly StyledProperty<DocumentManagerViewModel> DocumentManagerSubViewModelProperty = AvaloniaProperty.Register<PaletteViewerDockViewModel, DocumentManagerViewModel>(
        "DocumentManagerSubViewModel");

    public DocumentManagerViewModel DocumentManagerSubViewModel
    {
        get => GetValue(DocumentManagerSubViewModelProperty);
        set => SetValue(DocumentManagerSubViewModelProperty, value);
    }

    public PaletteViewerDockViewModel(ColorsViewModel colorsSubViewModel, DocumentManagerViewModel documentManagerViewModel)
    {
        ColorsSubViewModel = colorsSubViewModel;
        DocumentManagerSubViewModel = documentManagerViewModel;
    }
}
