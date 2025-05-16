using PixiEditor.Helpers.Converters;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Document;
using PixiEditor.ViewModels.SubViewModels;

namespace PixiEditor.ViewModels.Dock;

internal class PaletteViewerDockViewModel : DockableViewModel
{
    public override string Id => "PaletteViewer";
    public override string Title => new LocalizedString("PALETTE_TITLE");
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

        TabCustomizationSettings.Icon = PixiPerfectIcons.ToIcon(PixiPerfectIcons.ColorPalette);

    }
}
