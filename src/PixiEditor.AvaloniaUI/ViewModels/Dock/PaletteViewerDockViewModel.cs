using PixiEditor.AvaloniaUI.Helpers.Converters;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.AvaloniaUI.ViewModels.SubViewModels;

namespace PixiEditor.AvaloniaUI.ViewModels.Dock;

internal class PaletteViewerDockViewModel : DockableViewModel
{
    public override string Id => "PaletteViewer";
    public override string Title => "Palettes";
    public override bool CanFloat => true;
    public override bool CanClose => true;

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

        TabCustomizationSettings.Icon = ImagePathToBitmapConverter.TryLoadBitmapFromRelativePath("/Images/Dockables/PaletteViewer.png");
    }
}
